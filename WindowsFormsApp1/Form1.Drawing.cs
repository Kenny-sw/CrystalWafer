// Form1.Drawing.cs - Методы для отрисовки пластины и кристаллов
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        // Метод, вызываемый при необходимости перерисовки PictureBox. Используется для отрисовки всей пластины и кристаллов.
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Получаем объект Graphics для выполнения операций рисования на PictureBox
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            if (float.TryParse(SizeX.Text, out crystalWidthRaw) &&
                float.TryParse(SizeY.Text, out crystalHeightRaw) &&
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

        // Метод для отрисовки кремниевой пластины в виде круга в центре PictureBox
        private void DrawWafer(Graphics g)
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            float displayRadius = radius * scaleFactor;

            g.DrawEllipse(Pens.Black, centerX - displayRadius, centerY - displayRadius, displayRadius * 2, displayRadius * 2);
        }

        // Метод для отрисовки всех кристаллов на пластине
        private void DrawCrystals(Graphics g, float crystalWidthRaw, float crystalHeightRaw)
        {
            crystals.Clear(); // Очищаем текущую коллекцию кристаллов
            nextCrystalIndex = 1; // Сбрасываем индекс

            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            float radiusSquared = radius * radius; // Избегаем повторного вычисления Math.Sqrt
            int totalCrystals = 0;

            float crystalWidth = crystalWidthRaw / 1000;
            float crystalHeight = crystalHeightRaw / 1000;
            float displayCrystalWidth = crystalWidth * scaleFactor;
            float displayCrystalHeight = crystalHeight * scaleFactor;

            float startX = centerX - radius;
            float endX = centerX + radius;
            float startY = centerY - radius;
            float endY = centerY + radius;

            int numCrystalsX = (int)((endX - startX) / crystalWidth);
            int numCrystalsY = (int)((endY - startY) / crystalHeight);

            for (int i = 0; i <= numCrystalsX; i++)
            {
                for (int j = 0; j <= numCrystalsY; j++)
                {
                    float crystalX = startX + i * crystalWidth + crystalWidth / 2;
                    float crystalY = startY + j * crystalHeight + crystalHeight / 2;

                    float dx = crystalX - centerX;
                    float dy = crystalY - centerY;

                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        Crystal crystal = new Crystal
                        {
                            Index = nextCrystalIndex++,
                            RealX = crystalX,
                            RealY = crystalY,
                            Color = Color.Blue
                        };

                        crystals.Add(crystal);

                        float scaledCrystalX = (crystal.RealX - centerX) * scaleFactor + centerX;
                        float scaledCrystalY = (crystal.RealY - centerY) * scaleFactor + centerY;

                        crystal.DisplayX = scaledCrystalX;
                        crystal.DisplayY = scaledCrystalY;

                        g.DrawRectangle(Pens.Blue, crystal.DisplayX - displayCrystalWidth / 2, crystal.DisplayY - displayCrystalHeight / 2, displayCrystalWidth, displayCrystalHeight);

                        if (selectedCrystalIndex == crystal.Index)
                        {
                            using (Pen selectionPen = new Pen(Color.Yellow, 2))
                            {
                                g.DrawRectangle(selectionPen, crystal.DisplayX - displayCrystalWidth / 2, crystal.DisplayY - displayCrystalHeight / 2, displayCrystalWidth, displayCrystalHeight);
                            }
                        }

                        totalCrystals++;
                    }
                }
            }

            if (selectedCrystalIndex > crystals.Count)
            {
                selectedCrystalIndex = -1;
            }

            labelTotalCrystals.Text = $"Общее количество кристаллов: {totalCrystals}";
        }




        // Метод, вызываемый при движении мыши над PictureBox. Используется для определения, над каким кристаллом находится курсор.
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label3.Text = $"X: {e.X}";
            label4.Text = $"Y: {e.Y}";
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;

            // Перебираем все кристаллы в коллекции
            foreach (var crystal in crystals)
            {
                // Масштабируем координаты центра кристалла для отображения на экране
                float scaledCrystalX = (crystal.RealX - centerX) * scaleFactor + centerX;
                float scaledCrystalY = (crystal.RealY - centerY) * scaleFactor + centerY;

                // Определяем размеры кристалла на экране, используя scaleFactor
                float displayCrystalWidth = (crystalWidthRaw / 1000) * scaleFactor;
                float displayCrystalHeight = (crystalHeightRaw / 1000) * scaleFactor;

                // Определяем границы кристалла
                float left = scaledCrystalX - displayCrystalWidth / 2;
                float right = scaledCrystalX + displayCrystalWidth / 2;
                float top = scaledCrystalY - displayCrystalHeight / 2;
                float bottom = scaledCrystalY + displayCrystalHeight / 2;

                // Проверяем, находится ли курсор внутри границ текущего кристалла
                if (e.X >= left && e.X <= right && e.Y >= top && e.Y <= bottom)
                {
                    // Если курсор находится над кристаллом, показываем его индекс
                    labelIndex.Text = $"Индекс кристалла: {crystal.Index}";
                    return;
                }
            }

            // Если ни один кристалл не найден, очищаем текст
            labelIndex.Text = "Индекс кристалла: -";
        }
    }
}
