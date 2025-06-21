using System;
using System.Windows.Forms;
using System.Threading;

namespace CrystalTable
{
    public partial class Form1
    {
        // Обработчики кнопок движения
        private void buttonMoveLeft_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeX.Text, out uint stepSizeX))
            {
                // Команда 1 = движение влево
                SendCommand(1, stepSizeX);
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для X.");
            }
        }

        private void buttonMoveRight_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeX.Text, out uint stepSizeX))
            {
                // Команда 2 = движение вправо
                SendCommand(2, stepSizeX);
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для X.");
            }
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeY.Text, out uint stepSizeY))
            {
                // Команда 3 = движение вверх
                SendCommand(3, stepSizeY);
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для Y.");
            }
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeY.Text, out uint stepSizeY))
            {
                // Команда 4 = движение вниз
                SendCommand(4, stepSizeY);
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для Y.");
            }
        }

        // ----- Обработчик кнопки Scan -----
        private void scan_Click(object sender, EventArgs e)
        {
            
            if (uint.TryParse(SizeX.Text, out uint scanSizeX) &&
                uint.TryParse(SizeY.Text, out uint scanSizeY))
            {
                // Двигаемся влево (команда 1)
                SendCommand(1, scanSizeX * 100);
                Thread.Sleep(100);

                // Двигаемся вверх (команда 3)
                SendCommand(3, scanSizeY * 100);
                Thread.Sleep(100);
            }
            else
            {
                MessageBox.Show("Введите положительные числа для X и Y.");
            }
        }

        // ----- Метод отправки 5-байтовой команды (команда + 4 байта шага) -----
        private void SendCommand(byte commandByte, uint stepSize)
        {
            if (!MyserialPort.IsOpen)
            {
                MessageBox.Show("COM-порт не подключен.");
                return;
            }

            // Разбиваем 32-битное число stepSize на 4 байта (младший -> старший)
            byte b0 = (byte)(stepSize & 0xFF);         // младший байт
            byte b1 = (byte)((stepSize >> 8) & 0xFF);
            byte b2 = (byte)((stepSize >> 16) & 0xFF);
            byte b3 = (byte)((stepSize >> 24) & 0xFF); // старший байт

            // Пакет: [команда] [b0] [b1] [b2] [b3]
            byte[] dataToSend = new byte[] { commandByte, b0, b1, b2, b3 };

            try
            {
                // Отправляем в порт
                MyserialPort.Write(dataToSend, 0, dataToSend.Length);

                // Ваш метод обновления визуализации (если нужен)
                UpdateWaferVisualization();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}");
            }
        }

        // ----- Старый метод (3 байта) 
        /*
        private void SendCommand(byte commandByte, int stepSize)
        {
            if (stepSize >= 0 && stepSize <= 65535) 
            {
                byte firstByte = (byte)(stepSize >> 8);    
                byte secondByte = (byte)(stepSize & 0xFF);

                try
                {
                    MyserialPort.Write(new byte[] { commandByte, firstByte, secondByte }, 0, 3);
                    UpdateWaferVisualization();
                }
                catch
                {
                    MessageBox.Show("Ошибка отправки данных. Убедитесь, что COM-порт подключен.");
                }
            }
            else
            {
                MessageBox.Show("Неверное число. Диапазон от 0 до 65535.");
            }
        }
        */
    }
}