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
            // Берём реальные размеры pictureBox
            float picWidth = pictureBox1.Width;
            float picHeight = pictureBox1.Height;

            // Находим меньшую сторону
            float minSide = Math.Min(picWidth, picHeight);

            // Делим её на диаметр (в мм), чтобы круг точно влез.
            // Если waferDiameter = 100 мм, и pictureBox ~ 300 пикс по меньшей стороне,
            // scaleFactor будет ~3.0 (100 * 3 = 300).
            scaleFactor = minSide / waferDiameter;

            // Если хотите оставлять поле по краям, 
            // можно чуть уменьшить масштаб (например, на 10%):
            // scaleFactor *= 0.9f;
        }


        // Проверяем корректность введённых значений (SizeX, SizeY, WaferDiameter).
        private bool IsInputValid()
        {
            
            // записываем waferDiameter и возвращаем true.
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
            // Если что-то не так, возвращаем false.
            return false;
        }

        // Рисуем пластину в виде круга с учётом масштаба (scaleFactor).
        private void DrawWafer(Graphics g)
        {
            // Находим центр PictureBox.
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;

            // Рассчитываем радиус (половина диаметра) и масштабируем его.
            float radius = waferDiameter / 2;
            float displayRadius = radius * scaleFactor;

            // Рисуем окружность.
            g.DrawEllipse(Pens.Black,
                          centerX - displayRadius,
                          centerY - displayRadius,
                          displayRadius * 2,
                          displayRadius * 2);
        }

        // Строим (генерируем) коллекцию кристаллов, которые укладываются в круг (пластину). Тут не рисуем.
        private void BuildCrystals(float crystalWidthRaw, float crystalHeightRaw)
        {
            // Очищаем список и сбрасываем индекс кристаллов.
            crystals.Clear();
            nextCrystalIndex = 1;

            // Параметры для центра пластины и её радиуса.
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;
            float radiusSquared = radius * radius;

            // Счётчик всех кристаллов.
            int totalCrystals = 0;

            // Переводим размеры кристаллов из микрометров (мкм) в миллиметры (мм).
            float crystalWidth = crystalWidthRaw / 1000;
            float crystalHeight = crystalHeightRaw / 1000;

            // Ограничиваем зону перебора прямоугольником, который точно содержит круг.
            float startX = centerX - radius;
            float endX = centerX + radius;
            float startY = centerY - radius;
            float endY = centerY + radius;

            // Определяем, сколько кристаллов может влезть по осям X и Y (без учёта формы круга).
            int numCrystalsX = (int)((endX - startX) / crystalWidth);
            int numCrystalsY = (int)((endY - startY) / crystalHeight);

            // Флаг, говорящий, что нужно развернуть очередную строку кристаллов (змейка).
            bool isReversed = false;
            // Временный список для кристаллов из одной строки.
            List<Crystal> rowCrystals = new List<Crystal>();

            // Перебираем строки (j) сверху вниз.
            for (int j = 0; j <= numCrystalsY; j++)
            {
                // Очищаем список для новой строки.
                rowCrystals.Clear();

                // Перебираем позиции (i) для кристаллов по X.
                for (int i = 0; i <= numCrystalsX; i++)
                {
                    // Находим координату центра кристалла (crystalX, crystalY).
                    float crystalX = startX + i * crystalWidth + crystalWidth / 2;
                    float crystalY = startY + j * crystalHeight + crystalHeight / 2;

                    // Проверяем, попадает ли центр кристалла в круг.
                    if (IsInsideCircle(crystalX, crystalY, centerX, centerY, radiusSquared))
                    {
                        // Создаём объект кристалла и присваиваем ему уникальный индекс.
                        Crystal crystal = new Crystal
                        {
                            Index = nextCrystalIndex++,
                            RealX = crystalX,
                            RealY = crystalY,
                            Color = Color.Blue
                        };

                        // Добавляем кристалл в текущую строку.
                        rowCrystals.Add(crystal);
                        totalCrystals++;
                    }
                }

                // Если флаг isReversed выставлен, значит эта строка идёт "змейкой",
                // и мы разворачиваем порядок кристаллов.
                if (isReversed)
                {
                    rowCrystals.Reverse();
                    // Перенумеровываем кристаллы после разворота.
                    for (int i = 0; i < rowCrystals.Count; i++)
                    {
                        rowCrystals[i].Index = nextCrystalIndex - rowCrystals.Count + i;
                    }
                }

                // Добавляем сформированную строку в общий список кристаллов.
                crystals.AddRange(rowCrystals);

                // Инвертируем флаг, чтобы следующая строка шла в обратном порядке.
                isReversed = !isReversed;
            }

            // Показываем итоговое количество сгенерированных кристаллов.
            labelTotalCrystals.Text = $"Общее количество кристаллов: {totalCrystals}";
        }

        // Метод, который рисует уже построенные кристаллы (коллекция crystals).
        private void DrawCrystals(Graphics g)
        {
            // Рассчитываем визуальную (отображаемую) ширину и высоту кристалла на экране.
            float displayCrystalWidth = (crystalWidthRaw / 1000) * scaleFactor;
            float displayCrystalHeight = (crystalHeightRaw / 1000) * scaleFactor;

            // Перебираем все кристаллы и рисуем каждый как прямоугольник.
            foreach (var crystal in crystals)
            {
                float halfW = displayCrystalWidth / 2;
                float halfH = displayCrystalHeight / 2;

                // Для отрисовки пересчитываем координаты кристалла с учётом scaleFactor.
                float centerX = pictureBox1.Width / 2;
                float centerY = pictureBox1.Height / 2;

                float scaledCrystalX = (crystal.RealX - centerX) * scaleFactor + centerX;
                float scaledCrystalY = (crystal.RealY - centerY) * scaleFactor + centerY;

                // Сохраняем "экранные" координаты кристалла.
                crystal.DisplayX = scaledCrystalX;
                crystal.DisplayY = scaledCrystalY;

                // Рисуем прямоугольник, обозначающий кристалл.
                g.DrawRectangle(Pens.Blue,
                    crystal.DisplayX - halfW,
                    crystal.DisplayY - halfH,
                    displayCrystalWidth,
                    displayCrystalHeight);

                // Если кристалл выбран (по индексу selectedCrystalIndex), выделяем его жёлтой рамкой.
                if (selectedCrystalIndex == crystal.Index)
                {
                    using (Pen selectionPen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawRectangle(selectionPen,
                            crystal.DisplayX - halfW,
                            crystal.DisplayY - halfH,
                            displayCrystalWidth,
                            displayCrystalHeight);
                    }
                }
            }

            // Если выбранный индекс больше, чем реальное количество кристаллов, сбрасываем его.
            if (selectedCrystalIndex > crystals.Count)
            {
                selectedCrystalIndex = -1;
            }
        }

        // Метод для проверки, попадает ли точка (px, py) внутрь круга (centerX, centerY, radius^2).
        private bool IsInsideCircle(float px, float py, float cx, float cy, float radiusSq)
        {
            float dx = px - cx;
            float dy = py - cy;
            // Сравниваем (dx^2 + dy^2) с radiusSq, чтобы не использовать корни.
            return (dx * dx + dy * dy) <= radiusSq;
        }

        // Реакция на перемещение курсора над pictureBox1.
        // Определяем, над каким кристаллом находится курсор, чтобы показать его индекс.
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Показываем координаты курсора (в пикселях PictureBox).
            label3.Text = $"X: {e.X}";
            label4.Text = $"Y: {e.Y}";

            // Рассчитываем размеры кристалла на экране, чтобы понять его границы.
            float displayCrystalWidth = (crystalWidthRaw / 1000) * scaleFactor;
            float displayCrystalHeight = (crystalHeightRaw / 1000) * scaleFactor;

            // Перебираем все кристаллы и проверяем, лежит ли точка курсора внутри их прямоугольника.
            foreach (var crystal in crystals)
            {
                float left = crystal.DisplayX - displayCrystalWidth / 2;
                float right = crystal.DisplayX + displayCrystalWidth / 2;
                float top = crystal.DisplayY - displayCrystalHeight / 2;
                float bottom = crystal.DisplayY + displayCrystalHeight / 2;

                if (e.X >= left && e.X <= right && e.Y >= top && e.Y <= bottom)
                {
                    // Если курсор над кристаллом, выводим его индекс и выходим из цикла.
                    labelIndex.Text = $"Индекс кристалла: {crystal.Index}";
                    return;
                }
            }

            // Если ни один кристалл не найден под курсором, сбрасываем текст.
            labelIndex.Text = "Индекс кристалла: -";
        }
    }
}
