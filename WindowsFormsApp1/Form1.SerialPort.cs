// Класс для обработки подключения к COM-порту и работы с ним
using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        // Обработчик кнопки подключения/отключения к COM-порту
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!MyserialPort.IsOpen)
                {
                    // Проверяем, выбран ли порт для подключения
                    if (string.IsNullOrEmpty(comboBoxPorts.Text))
                    {
                        MessageBox.Show("Выберите порт для подключения.");
                        return;
                    }

                    // Устанавливаем имя порта и открываем соединение
                    MyserialPort.PortName = comboBoxPorts.Text;
                    MyserialPort.Open();
                    comboBoxPorts.Enabled = false; // Блокируем выбор порта после подключения
                    buttonConnect.Text = "Отключиться";
                    buttonConnect.BackColor = Color.LightGreen; // Меняем цвет кнопки на зеленый при успешном подключении
                }
                else
                {
                    // Закрываем соединение при повторном нажатии кнопки
                    MyserialPort.Close();
                    comboBoxPorts.Enabled = true; // Разрешаем выбор порта после отключения
                    buttonConnect.Text = "Подключить";
                    buttonConnect.BackColor = Color.LightCoral; // Меняем цвет кнопки на красный при отключении
                }
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке при попытке подключения/отключения
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

        // Обработчик приема данных с COM-порта
        private void MyserialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Считываем данные, поступившие с COM-порта
            string data = MyserialPort.ReadExisting();
            this.Invoke(new MethodInvoker(delegate
            {
                // Отображаем полученные данные в сообщении
                MessageBox.Show("Получены данные: " + data);
            }));
        }
    }
}
