using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrystalTable.Controllers
{
    public class SerialPortController : IDisposable
    {
        private readonly SerialPort _port;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _commandTimeout = TimeSpan.FromSeconds(3);

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
                    StopListener();
                    _port.Close();
                    if (btnConnect != null) btnConnect.Text = "Подключить";
                    ConnectionStateChanged?.Invoke(false, null);
                    return;
                }

                if (string.IsNullOrWhiteSpace(portName))
                {
                    MessageBox.Show("Выберите COM-порт.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _port.BaudRate = 115200;
                _port.Parity = Parity.None;
                _port.DataBits = 8;
                _port.StopBits = StopBits.One;
                _port.Handshake = Handshake.None;
                _port.PortName = portName;
                _port.NewLine = "\r\n";

                _port.Open();
                StartListener();

                if (btnConnect != null) btnConnect.Text = "Отключить";
                ConnectionStateChanged?.Invoke(true, _port.PortName);
            }
            catch (Exception ex)
            {
                StopListener();

                if (_port.IsOpen)
                {
                    try { _port.Close(); } catch { }
                }

                MessageBox.Show("Не удалось открыть порт: " + ex.Message, "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ConnectionStateChanged?.Invoke(false, null);
            }
        }

        public void UpdatePortList(ComboBox combo)
        {
            if (combo == null) return;
            var list = SerialPort.GetPortNames().OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToArray();
            combo.Items.Clear();
            combo.Items.AddRange(list);
            if (list.Length > 0) combo.SelectedIndex = 0;
        }

        public async Task<bool> SendCommandAsync(byte command, uint data)
        {
            if (_port == null || !_port.IsOpen)
            {
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
                        return false;
                    }

                    _pendingCommand = pending;
                }

                byte[] packet = BuildPacket(command, data);

                try
                {
                    await _port.BaseStream.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
                    await _port.BaseStream.FlushAsync().ConfigureAwait(false);
                }
                catch
                {
                    ClearPending(pending);
                    return false;
                }

                var completed = await Task.WhenAny(pending.Task, Task.Delay(_commandTimeout)).ConfigureAwait(false);
                if (completed != pending.Task)
                {
                    ClearPending(pending);
                    return false;
                }

                var result = await pending.Task.ConfigureAwait(false);
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
            catch { }
            finally
            {
                _listenerCts.Dispose();
                _listenerCts = null;
                _listenerTask = null;
            }

            FailPendingCommand("Порт закрыт");
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
                        catch (IOException)
                        {
                            break;
                        }
                        catch (InvalidOperationException)
                        {
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

                        if (line.StartsWith("EV", StringComparison.OrdinalIgnoreCase))
                        {
                            UnsolicitedEventReceived?.Invoke(line);
                            continue;
                        }

                        if (string.Equals(line, "OK", StringComparison.OrdinalIgnoreCase))
                        {
                            CompletePendingCommand(true, line);
                            continue;
                        }

                        if (line.StartsWith("ERR", StringComparison.OrdinalIgnoreCase))
                        {
                            CompletePendingCommand(false, line);
                            continue;
                        }
                    }
                }
            }
            finally
            {
                FailPendingCommand("Связь потеряна");
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

            pending?.TrySetResult(new CommandResult(false, reason));
        }

        private void ClearPending(TaskCompletionSource<CommandResult> expected)
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

            pending?.TrySetResult(new CommandResult(false, "Не удалось отправить"));
        }

        public void Dispose()
        {
            StopListener();

            try
            {
                if (_port != null)
                {
                    if (_port.IsOpen) _port.Close();
                    _port.Dispose();
                }
            }
            catch { /* ignore */ }
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
