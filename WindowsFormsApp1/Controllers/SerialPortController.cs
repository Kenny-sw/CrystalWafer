using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    public class SerialPortController : IDisposable
    {
        private readonly SerialPort _port;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _commandTimeout = Protocol.Timeouts.Command;

        private TaskCompletionSource<CommandResult> _pendingCommand;
        private readonly object _pendingLock = new object();

        private CancellationTokenSource _listenerCts;
        private Task _listenerTask;

        public event Action<string> UnsolicitedEventReceived;
        public event Action<bool, string> ConnectionStateChanged;

        public SerialPortController(SerialPort serialPort)
        {
            _port = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
        }

        public void ToggleConnection(string portName, Button btnConnect, ComboBox portsCombo)
        {
            try
            {
                if (_port.IsOpen)
                {
                    AppLogger.Info($"Closing COM port {_port.PortName}.");
                    StopListener();

                    _port.Close();
                    if (btnConnect != null)
                    {
                        btnConnect.Text = "Connect";
                    }

                    ConnectionStateChanged?.Invoke(false, null);
                    return;
                }

                if (string.IsNullOrWhiteSpace(portName))
                {
                    AppLogger.Warning("COM port name is empty.");
                    MessageBox.Show("Select a COM port.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ConfigurePort(portName.Trim());
                _port.Open();

                StartListener();

                if (btnConnect != null)
                {
                    btnConnect.Text = "Disconnect";
                }

                AppLogger.Info($"COM port opened: {_port.PortName}");
                ConnectionStateChanged?.Invoke(true, _port.PortName);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Failed to toggle serial port connection.", ex);
                StopListener();

                if (_port.IsOpen)
                {
                    try { _port.Close(); }
                    catch { }
                }

                MessageBox.Show("Failed to communicate with the COM port: " + ex.Message, "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ConnectionStateChanged?.Invoke(false, null);
            }
        }

        private void ConfigurePort(string portName)
        {
            _port.BaudRate = 115200;
            _port.Parity = Parity.None;
            _port.DataBits = 8;
            _port.StopBits = StopBits.One;
            _port.Handshake = Handshake.None;
            _port.PortName = portName;
            _port.NewLine = "\r\n";
        }

        public void UpdatePortList(ComboBox combo)
        {
            if (combo == null)
            {
                return;
            }

            var ports = SerialPort.GetPortNames()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            AppLogger.Info($"Available COM ports: {string.Join(", ", ports)}");

            combo.Items.Clear();
            combo.Items.AddRange(ports);
            if (ports.Length > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        public async Task<bool> SendCommandAsync(byte command, uint data)
        {
            if (_port == null || !_port.IsOpen)
            {
                AppLogger.Warning($"Attempt to send 0x{command:X2} while port is closed.");
                return false;
            }

            await _sendLock.WaitAsync().ConfigureAwait(false);

            try
            {
                var pending = new TaskCompletionSource<CommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);

                lock (_pendingLock)
                {
                    if (_pendingCommand != null)
                    {
                        AppLogger.Warning($"Command 0x{command:X2} rejected: previous command still pending.");
                        return false;
                    }

                    _pendingCommand = pending;
                }

                var packet = BuildPacket(command, data);
                AppLogger.Debug($"TX -> cmd=0x{command:X2}, data={data}");

                try
                {
                    await _port.BaseStream.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
                    await _port.BaseStream.FlushAsync().ConfigureAwait(false);
                }
                catch (Exception writeError)
                {
                    AppLogger.Error("Failed to write to serial port.", writeError);
                    ClearPending(pending, "Write failure");
                    return false;
                }

                var completedTask = await Task.WhenAny(pending.Task, Task.Delay(_commandTimeout)).ConfigureAwait(false);
                if (completedTask != pending.Task)
                {
                    AppLogger.Warning("Serial command timed out.");
                    ClearPending(pending, "Timeout");
                    return false;
                }

                var result = await pending.Task.ConfigureAwait(false);
                AppLogger.Debug($"RX <- {(result.Success ? Protocol.Responses.Ok : Protocol.Responses.ErrorPrefix)}: {result.Message}");
                return result.Success;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private static byte[] BuildPacket(byte command, uint data)
        {
            var packet = new byte[6];
            packet[0] = command;
            packet[1] = (byte)(data & 0xFF);
            packet[2] = (byte)((data >> 8) & 0xFF);
            packet[3] = (byte)((data >> 16) & 0xFF);
            packet[4] = (byte)((data >> 24) & 0xFF);
            packet[5] = CalculateChecksum(packet, 5);
            return packet;
        }

        private static byte CalculateChecksum(byte[] buffer, int length)
        {
            int sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum = (sum + buffer[i]) & 0xFF;
            }

            return (byte)sum;
        }

        private void StartListener()
        {
            StopListener();

            _listenerCts = new CancellationTokenSource();
            var token = _listenerCts.Token;
            _listenerTask = Task.Run(() => ListenAsync(token), token);
            AppLogger.Debug("Serial listener started.");
        }

        private void StopListener()
        {
            if (_listenerCts == null)
            {
                return;
            }

            try
            {
                _listenerCts.Cancel();
                _listenerTask?.Wait(500);
            }
            catch (AggregateException ex)
            {
                AppLogger.Debug($"Serial listener stop aggregate exception: {ex.Flatten().Message}");
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation triggers while awaiting.
            }
            finally
            {
                _listenerCts.Dispose();
                _listenerCts = null;
                _listenerTask = null;
            }

            FailPendingCommand("Listener stopped");
            AppLogger.Debug("Serial listener stopped.");
        }

        private async Task ListenAsync(CancellationToken token)
        {
            try
            {
                var encoding = _port.Encoding ?? Encoding.ASCII;
                using (var reader = new StreamReader(_port.BaseStream, encoding, false, 1024, leaveOpen: true))
                {
                    while (!token.IsCancellationRequested)
                    {
                        string line;

                        try
                        {
                            var readTask = reader.ReadLineAsync();
                            var completed = await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, token)).ConfigureAwait(false);
                            if (completed != readTask)
                            {
                                return;
                            }

                            line = await readTask.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (IOException ioEx)
                        {
                            AppLogger.Warning("Serial port read error.", ioEx);
                            break;
                        }
                        catch (InvalidOperationException invalidEx)
                        {
                            AppLogger.Warning("Serial port became unavailable.", invalidEx);
                            break;
                        }

                        if (line == null)
                        {
                            break;
                        }

                        line = line.Trim();
                        if (line.Length == 0)
                        {
                            continue;
                        }

                        AppLogger.Trace($"RX raw: {line}");

                        if (Protocol.Events.TryParseSensorEvent(line, out bool isSensorOn))
                        {
                            AppLogger.Info($"Sensor event: {(isSensorOn ? "ON" : "OFF")}");
                            UnsolicitedEventReceived?.Invoke(line);
                            continue;
                        }

                        if (Protocol.Events.IsEvent(line))
                        {
                            AppLogger.Debug($"Unsolicited event: {line}");
                            UnsolicitedEventReceived?.Invoke(line);
                            continue;
                        }

                        if (Protocol.Responses.IsOk(line))
                        {
                            CompletePendingCommand(true, line);
                            continue;
                        }

                        if (Protocol.Responses.IsError(line))
                        {
                            CompletePendingCommand(false, line);
                            continue;
                        }

                        AppLogger.Warning($"Unknown serial response: {line}");
                    }
                }
            }
            finally
            {
                FailPendingCommand("Listener stopped");
            }
        }

        private void CompletePendingCommand(bool success, string message)
        {
            TaskCompletionSource<CommandResult> pending;
            lock (_pendingLock)
            {
                pending = _pendingCommand;
                _pendingCommand = null;
            }

            pending?.TrySetResult(new CommandResult(success, message));
        }

        private void FailPendingCommand(string reason)
        {
            TaskCompletionSource<CommandResult> pending;
            lock (_pendingLock)
            {
                pending = _pendingCommand;
                _pendingCommand = null;
            }

            if (pending != null)
            {
                AppLogger.Warning($"Pending command failed: {reason}");
                pending.TrySetResult(new CommandResult(false, reason));
            }
        }

        private void ClearPending(TaskCompletionSource<CommandResult> expected, string reason)
        {
            TaskCompletionSource<CommandResult> pending = null;
            lock (_pendingLock)
            {
                if (_pendingCommand == expected)
                {
                    pending = _pendingCommand;
                    _pendingCommand = null;
                }
            }

            if (pending != null)
            {
                AppLogger.Warning($"Pending command cleared: {reason}");
                pending.TrySetResult(new CommandResult(false, reason));
            }
        }

        public void Dispose()
        {
            StopListener();

            try
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }

                _port.Dispose();
            }
            catch (Exception disposingError)
            {
                AppLogger.Warning("Error while disposing serial port controller.", disposingError);
            }
        }

        private class CommandResult
        {
            public CommandResult(bool success, string message)
            {
                Success = success;
                Message = message;
            }

            public bool Success { get; }
            public string Message { get; }
        }
    }
}
