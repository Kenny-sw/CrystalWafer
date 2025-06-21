using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CrystalTable
{
    public partial class Form1
    {
        // Обработчики кнопок движения
        private async void buttonMoveLeft_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeX.Text, out uint stepSizeX))
            {
                await SendCommandAsync(1, stepSizeX);
                await Task.Delay(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для X.");
            }
        }

        private async void buttonMoveRight_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeX.Text, out uint stepSizeX))
            {
                await SendCommandAsync(2, stepSizeX);
                await Task.Delay(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для X.");
            }
        }

        private async void buttonMoveUp_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeY.Text, out uint stepSizeY))
            {
                await SendCommandAsync(3, stepSizeY);
                await Task.Delay(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для Y.");
            }
        }

        private async void buttonMoveDown_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(SizeY.Text, out uint stepSizeY))
            {
                await SendCommandAsync(4, stepSizeY);
                await Task.Delay(300);
            }
            else
            {
                MessageBox.Show("Введите положительное число для Y.");
            }
        }

        // ----- Обработчик кнопки Scan -----
        private async void scan_Click(object sender, EventArgs e)
        {

            if (uint.TryParse(SizeX.Text, out uint scanSizeX) &&
                uint.TryParse(SizeY.Text, out uint scanSizeY))
            {
                await SendCommandAsync(1, scanSizeX * 100);
                await Task.Delay(100);

                await SendCommandAsync(3, scanSizeY * 100);
                await Task.Delay(100);
            }
            else
            {
                MessageBox.Show("Введите положительные числа для X и Y.");
            }
        }

        // ----- Метод отправки 5-байтовой команды (команда + 4 байта шага) -----
        private async Task SendCommandAsync(byte commandByte, uint stepSize)
        {
            // Разбиваем 32-битное число stepSize на 4 байта (младший -> старший)
            byte b0 = (byte)(stepSize & 0xFF);         // младший байт
            byte b1 = (byte)((stepSize >> 8) & 0xFF);
            byte b2 = (byte)((stepSize >> 16) & 0xFF);
            byte b3 = (byte)((stepSize >> 24) & 0xFF); // старший байт

            // Пакет: [команда] [b0] [b1] [b2] [b3]
            byte[] dataToSend = new byte[] { commandByte, b0, b1, b2, b3 };

            try
            {
                await MyserialPort.BaseStream.WriteAsync(dataToSend, 0, dataToSend.Length);
                await MyserialPort.BaseStream.FlushAsync();
                UpdateWaferVisualization();
            }
            catch
            {
                MessageBox.Show("Ошибка отправки. Проверьте, что COM-порт доступен.");
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