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