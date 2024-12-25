using System;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        private void buttonMoveLeft_Click(object sender, EventArgs e)
        {
            int stepSizeX;
            if (int.TryParse(SizeX.Text, out stepSizeX))
            {
                SendCommand(1, stepSizeX); // Команда движения влево
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите корректное значение для X.");
            }
        }

        private void buttonMoveRight_Click(object sender, EventArgs e)
        {
            int stepSizeX;
            if (int.TryParse(SizeX.Text, out stepSizeX))
            {
                SendCommand(2, stepSizeX); // Команда движения вправо
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите корректное значение для X.");
            }
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            int stepSizeY;
            if (int.TryParse(SizeY.Text, out stepSizeY))
            {
                SendCommand(3, stepSizeY); // Команда движения вверх
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите корректное значение для Y.");
            }
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            int stepSizeY;
            if (int.TryParse(SizeY.Text, out stepSizeY))
            {
                SendCommand(4, stepSizeY); // Команда движения вниз
                Thread.Sleep(300);
            }
            else
            {
                MessageBox.Show("Введите корректное значение для Y.");
            }
        }

        private void scan_Click(object sender, EventArgs e)
        {
            int scanSizeX, scanSizeY;
            if (int.TryParse(SizeX.Text, out scanSizeX) && (int.TryParse(SizeY.Text, out scanSizeY)))
            {
                SendCommand(1, scanSizeX * 10);
                Thread.Sleep(300);
                
                SendCommand(3, scanSizeY * 10);
                Thread.Sleep(300);


            }
        }
      

        private void SendCommand(byte commandByte, int stepSize)
        {
            if (stepSize >= 0 && stepSize <= 65535)
            {
                byte firstByte = (byte)(stepSize / 256); // Старший байт
                byte secondByte = (byte)(stepSize % 256); // Младший байт

                try
                {
                    MyserialPort.Write(new byte[] { commandByte, firstByte, secondByte }, 0, 3);
                    UpdateWaferVisualization(); // Обновляем визуализацию после отправки команды
                }
                catch
                {
                    MessageBox.Show("Ошибка отправки данных. Убедитесь, что COM-порт подключен.");
                }
            }
            else
            {
                MessageBox.Show("Неверное число или значение байт для отправки. Убедитесь, что число в диапазоне от 0 до 65535.");
            }
        }
    }
}
