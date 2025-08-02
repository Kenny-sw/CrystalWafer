using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для работы с COM-портом
    /// </summary>
    public class SerialPortController : IDisposable
    {
        private readonly SerialPort serialPort;

        public SerialPortController(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        /// <summary>
        /// Переключение подключения
        /// </summary>
        public void ToggleConnection(string portName, Button connectButton, ComboBox portsComboBox)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    if (string.IsNullOrEmpty(portName))
                    {
                        MessageBox.Show("Выберите порт для подключения.");
                        return;
                    }

                    serialPort.PortName = portName;
                    serialPort.Open();
                    portsComboBox.Enabled = false;
                    connectButton.Text = "Отключиться";
                    connectButton.BackColor = Color.LightGreen;
                }
                else
                {
                    serialPort.Close();
                    portsComboBox.Enabled = true;
                    connectButton.Text = "Подключить";
                    connectButton.BackColor = Color.LightCoral;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

        /// <summary>
        /// Обновление списка портов
        /// </summary>
        public void UpdatePortList(ComboBox comboBox)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox.Items.Clear();
            comboBox.Text = "";

            if (ports.Length != 0)
            {
                comboBox.Items.AddRange(ports);
                comboBox.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Нет доступных COM-портов.");
            }
        }

        /// <summary>
        /// Отправка команды
        /// </summary>
        public async Task SendCommandAsync(byte commandByte, uint stepSize)
        {
            if (!serialPort.IsOpen)
            {
                MessageBox.Show("Порт не подключен!");
                return;
            }

            // Разбиваем 32-битное число на 4 байта
            byte b0 = (byte)(stepSize & 0xFF);
            byte b1 = (byte)((stepSize >> 8) & 0xFF);
            byte b2 = (byte)((stepSize >> 16) & 0xFF);
            byte b3 = (byte)((stepSize >> 24) & 0xFF);

            byte[] dataToSend = new byte[] { commandByte, b0, b1, b2, b3 };

            try
            {
                await serialPort.BaseStream.WriteAsync(dataToSend, 0, dataToSend.Length);
                await serialPort.BaseStream.FlushAsync();
            }
            catch
            {
                MessageBox.Show("Ошибка отправки. Проверьте, что COM-порт доступен.");
            }
        }

        /// <summary>
        /// Обработка полученных данных
        /// </summary>
        public void HandleDataReceived()
        {
            try
            {
                string data = serialPort.ReadExisting();
                MessageBox.Show("Получены данные: " + data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении данных: " + ex.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                if (serialPort?.IsOpen == true)
                {
                    serialPort.Close();
                }
                serialPort?.Dispose();
            }
            catch { }
        }
    }
}