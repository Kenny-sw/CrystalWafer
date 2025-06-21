using CrystalTable.Data;
using CrystalTable.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Вычисляемые свойства для отображаемых размеров кристалла.
        private float DisplayCrystalWidth => (crystalWidthRaw / 1000f) * scaleFactor;
        private float DisplayCrystalHeight => (crystalHeightRaw / 1000f) * scaleFactor;

        // Обработчик события перерисовки pictureBox1.
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; // Объект Graphics для рисования.
            g.Clear(Color.White);    // Очищаем область рисования (заливка белым цветом).

            // Проверяем корректность ввода размеров изменяя цвет(пока не работает,т.к по дефолту False)
            if (!IsInputValid())
            {
              //SizeX.BackColor = Color.Red;
               // SizeY.BackColor = Color.Red;
                //WaferDiameter.BackColor = Color.Red;
                return;
            }

            // Подбираем коэффициент масштабирования, чтобы пластина целиком помещалась в pictureBox1.
            AutoSetScaleFactor();

            // Рисуем пластину.
            DrawWafer(g);

            // Вычисляем расположение кристаллов в логической системе координат (мм относительно центра пластины).
            BuildCrystals(crystalWidthRaw, crystalHeightRaw);

            // Отрисовываем кристаллы с преобразованием логических координат в экранные.
            DrawCrystals(g);
        }

        // Метод для подбора коэффициента масштабирования.
        private void AutoSetScaleFactor()
        {
            float picWidth = pictureBox1.Width;
            float picHeight = pictureBox1.Height;
            float minSide = Math.Min(picWidth, picHeight);

            // waferDiameter указан в мм, scaleFactor — пиксели на мм.
            scaleFactor = minSide / waferDiameter;
            // Если нужен отступ, можно уменьшить масштаб: scaleFactor *= 0.9f;
        }

        // Проверка корректности введённых размеров.
        private bool IsInputValid()
        {
            if (float.TryParse(SizeX.Text, out crystalWidthRaw) &&      //Ключевое слово out указывает, что переменная crystalWidthRaw будет использоваться для хранения результата преобразования.
                float.TryParse(SizeY.Text, out crystalHeightRaw) &&
                float.TryParse(WaferDiameter.Text, out float waferDiameterRaw) &&
                crystalWidthRaw > 0 && crystalHeightRaw > 0 &&
                waferDiameterRaw >= MinWaferDiameter && waferDiameterRaw <= MaxWaferDiameter)
            {
                waferDiameter = waferDiameterRaw;
                return true;
            }
            return false;
        }

        // Рисование пластины (окружности) по центру pictureBox1.
        private void DrawWafer(Graphics g)
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferDiameter / 2;             // Радиус в мм.
            float displayRadius = radius * scaleFactor;     // Радиус в пикселях.

            if (checkBox1.Checked == true)
            {
                // Рисуем только контур пластины
                g.DrawEllipse(Pens.Black,
                                 centerX - displayRadius,
                                 centerY - displayRadius,
                                 displayRadius * 2,
                                 displayRadius * 2);

            }
            else
            {
                // Заполняем пластину и обводим её контур
                using (Brush fillBrush = new SolidBrush(Color.Green))
                {
                    // Заполняем круг зеленым цветом
                    g.FillEllipse(fillBrush,
                                  centerX - displayRadius,
                                  centerY - displayRadius,
                                  displayRadius * 2,
                                  displayRadius * 2);

                    // Рисуем чёрный контур круга
                    g.DrawEllipse(Pens.Black,
                                  centerX - displayRadius,
                                  centerY - displayRadius,
                                  displayRadius * 2,
                                  displayRadius * 2);
                }
            }
        }
    


        // Вычисление расположения кристаллов в логической системе координат (мм относительно центра пластины).
        private void BuildCrystals(float crystalWidthRaw, float crystalHeightRaw)
        {
            CrystalManager.Instance.Crystals.Clear();
            nextCrystalIndex = 1;

            float waferRadius = waferDiameter / 2; // в мм

            // Преобразуем размеры кристаллов из исходных единиц в мм.
            float crystalWidth = crystalWidthRaw / 1000f;
            float crystalHeight = crystalHeightRaw / 1000f;

            // Задаём область размещения кристаллов в логической системе координат (центр пластины — 0,0).
            float startX = -waferRadius;
            float endX = waferRadius;
            float startY = -waferRadius;
            float endY = waferRadius;

            int numCrystalsX = (int)((endX - startX) / crystalWidth);
            int numCrystalsY = (int)((endY - startY) / crystalHeight);

            int totalCrystals = 0;
            bool isReversed = false;
            List<Crystal> rowCrystals = new List<Crystal>();

            for (int j = 0; j <= numCrystalsY; j++)
            {
                rowCrystals.Clear();
                for (int i = 0; i <= numCrystalsX; i++)
                {
                    // Вычисляем центр потенциального кристалла в логических координатах (мм).
                    float crystalX = startX + i * crystalWidth + crystalWidth / 2;
                    float crystalY = startY + j * crystalHeight + crystalHeight / 2;

                    // Добавляем кристалл, если его центр находится внутри пластины (окружности).
                    if ((crystalX * crystalX + crystalY * crystalY) <= (waferRadius * waferRadius))
                    {
                        var crystal = new Crystal
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

                CrystalManager.Instance.Crystals.AddRange(rowCrystals);
                isReversed = !isReversed;
            }

            labelTotalCrystals.Text = $"Общее количество кристаллов: {totalCrystals}";
        }

        // Отрисовка кристаллов с преобразованием логических координат (мм) в экранные координаты (пиксели).
      
        private void DrawCrystals(Graphics g)
        {
            //if()


            float displayCrystalWidth = (crystalWidthRaw / 1000f) * scaleFactor;
            float displayCrystalHeight = (crystalHeightRaw / 1000f) * scaleFactor;
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                float halfW = displayCrystalWidth / 2;
                float halfH = displayCrystalHeight / 2;

                // Преобразуем логические координаты кристалла (мм) в экранные координаты (пиксели).
                float scaledCrystalX = crystal.RealX * scaleFactor + centerX;
                float scaledCrystalY = crystal.RealY * scaleFactor + centerY;
                crystal.DisplayX = scaledCrystalX;
                crystal.DisplayY = scaledCrystalY;

                // Вычисляем границы кристалла для обработки MouseMove и MouseDown.
                crystal.DisplayLeft = scaledCrystalX - halfW;
                crystal.DisplayRight = scaledCrystalX + halfW;
                crystal.DisplayTop = scaledCrystalY - halfH;
                crystal.DisplayBottom = scaledCrystalY + halfH;

               

                // Отрисовка: если кристалл выбран, заливаем жёлтым, иначе рисуем синим контуром.
                if (selectedCrystalIndex == crystal.Index)
                {
                    using (Brush fillBrush = new SolidBrush(Color.Yellow))
                    {
                        g.FillRectangle(fillBrush,
                            crystal.DisplayLeft,
                            crystal.DisplayTop,
                            displayCrystalWidth,
                            displayCrystalHeight);
                    }
                }
                else
                {
                    g.DrawRectangle(Pens.Blue,
                        crystal.DisplayLeft,
                        crystal.DisplayTop,
                        displayCrystalWidth,
                        displayCrystalHeight);
                }
            }

            if (selectedCrystalIndex > CrystalManager.Instance.Crystals.Count)
            {
                selectedCrystalIndex = -1;
            }
        }

        // Проверка принадлежности точки кругу.
        private bool IsInsideCircle(float px, float py, float cx, float cy, float radiusSq)
        {
            float dx = px - cx;
            float dy = py - cy;
            return (dx * dx + dy * dy) <= radiusSq;
        }

        // Обработчик перемещения мыши по pictureBox1.
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label3.Text = $"X: {e.X}";
            label4.Text = $"Y: {e.Y}";

            // Проверяем, попадает ли курсор в область какого-либо кристалла.
            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                if (e.X >= crystal.DisplayLeft && e.X <= crystal.DisplayRight &&
                    e.Y >= crystal.DisplayTop && e.Y <= crystal.DisplayBottom)
                {
                    labelIndex.Text = $"Индекс кристалла: {crystal.Index}";
                    return;
                }
            }

            labelIndex.Text = "Индекс кристалла: -";
        }

        // Обработчик нажатия кнопки мыши в pictureBox1.
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Перебираем кристаллы в обратном порядке (для выбора верхнего при перекрытии).
            for (int i = CrystalManager.Instance.Crystals.Count - 1; i >= 0; i--)
            {
                var crystal = CrystalManager.Instance.Crystals[i];

                if (e.X >= crystal.DisplayLeft && e.X <= crystal.DisplayRight &&
                    e.Y >= crystal.DisplayTop && e.Y <= crystal.DisplayBottom)
                {
                    selectedCrystalIndex = crystal.Index;
                    labelSelectedCrystal.Text = $"Текущий индекс кристалла: {Convert.ToString(selectedCrystalIndex)}"; // Обновляем лейбл здесь
                    pictureBox1.Invalidate(); // Обновляем отрисовку.
                    return;
                }
            }

            // Если ни один кристалл не выбран, сбрасываем индекс и обновляем лейбл.
            selectedCrystalIndex = -1;
            labelSelectedCrystal.Text = Convert.ToString(selectedCrystalIndex);
            pictureBox1.Invalidate();
        }

    }
}
