// Form1.Drawing.cs - Методы для отрисовки пластины и кристаллов
using System.Drawing;
using System.Windows.Forms;
using System;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            g.Clear(Color.White);

            if (float.TryParse(SizeX.Text, out float crystalWidthRaw) &&
                float.TryParse(SizeY.Text, out float crystalHeightRaw) &&
                float.TryParse(WaferDiameter.Text, out float waferDiameterRaw) &&
                crystalWidthRaw > 0 && crystalHeightRaw > 0 && waferDiameterRaw >= MinWaferDiameter && waferDiameterRaw <= MaxWaferDiameter)
            {
                waferDiameter = waferDiameterRaw;
                DrawWafer(g);
                DrawCrystals(g, crystalWidthRaw, crystalHeightRaw);
            }
            else
            {
                labelTotalCrystals.Text = "Введите корректные размеры кристаллов и пластины";
            }
        }

        private void DrawWafer(Graphics g)
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            float displayRadius = radius * scaleFactor;

            g.DrawEllipse(Pens.Black, centerX - displayRadius, centerY - displayRadius, displayRadius * 2, displayRadius * 2);
        }

        private void DrawCrystals(Graphics g, float crystalWidthRaw, float crystalHeightRaw)
        {


            // Находим центр PictureBox для размещения кристаллов на пластине
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            int totalCrystals = 0;

            // Переводим размеры кристаллов в метры и применяем масштаб для отображения
            float crystalWidth = crystalWidthRaw / 1000;
            float crystalHeight = crystalHeightRaw / 1000;
            float displayCrystalWidth = crystalWidth * scaleFactor;
            float displayCrystalHeight = crystalHeight * scaleFactor;

            // Определяем количество кристаллов, которые можно разместить по оси X и Y
            int numCrystalsX = (int)(2 * radius / crystalWidth);
            int numCrystalsY = (int)(2 * radius / crystalHeight);

            // Начальные координаты для размещения кристаллов (левый верхний угол области)
            float startX = centerX - radius;
            float startY = centerY - radius;

            // Цикл по координате X
            for (int i = 0; i < numCrystalsX; i++)
            {
                // Цикл по координате Y
                for (int j = 0; j < numCrystalsY; j++)
                {
                    // Рассчитываем координаты центра текущего кристалла
                    float crystalX = startX + i * crystalWidth + crystalWidth / 2;
                    float crystalY = startY + j * crystalHeight + crystalHeight / 2;

                    // Рассчитываем расстояние от центра пластины до текущего кристалла
                    float distanceFromCenter = (float)Math.Sqrt(Math.Pow(crystalX - centerX, 2) + Math.Pow(crystalY - centerY, 2));

                    // Проверяем, находится ли кристалл внутри окружности пластины
                    if (distanceFromCenter <= radius)
                    {
                        // Масштабируем координаты кристалла для отображения на экране
                        float scaledCrystalX = (crystalX - centerX) * scaleFactor + centerX;
                        float scaledCrystalY = (crystalY - centerY) * scaleFactor + centerY;

                        // Рисуем кристалл в виде синего прямоугольника
                        g.DrawRectangle(Pens.Blue, scaledCrystalX - displayCrystalWidth / 2, scaledCrystalY - displayCrystalHeight / 2, displayCrystalWidth, displayCrystalHeight);

                        // Проверяем, является ли текущий кристалл выбранным
                        if (selectedCrystalIndex == totalCrystals) // Индекс totalCrystals будет равен текущему количеству кристаллов
                        {
                            using (Pen selectionPen = new Pen(Color.Yellow, 2))
                            {
                                g.DrawRectangle(selectionPen, scaledCrystalX - displayCrystalWidth / 2, scaledCrystalY - displayCrystalHeight / 2, displayCrystalWidth, displayCrystalHeight);
                            }
                        }

                        // Увеличиваем количество размещенных кристаллов
                        totalCrystals++;
                    }
                }
            }

            labelTotalCrystals.Text = $"Общее количество кристаллов: {totalCrystals}";
        }
    }
}
