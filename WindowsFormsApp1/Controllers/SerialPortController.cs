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
                
                // ✅ ИСПРАВЛЕНО: Открываем порт и даем время на инициализацию
                _port.Open();
                AppLogger.Info($"COM port opened: {_port.PortName}");
                
                // ✅ Устанавливаем DTR после открытия для стабильной работы
                _port.DtrEnable = true;
                _port.RtsEnable = true;
                
                // ✅ Очищаем буферы перед запуском listener
                if (_port.BytesToRead > 0)
                {
                    AppLogger.Debug($"Очистка входного буфера: {_port.BytesToRead} байт");
                    _port.DiscardInBuffer();
                }
                if (_port.BytesToWrite > 0)
                {
                    AppLogger.Debug($"Очистка выходного буфера: {_port.BytesToWrite} байт");
                    _port.DiscardOutBuffer();
                }
                
                // ✅ Даем Arduino время на стабилизацию (без перезагрузки, т.к. DTR был false при открытии)
                System.Threading.Thread.Sleep(100);

                StartListener();

                if (btnConnect != null)
                {
                    btnConnect.Text = "Disconnect";
                }

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
            
            // ✅ КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Arduino использует \n, не \r\n!
            _port.NewLine = "\n";  // Было: "\r\n"
            
            // ✅ ИСПРАВЛЕНО: Управление DTR и RTS
            // DTR = false предотвращает автоматический reset Arduino при подключении
            // Установим в true после открытия порта для стабильной работы
            _port.DtrEnable = false;
            _port.RtsEnable = false;
            
            // ✅ Таймауты для операций чтения/записи
            _port.ReadTimeout = 5000;  // 5 секунд
            _port.WriteTimeout = 1000; // 1 секунда
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

            // ✅ ИСПРАВЛЕНО: Проверяем listener, но НЕ перезапускаем автоматически
            if (_listenerTask == null || _listenerTask.IsCompleted)
            {
                AppLogger.Error($"Listener не запущен или завершился!");
                AppLogger.Error($"Необходимо переподключить COM-порт (Disconnect → Connect)");
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
                AppLogger.Debug($"TX -> cmd=0x{command:X2}, data={data}, packet=[{BitConverter.ToString(packet)}]");

                try
                {
                    await _port.BaseStream.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
                    await _port.BaseStream.FlushAsync().ConfigureAwait(false);
                    AppLogger.Debug($"Пакет команды 0x{command:X2} отправлен, ожидание ответа...");
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
                    AppLogger.Warning($"Serial command 0x{command:X2} timed out after {_commandTimeout.TotalSeconds} seconds.");
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
            byte checksum = 0;
            for (int i = 0; i < length; i++)
            {
                checksum ^= buffer[i];  // XOR вместо суммы
            }

            return checksum;
        }

        private void StartListener()
        {
            StopListener();

            // ✅ ИСПРАВЛЕНО: Проверка состояния порта перед запуском
            if (!_port.IsOpen)
            {
                AppLogger.Warning("Cannot start listener: port is not open");
                return;
            }

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
                
                // ✅ ИСПРАВЛЕНО: Даем больше времени на остановку
                if (_listenerTask != null && !_listenerTask.Wait(1000))
                {
                    AppLogger.Warning("Listener did not stop within timeout");
                }
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
                _listenerCts?.Dispose();
                _listenerCts = null;
                _listenerTask = null;
            }

            FailPendingCommand("Listener stopped");
            AppLogger.Debug("Serial listener stopped.");
        }

        private async Task ListenAsync(CancellationToken token)
        {
            StreamReader reader = null;
            try
            {
                // ✅ ИСПРАВЛЕНО: Очистка буферов перед стартом
                if (_port.IsOpen && _port.BytesToRead > 0)
                {
                    AppLogger.Debug($"Очистка буфера приема: {_port.BytesToRead} байт");
                    _port.DiscardInBuffer();
                }
                
                var encoding = _port.Encoding ?? Encoding.ASCII;
                reader = new StreamReader(_port.BaseStream, encoding, false, 1024, leaveOpen: true);
                
                while (!token.IsCancellationRequested && _port.IsOpen)
                {
                    string line;

                    try
                    {
                        // ✅ ИСПРАВЛЕНО: Используем token для отмены
                        line = await reader.ReadLineAsync().ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        AppLogger.Debug("Listener: operation cancelled");
                        return;
                    }
                    catch (IOException ioEx)
                    {
                        // ✅ Проверяем, не закрыт ли порт
                        if (!_port.IsOpen)
                        {
                            AppLogger.Debug("Listener: port closed");
                            return;
                        }
                        
                        AppLogger.Warning("Serial port read error.", ioEx);
                        
                        // ✅ Пытаемся переподключиться через короткую задержку
                        await Task.Delay(100, token).ConfigureAwait(false);
                        continue;
                    }
                    catch (InvalidOperationException invalidEx)
                    {
                        AppLogger.Warning("Serial port became unavailable.", invalidEx);
                        break;
                    }

                    if (line == null)
                    {
                        // ✅ Конец потока - порт закрыт
                        AppLogger.Debug("Listener: end of stream");
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
          
                    // ✅ НОВОЕ: Обработка ответа "PSET" (профиль установлен)
                    if (Protocol.Responses.IsProfileSet(line))
                    {
                        AppLogger.Info("Arduino подтвердил установку профиля");
                        CompletePendingCommand(true, line);
                        continue;
                    }

                    // ✅ НОВОЕ: Обработка ответа "PROFILE:..." (данные профиля)
                    if (Protocol.Responses.IsProfileData(line))
                    {
                        AppLogger.Debug($"Получены данные профиля от Arduino: {line}");
                          // TODO: Парсинг данных профиля и событие
                        CompletePendingCommand(true, line);
                        continue;
                    }

                    AppLogger.Warning($"Unknown serial response: {line}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Listener crashed", ex);
            }
            finally
            {
                // ✅ ИСПРАВЛЕНО: Корректное закрытие reader
                try
                {
                    reader?.Dispose();
                }
                catch (Exception disposeEx)
                {
                    AppLogger.Debug($"Error disposing reader: {disposeEx.Message}");
                }
                
                FailPendingCommand("Listener stopped");
                AppLogger.Debug("Listener finished");
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
