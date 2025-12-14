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
    /// <summary>
    /// Упрощённый и надёжный контроллер последовательного порта.
    /// Особенности:
    /// - Polling вместо async для надёжности
    /// - Автоматический retry при сбоях
    /// - Защита от переполнения буфера
    /// - Синхронизация после сбоев
    /// </summary>
    public class SerialPortController : IDisposable
    {
        private readonly SerialPort _port;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        
        // ✅ Настраиваемые параметры
        private const int COMMAND_TIMEOUT_MS = 10000;      // 10 секунд на команду (длинные перемещения)
        private const int READ_POLL_INTERVAL_MS = 5;       // Интервал опроса буфера
        private const int MAX_RETRIES = 2;                 // Количество повторов при сбое
        private const int RETRY_DELAY_MS = 100;            // Задержка между повторами
        private const int STARTUP_DELAY_MS = 2000;         // Время на старт Arduino
        private const int MAX_LINE_BUFFER_SIZE = 1024;     // Макс. размер буфера строки

        private TaskCompletionSource<CommandResult> _pendingCommand;
        private readonly object _pendingLock = new object();

        private CancellationTokenSource _listenerCts;
        private Task _listenerTask;
        private volatile bool _isListenerRunning;

        public event Action<string> UnsolicitedEventReceived;
        public event Action<bool, string> ConnectionStateChanged;
        public event Action<string> ProfileDataReceived;

        // ✅ Состояние подключения
        public bool IsConnected => _port?.IsOpen == true && _isListenerRunning;
        public string PortName => _port?.PortName;

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
                    Disconnect();
                    if (btnConnect != null) btnConnect.Text = "Connect";
                    ConnectionStateChanged?.Invoke(false, null);
                    return;
                }

                if (string.IsNullOrWhiteSpace(portName))
                {
                    MessageBox.Show("Выберите COM порт.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (Connect(portName.Trim()))
                {
                    if (btnConnect != null) btnConnect.Text = "Disconnect";
                    ConnectionStateChanged?.Invoke(true, _port.PortName);
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к COM порту.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ConnectionStateChanged?.Invoke(false, null);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка переключения COM порта", ex);
                Disconnect();
                MessageBox.Show($"Ошибка: {ex.Message}", "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ConnectionStateChanged?.Invoke(false, null);
            }
        }

        /// <summary>
        /// Подключение к порту с ожиданием готовности Arduino
        /// </summary>
        private bool Connect(string portName)
        {
            try
            {
                AppLogger.Info($"Подключение к {portName}...");

                // ✅ 1. Конфигурируем порт
                _port.PortName = portName;
                _port.BaudRate = 115200;
                _port.Parity = Parity.None;
                _port.DataBits = 8;
                _port.StopBits = StopBits.One;
                _port.Handshake = Handshake.None;
                _port.NewLine = "\n";
                _port.ReadTimeout = 1000;
                _port.WriteTimeout = 1000;
                
                // ✅ 2. DTR=false ПЕРЕД открытием - предотвращает reset Arduino
                _port.DtrEnable = false;
                _port.RtsEnable = false;

                // ✅ 3. Открываем порт
                _port.Open();
                AppLogger.Info($"Порт {portName} открыт");

                // ✅ 4. Включаем DTR/RTS для стабильной работы
                Thread.Sleep(50);
                _port.DtrEnable = true;
                _port.RtsEnable = true;

                // ✅ 5. Очищаем буферы
                Thread.Sleep(50);
                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();

                // ✅ 6. Ждём READY от Arduino
                AppLogger.Info($"Ожидание готовности Arduino ({STARTUP_DELAY_MS}мс)...");
                bool gotReady = WaitForReady(STARTUP_DELAY_MS);
                
                if (!gotReady)
                {
                    AppLogger.Warning("Arduino не отправил READY, но продолжаем работу");
                }

                // ✅ 7. Запускаем Listener
                StartListener();

                AppLogger.Info($"Подключение к {portName} успешно");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Ошибка подключения к {portName}", ex);
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Ожидание сообщения READY от Arduino
        /// </summary>
        private bool WaitForReady(int timeoutMs)
        {
            var deadline = DateTime.Now.AddMilliseconds(timeoutMs);
            StringBuilder buffer = new StringBuilder();

            while (DateTime.Now < deadline)
            {
                try
                {
                    if (_port.BytesToRead > 0)
                    {
                        string chunk = _port.ReadExisting();
                        buffer.Append(chunk);
                        
                        string content = buffer.ToString();
                        
                        // Ищем READY
                        if (content.Contains("READY"))
                        {
                            AppLogger.Info("Arduino готов (READY получен)");
                            
                            // Обрабатываем все строки до READY
                            foreach (var line in content.Split('\n'))
                            {
                                var trimmed = line.Trim('\r', ' ');
                                if (!string.IsNullOrEmpty(trimmed) && !trimmed.Equals("READY", StringComparison.OrdinalIgnoreCase))
                                {
                                    AppLogger.Debug($"Startup: {trimmed}");
                                }
                            }
                            
                            return true;
                        }
                    }
                    
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    AppLogger.Warning($"Ошибка при ожидании READY: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Отключение от порта
        /// </summary>
        private void Disconnect()
        {
            try
            {
                StopListener();

                if (_port.IsOpen)
                {
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();
                    _port.Close();
                    AppLogger.Info($"Порт {_port.PortName} закрыт");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Ошибка при отключении: {ex.Message}");
            }
        }

        public void UpdatePortList(ComboBox combo)
        {
            if (combo == null) return;

            var ports = SerialPort.GetPortNames()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            AppLogger.Info($"Доступные COM порты: {string.Join(", ", ports)}");

            combo.Items.Clear();
            combo.Items.AddRange(ports);
            if (ports.Length > 0) combo.SelectedIndex = 0;
        }

        /// <summary>
        /// Отправка команды с автоматическим retry
        /// </summary>
        public async Task<bool> SendCommandAsync(byte command, uint data)
        {
            // ✅ Проверка состояния
            if (!IsConnected)
            {
                AppLogger.Warning($"Команда 0x{command:X2}: порт не подключён");
                return false;
            }

            await _sendLock.WaitAsync().ConfigureAwait(false);

            try
            {
                // ✅ Retry loop
                for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                {
                    if (attempt > 1)
                    {
                        AppLogger.Info($"Повтор команды 0x{command:X2}, попытка {attempt}/{MAX_RETRIES}");
                        await Task.Delay(RETRY_DELAY_MS).ConfigureAwait(false);
                        
                        // Синхронизация - очистка буферов
                        SyncBuffers();
                    }

                    var result = await SendCommandInternalAsync(command, data).ConfigureAwait(false);
                    
                    if (result.Success)
                    {
                        return true;
                    }

                    // Если ошибка контрольной суммы или таймаут - повторяем
                    if (result.Message.Contains("CS") || result.Message.Contains("Timeout"))
                    {
                        AppLogger.Warning($"Команда 0x{command:X2} - ошибка: {result.Message}, повтор...");
                        continue;
                    }

                    // Другие ошибки - не повторяем
                    AppLogger.Warning($"Команда 0x{command:X2} - ошибка: {result.Message}");
                    return false;
                }

                AppLogger.Error($"Команда 0x{command:X2} не выполнена после {MAX_RETRIES} попыток");
                return false;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// Синхронизация буферов после сбоя
        /// </summary>
        private void SyncBuffers()
        {
            try
            {
                if (_port.IsOpen)
                {
                    if (_port.BytesToRead > 0)
                    {
                        AppLogger.Debug($"Синхронизация: очистка {_port.BytesToRead} байт");
                        _port.DiscardInBuffer();
                    }
                    if (_port.BytesToWrite > 0)
                    {
                        _port.DiscardOutBuffer();
                    }
                }

                // Сбрасываем pending command
                lock (_pendingLock)
                {
                    _pendingCommand = null;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Ошибка синхронизации: {ex.Message}");
            }
        }

        /// <summary>
        /// Внутренняя отправка команды (одна попытка)
        /// </summary>
        private async Task<CommandResult> SendCommandInternalAsync(byte command, uint data)
        {
            var pending = new TaskCompletionSource<CommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_pendingLock)
            {
                if (_pendingCommand != null)
                {
                    return new CommandResult(false, "Previous command pending");
                }
                _pendingCommand = pending;
            }

            try
            {
                // ✅ Формируем и отправляем пакет
                var packet = BuildPacket(command, data);
                AppLogger.Debug($"TX -> cmd=0x{command:X2}, data={data}, packet=[{BitConverter.ToString(packet)}]");

                await _port.BaseStream.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
                await _port.BaseStream.FlushAsync().ConfigureAwait(false);

                // ✅ Ожидание ответа с таймаутом
                var completedTask = await Task.WhenAny(
                    pending.Task, 
                    Task.Delay(COMMAND_TIMEOUT_MS)
                ).ConfigureAwait(false);

                if (completedTask != pending.Task)
                {
                    ClearPending(pending);
                    return new CommandResult(false, "Timeout");
                }

                return await pending.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ClearPending(pending);
                return new CommandResult(false, ex.Message);
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
            packet[5] = (byte)(packet[0] ^ packet[1] ^ packet[2] ^ packet[3] ^ packet[4]);
            return packet;
        }

        private void StartListener()
        {
            StopListener();

            if (!_port.IsOpen)
            {
                AppLogger.Warning("Listener: порт не открыт");
                return;
            }

            _listenerCts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenerLoop(_listenerCts.Token));
            AppLogger.Debug("Listener запущен");
        }

        private void StopListener()
        {
            _isListenerRunning = false;

            if (_listenerCts == null) return;

            try
            {
                _listenerCts.Cancel();
                _listenerTask?.Wait(1000);
            }
            catch { }
            finally
            {
                _listenerCts?.Dispose();
                _listenerCts = null;
                _listenerTask = null;
            }

            // Сбрасываем pending
            lock (_pendingLock)
            {
                _pendingCommand?.TrySetResult(new CommandResult(false, "Listener stopped"));
                _pendingCommand = null;
            }

            AppLogger.Debug("Listener остановлен");
        }

        /// <summary>
        /// Главный цикл чтения данных от Arduino
        /// </summary>
        private void ListenerLoop(CancellationToken token)
        {
            StringBuilder lineBuffer = new StringBuilder();
            _isListenerRunning = true;
            
            AppLogger.Debug("Listener: цикл чтения запущен");

            try
            {
                while (!token.IsCancellationRequested && _port.IsOpen)
                {
                    try
                    {
                        // ✅ Проверяем наличие данных
                        int available = _port.BytesToRead;
                        
                        if (available == 0)
                        {
                            Thread.Sleep(READ_POLL_INTERVAL_MS);
                            continue;
                        }

                        // ✅ Читаем все доступные данные
                        string chunk = _port.ReadExisting();
                        
                        if (string.IsNullOrEmpty(chunk))
                            continue;

                        lineBuffer.Append(chunk);

                        // ✅ Защита от переполнения буфера
                        if (lineBuffer.Length > MAX_LINE_BUFFER_SIZE)
                        {
                            AppLogger.Warning($"Listener: буфер переполнен ({lineBuffer.Length} байт), очистка");
                            lineBuffer.Clear();
                            continue;
                        }

                        // ✅ Обрабатываем полные строки
                        string content = lineBuffer.ToString();
                        int lastNewline = content.LastIndexOf('\n');
                        
                        if (lastNewline >= 0)
                        {
                            // Извлекаем все полные строки
                            string completedLines = content.Substring(0, lastNewline + 1);
                            string remaining = content.Substring(lastNewline + 1);
                            
                            lineBuffer.Clear();
                            lineBuffer.Append(remaining);

                            // Обрабатываем каждую строку
                            foreach (string rawLine in completedLines.Split('\n'))
                            {
                                string line = rawLine.Trim('\r', ' ');
                                if (!string.IsNullOrEmpty(line))
                                {
                                    ProcessReceivedLine(line);
                                }
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        // Нормально - нет данных
                    }
                    catch (IOException ioEx)
                    {
                        if (_port.IsOpen)
                        {
                            AppLogger.Warning($"Listener IO ошибка: {ioEx.Message}");
                            Thread.Sleep(100);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        AppLogger.Debug("Listener: порт стал недоступен");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    AppLogger.Error("Listener crashed", ex);
                }
            }
            finally
            {
                _isListenerRunning = false;
                
                lock (_pendingLock)
                {
                    _pendingCommand?.TrySetResult(new CommandResult(false, "Listener finished"));
                    _pendingCommand = null;
                }

                AppLogger.Debug("Listener: цикл завершён");
            }
        }

        /// <summary>
        /// Обработка полученной строки от Arduino
        /// </summary>
        private void ProcessReceivedLine(string line)
        {
            AppLogger.Trace($"RX: {line}");

            // ✅ Ответы на команды (имеют приоритет)
            if (line.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                CompletePending(true, line);
                return;
            }

            if (line.StartsWith("ERR", StringComparison.OrdinalIgnoreCase))
            {
                CompletePending(false, line);
                return;
            }

            if (line.StartsWith("PSET", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Info($"Профиль установлен: {line}");
                CompletePending(true, line);
                return;
            }

            if (line.StartsWith("PROFILE:", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Debug($"Данные профиля: {line}");
                ProfileDataReceived?.Invoke(line);
                CompletePending(true, line);
                return;
            }

            if (line.Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Warning("Arduino: команда не реализована");
                CompletePending(false, line);
                return;
            }

            // ✅ События (не влияют на pending command)
            if (line.Equals("READY", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Info("Arduino: READY");
                UnsolicitedEventReceived?.Invoke(line);
                return;
            }

            if (Protocol.Events.TryParseSensorEvent(line, out bool sensorOn))
            {
                AppLogger.Info($"Датчик: {(sensorOn ? "ВКЛ" : "ВЫКЛ")}");
                UnsolicitedEventReceived?.Invoke(line);
                return;
            }

            if (Protocol.Events.IsEvent(line))
            {
                AppLogger.Debug($"Событие: {line}");
                UnsolicitedEventReceived?.Invoke(line);
                return;
            }

            // Ответ на запрос датчика
            if (line.StartsWith("S:", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Debug($"Состояние датчика: {line}");
                CompletePending(true, line);
                return;
            }

            AppLogger.Warning($"Неизвестный ответ: {line}");
        }

        private void CompletePending(bool success, string message)
        {
            TaskCompletionSource<CommandResult> pending;
            lock (_pendingLock)
            {
                pending = _pendingCommand;
                _pendingCommand = null;
            }

            pending?.TrySetResult(new CommandResult(success, message));
        }

        private void ClearPending(TaskCompletionSource<CommandResult> expected)
        {
            lock (_pendingLock)
            {
                if (_pendingCommand == expected)
                {
                    _pendingCommand = null;
                }
            }
            
            expected.TrySetResult(new CommandResult(false, "Cleared"));
        }

        public void Dispose()
        {
            Disconnect();
            
            try
            {
                _port?.Dispose();
            }
            catch { }

            _sendLock?.Dispose();
        }

        private class CommandResult
        {
            public bool Success { get; }
            public string Message { get; }

            public CommandResult(bool success, string message)
            {
                Success = success;
                Message = message;
            }
        }
    }
}
