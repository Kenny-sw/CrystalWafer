using System;
using System.IO.Ports;
using System.Threading;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Упрощенный контроллер последовательного порта для диагностики и тестирования.
    /// Синхронная отправка/прием без отдельного listener thread.
    /// </summary>
    public class SimpleSerialController : IDisposable
    {
        private readonly SerialPort _port;
        private readonly object _lock = new object();
        private readonly int _commandTimeoutMs = 3000;

        public SimpleSerialController(SerialPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public void Connect(string portName)
        {
            lock (_lock)
            {
                if (_port.IsOpen)
                {
                    AppLogger.Info("Порт уже открыт. Закрываю...");
                    _port.Close();
                }

                _port.PortName = portName;
                _port.BaudRate = 115200;
                _port.Parity = Parity.None;
                _port.DataBits = 8;
                _port.StopBits = StopBits.One;
                _port.Handshake = Handshake.None;
                _port.NewLine = "\n";  // ✅ Arduino стиль
                _port.ReadTimeout = 1000;
                _port.WriteTimeout = 1000;
                _port.DtrEnable = false; // Не сбрасываем Arduino

                _port.Open();
                AppLogger.Info($"[SIMPLE] Порт {portName} открыт");

                // Настраиваем DTR для стабильности
                _port.DtrEnable = true;
                _port.RtsEnable = true;

                // Очищаем буферы
                if (_port.BytesToRead > 0)
                {
                    AppLogger.Debug($"[SIMPLE] Очистка входного буфера: {_port.BytesToRead} байт");
                    _port.DiscardInBuffer();
                }

                // Даем Arduino время на старт
                Thread.Sleep(100);

                // Читаем приветствие
                ReadStartupMessages();
            }
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                if (_port.IsOpen)
                {
                    try
                    {
                        _port.Close();
                        AppLogger.Info("[SIMPLE] Порт закрыт");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("[SIMPLE] Ошибка закрытия порта", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Отправка команды и синхронное ожидание ответа
        /// </summary>
        public bool SendCommand(byte command, uint data)
        {
            lock (_lock)
            {
                if (!_port.IsOpen)
                {
                    AppLogger.Warning("[SIMPLE] Порт закрыт");
                    return false;
                }

                try
                {
                    // 1. Очищаем входной буфер
                    if (_port.BytesToRead > 0)
                    {
                        AppLogger.Debug($"[SIMPLE] Очистка буфера: {_port.BytesToRead} байт");
                        _port.DiscardInBuffer();
                    }

                    // 2. Формируем пакет
                    var packet = BuildPacket(command, data);
                    AppLogger.Debug($"[SIMPLE] TX -> cmd=0x{command:X2}, data={data}, packet=[{BitConverter.ToString(packet)}]");

                    // 3. Отправляем
                    _port.Write(packet, 0, packet.Length);
                    _port.BaseStream.Flush();

                    // 4. Ждем ответа (синхронно)
                    return WaitForResponse();
                }
                catch (TimeoutException)
                {
                    AppLogger.Warning($"[SIMPLE] Timeout команды 0x{command:X2}");
                    return false;
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"[SIMPLE] Ошибка отправки команды 0x{command:X2}", ex);
                    return false;
                }
            }
        }

        private bool WaitForResponse()
        {
            var deadline = DateTime.Now.AddMilliseconds(_commandTimeoutMs);

            while (DateTime.Now < deadline)
            {
                try
                {
                    // Проверяем наличие данных
                    if (_port.BytesToRead > 0)
                    {
                        string line = _port.ReadLine().Trim();

                        if (string.IsNullOrEmpty(line))
                            continue;

                        AppLogger.Debug($"[SIMPLE] RX <- {line}");

                        // Обработка ответов
                        if (line.Equals("OK", StringComparison.OrdinalIgnoreCase))
                        {
                            AppLogger.Info("[SIMPLE] Команда выполнена: OK");
                            return true;
                        }

                        if (line.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                        {
                            AppLogger.Warning($"[SIMPLE] Arduino вернул ошибку: {line}");
                            return false;
                        }

                        // Событие - пропускаем и ждем OK
                        if (line.StartsWith("EV ", StringComparison.OrdinalIgnoreCase))
                        {
                            AppLogger.Info($"[SIMPLE] Событие: {line}");
                            continue; // Продолжаем ждать OK
                        }

                        // Неизвестный ответ
                        AppLogger.Warning($"[SIMPLE] Неизвестный ответ: {line}");
                    }

                    // Небольшая задержка чтобы не нагружать CPU
                    Thread.Sleep(10);
                }
                catch (TimeoutException)
                {
                    // ReadLine timeout - продолжаем ждать
                    continue;
                }
                catch (Exception ex)
                {
                    AppLogger.Error("[SIMPLE] Ошибка чтения ответа", ex);
                    return false;
                }
            }

            AppLogger.Warning("[SIMPLE] Timeout: Arduino не ответил");
            return false;
        }

        private void ReadStartupMessages()
        {
            try
            {
                var deadline = DateTime.Now.AddMilliseconds(1000);

                while (DateTime.Now < deadline)
                {
                    if (_port.BytesToRead > 0)
                    {
                        string line = _port.ReadLine().Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            AppLogger.Info($"[SIMPLE] Startup: {line}");
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Debug($"[SIMPLE] Ошибка чтения startup: {ex.Message}");
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
                checksum ^= buffer[i];
            }
            return checksum;
        }

        public bool IsOpen => _port?.IsOpen ?? false;

        public void Dispose()
        {
            Disconnect();
            _port?.Dispose();
        }
    }
}
