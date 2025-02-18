// Класс для обработки подключения к COM-порту и работы с ним
using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1
    {
        // Обработчик подключения/отключения COM-порта
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!MyserialPort.IsOpen)
                {
                    // Проверка выбора порта
                    if (string.IsNullOrEmpty(comboBoxPorts.Text))
                    {
                        MessageBox.Show("Выберите порт для подключения.");
                        return;
                    }

                    // Подключение к порту
                    MyserialPort.PortName = comboBoxPorts.Text;
                    MyserialPort.Open();
                    comboBoxPorts.Enabled = false;
                    buttonConnect.Text = "Отключиться";
                    buttonConnect.BackColor = Color.LightGreen;
                }
                else
                {
                    // Отключение от порта
                    MyserialPort.Close();
                    comboBoxPorts.Enabled = true;
                    buttonConnect.Text = "Подключить";
                    buttonConnect.BackColor = Color.LightCoral;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

        // Обработчик кнопки обновления списка доступных портов
        private void buttonUpdatePort_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames(); // Получаем список доступных портов
            comboBoxPorts.Items.Clear();                // Очищаем выпадающий список
            comboBoxPorts.Text = "";                    // Очищаем текущее значение
            if (ports.Length != 0)                      // Проверяем, есть ли доступные порты
            {
                comboBoxPorts.Items.AddRange(ports);   // Добавляем доступные порты в выпадающий список
                comboBoxPorts.SelectedIndex = 0;       // Устанавливаем первый доступный порт как выбранный
            }
            else
            {
                // Выводим сообщение, если нет доступных COM-портов
                MessageBox.Show("Нет доступных COM-портов.");
            }
        }

        // Обработчик приема данных с порта
        private void MyserialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Чтение данных с порта
            string data = MyserialPort.ReadExisting();
            this.Invoke(new MethodInvoker(delegate
            {
                MessageBox.Show("Получены данные: " + data);
            }));
        }
    }
}
