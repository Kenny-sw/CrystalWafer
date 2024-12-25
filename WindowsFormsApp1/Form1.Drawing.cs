using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        // Метод, который вызывается, когда необходимо перерисовать pictureBox1 (например, при Invalidate()).
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            // Проверка корректности данных
            if (!IsInputValid())
            {
                labelTotalCrystals.Text = "Введите корректные размеры кристаллов и пластины";
                return;
            }

            // Перед рисованием автоматически подстраиваем scaleFactor
            AutoSetScaleFactor();

            // Рисуем круг (пластину)
            DrawWafer(g);

            // Строим (пересчитываем) кристаллы
            BuildCrystals(crystalWidthRaw, crystalHeightRaw);

            // Рисуем кристаллы
            DrawCrystals(g);
        }

        private void AutoSetScaleFactor()
        {
            float picWidth = pictureBox1.Width;
            float picHeight = pictureBox1.Height;
            float minSide = Math.Min(picWidth, picHeight);

            scaleFactor = minSide / waferDiameter;
            // scaleFactor *= 0.9f; // если хотите чуть поменьше, чтобы был отступ
        }

        private bool IsInputValid()
        {
            if (float.TryParse(SizeX.Text, out crystalWidthRaw) &&
                float.TryParse(SizeY.Text, out crystalHeightRaw) &&
                float.TryParse(WaferDiameter.Text, out float waferDiameterRaw) &&
                crystalWidthRaw > 0 && crystalHeightRaw > 0 &&
                waferDiameterRaw >= MinWaferDiameter &&
                waferDiameterRaw <= MaxWaferDiameter)
            {
                waferDiameter = waferDiameterRaw;
                return true;
            }
            return false;
        }

        // Рисуем пластину в виде круга
        private void DrawWafer(Graphics g)
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            float displayRadius = radius * scaleFactor;

            g.DrawEllipse(Pens.Black,
                          centerX - displayRadius,
                          centerY - displayRadius,
                          displayRadius * 2,
                          displayRadius * 2);
        }

        // Строим (генерируем) коллекцию кристаллов, которые укладываются в круг.
        private void BuildCrystals(float crystalWidthRaw, float crystalHeightRaw)
        {
            crystals.Clear();
            nextCrystalIndex = 1;

            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            float radiusSquared = radius * radius;

            int totalCrystals = 0;

            float crystalWidth = crystalWidthRaw / 1000;
            float crystalHeight = crystalHeightRaw / 1000;

            float startX = centerX - radius;
            float endX = centerX + radius;
            float startY = centerY - radius;
            float endY = centerY + radius;

            int numCrystalsX = (int)((endX - startX) / crystalWidth);
            int numCrystalsY = (int)((endY - startY) / crystalHeight);

            bool isReversed = false;
            List<Crystal> rowCrystals = new List<Crystal>();

            for (int j = 0; j <= numCrystalsY; j++)
            {
                rowCrystals.Clear();
                for (int i = 0; i <= numCrystalsX; i++)
                {
                    float crystalX = startX + i * crystalWidth + crystalWidth / 2;
                    float crystalY = startY + j * crystalHeight + crystalHeight / 2;

                    if (IsInsideCircle(crystalX, crystalY, centerX, centerY, radiusSquared))
                    {
                        Crystal crystal = new Crystal
                        {
                            Index = nextCrystalIndex++,
                            RealX = crystalX,
                            RealY = crystalY,
                            Color = Color.Blue
                        };

                        rowCrystals.Add(crystal);
                        totalCrystals++;
                    }
                }

                if (isReversed)
                {
                    rowCrystals.Reverse();
                    for (int i = 0; i < rowCrystals.Count; i++)
                    {
                        rowCrystals[i].Index = nextCrystalIndex - rowCrystals.Count + i;
                    }
                }

                crystals.AddRange(rowCrystals);
                isReversed = !isReversed;
            }

            labelTotalCrystals.Text = $"Общее количество кристаллов: {totalCrystals}";
        }

        // Рисуем кристаллы. Если кристалл выбран, заливаем его жёлтым.
        private void DrawCrystals(Graphics g)
        {
            float displayCrystalWidth = (crystalWidthRaw / 1000) * scaleFactor;
            float displayCrystalHeight = (crystalHeightRaw / 1000) * scaleFactor;

            foreach (var crystal in crystals)
            {
                float halfW = displayCrystalWidth / 2;
                float halfH = displayCrystalHeight / 2;

                float centerX = pictureBox1.Width / 2;
                float centerY = pictureBox1.Height / 2;

                float scaledCrystalX = (crystal.RealX - centerX) * scaleFactor + centerX;
                float scaledCrystalY = (crystal.RealY - centerY) * scaleFactor + centerY;

                crystal.DisplayX = scaledCrystalX;
                crystal.DisplayY = scaledCrystalY;

                // Если кристалл выбран, заливаем его жёлтым
                if (selectedCrystalIndex == crystal.Index)
                {
                    using (Brush fillBrush = new SolidBrush(Color.Yellow))
                    {
                        g.FillRectangle(fillBrush,
                            crystal.DisplayX - halfW,
                            crystal.DisplayY - halfH,
                            displayCrystalWidth,
                            displayCrystalHeight);
                    }
                }
                else
                {
                    // Просто рисуем синим контуром
                    g.DrawRectangle(Pens.Blue,
                        crystal.DisplayX - halfW,
                        crystal.DisplayY - halfH,
                        displayCrystalWidth,
                        displayCrystalHeight);
                }
            }

            // Если выбранный индекс за границами списка, сбрасываем
            if (selectedCrystalIndex > crystals.Count)
            {
                selectedCrystalIndex = -1;
            }
        }

        private bool IsInsideCircle(float px, float py, float cx, float cy, float radiusSq)
        {
            float dx = px - cx;
            float dy = py - cy;
            return (dx * dx + dy * dy) <= radiusSq;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label3.Text = $"X: {e.X}";
            label4.Text = $"Y: {e.Y}";

            float displayCrystalWidth = (crystalWidthRaw / 1000) * scaleFactor;
            float displayCrystalHeight = (crystalHeightRaw / 1000) * scaleFactor;

            foreach (var crystal in crystals)
            {
                float left = crystal.DisplayX - displayCrystalWidth / 2;
                float right = crystal.DisplayX + displayCrystalWidth / 2;
                float top = crystal.DisplayY - displayCrystalHeight / 2;
                float bottom = crystal.DisplayY + displayCrystalHeight / 2;

                if (e.X >= left && e.X <= right && e.Y >= top && e.Y <= bottom)
                {
                    labelIndex.Text = $"Индекс кристалла: {crystal.Index}";
                    return;
                }
            }

            labelIndex.Text = "Индекс кристалла: -";
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Проверяем кристаллы от последнего к первому (чтобы выбрать "верхний" при перекрытии)
            for (int i = crystals.Count - 1; i >= 0; i--)
            {
                var crystal = crystals[i];

                float displayCrystalWidth = (crystalWidthRaw / 1000) * scaleFactor;
                float displayCrystalHeight = (crystalHeightRaw / 1000) * scaleFactor;

                float left = crystal.DisplayX - displayCrystalWidth / 2;
                float right = crystal.DisplayX + displayCrystalWidth / 2;
                float top = crystal.DisplayY - displayCrystalHeight / 2;
                float bottom = crystal.DisplayY + displayCrystalHeight / 2;

                if (e.X >= left && e.X <= right && e.Y >= top && e.Y <= bottom)
                {
                    // Назначаем выбранный кристалл
                    selectedCrystalIndex = crystal.Index;
                    pictureBox1.Invalidate();
                    return;
                }
            }

            // Если ни один кристалл не выбран
            selectedCrystalIndex = -1;
            pictureBox1.Invalidate();
        }
    }
}
