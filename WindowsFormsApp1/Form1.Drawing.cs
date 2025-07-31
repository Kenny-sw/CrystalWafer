using CrystalTable.Data;
using CrystalTable.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Вычисляемые свойства для отображаемых размеров кристалла.
        private float DisplayCrystalWidth => (crystalWidthRaw / 1000f) * scaleFactor;
        private float DisplayCrystalHeight => (crystalHeightRaw / 1000f) * scaleFactor;

        // Обработчик события перерисовки pictureBox1.
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; // Объект Graphics для рисования.
            g.Clear(Color.White);    // Очищаем область рисования (заливка белым цветом).

            // Проверяем корректность ввода размеров
            if (!IsInputValid())
            {
                return;
            }

            // ===== ПРИМЕНЯЕМ ТРАНСФОРМАЦИИ ДЛЯ МАСШТАБИРОВАНИЯ И ПАНОРАМИРОВАНИЯ =====

            // Сохраняем исходное состояние графики
            GraphicsState originalState = g.Save();

            // Применяем трансформации
            g.TranslateTransform(panOffset.X, panOffset.Y);
            g.ScaleTransform(zoomFactor, zoomFactor);

            // Включаем сглаживание для лучшего качества при масштабировании
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Подбираем коэффициент масштабирования, чтобы пластина целиком помещалась в pictureBox1.
            AutoSetScaleFactor();

            // Рисуем пластину.
            DrawWafer(g);

            // Вычисляем расположение кристаллов с учетом кеша.
            BuildCrystalsCached(crystalWidthRaw, crystalHeightRaw);

            // Отрисовываем кристаллы
            DrawCrystals(g);

            // Рисуем предпросмотр маршрута, если включен
            if (showRoutePreview && routePreview != null)
            {
                float centerX = pictureBox1.Width / 2;
                float centerY = pictureBox1.Height / 2;
                routePreview.DrawRoutePreview(g, CrystalManager.Instance.Crystals,
                    scaleFactor, centerX, centerY);
            }

            // Восстанавливаем исходное состояние графики
            g.Restore(originalState);

            // ===== РИСУЕМ ЭЛЕМЕНТЫ ИНТЕРФЕЙСА БЕЗ ТРАНСФОРМАЦИЙ =====

            // Рисуем прямоугольник выделения (без трансформаций)
            if (isSelecting && selectionRectangle.Width > 0 && selectionRectangle.Height > 0)
            {
                using (Pen selectionPen = new Pen(Color.FromArgb(128, Color.Blue), 2))
                {
                    selectionPen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(selectionPen, selectionRectangle);
                }

                // Полупрозрачная заливка области выделения
                using (Brush selectionBrush = new SolidBrush(Color.FromArgb(30, Color.Blue)))
                {
                    g.FillRectangle(selectionBrush, selectionRectangle);
                }
            }

            // Отображаем информацию о масштабе
            DrawZoomInfo(g);
        }

        /// <summary>
        /// Отображает информацию о текущем масштабе
        /// </summary>
        private void DrawZoomInfo(Graphics g)
        {
            string zoomText = $"Масштаб: {zoomFactor:F1}x";
            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                SizeF textSize = g.MeasureString(zoomText, font);
                float x = pictureBox1.Width - textSize.Width - 10;
                float y = pictureBox1.Height - textSize.Height - 10;

                // Фон для текста
                using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                {
                    g.FillRectangle(bgBrush, x - 5, y - 5, textSize.Width + 10, textSize.Height + 10);
                }

                g.DrawString(zoomText, font, brush, x, y);
            }
        }

        // Метод для подбора коэффициента масштабирования.
        private void AutoSetScaleFactor()
        {
            float picWidth = pictureBox1.Width;
            float picHeight = pictureBox1.Height;
            float minSide = Math.Min(picWidth, picHeight);

            // waferDiameter указан в мм, scaleFactor — пиксели на мм.
            scaleFactor = minSide / waferDiameter * 0.9f; // 0.9 для отступов
        }

        // Проверка корректности введённых размеров.
        private bool IsInputValid()
        {
            if (float.TryParse(SizeX.Text, out crystalWidthRaw) &&
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

            if (checkBoxFillWafer.Checked)
            {
                // Режим схемы - только контур
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.DrawEllipse(pen,
                                     centerX - displayRadius,
                                     centerY - displayRadius,
                                     displayRadius * 2,
                                     displayRadius * 2);
                }
            }
            else
            {
                // Режим визуализации - заливка
                using (Brush fillBrush = new SolidBrush(Color.LightGreen))
                {
                    // Заполняем круг зеленым цветом
                    g.FillEllipse(fillBrush,
                                  centerX - displayRadius,
                                  centerY - displayRadius,
                                  displayRadius * 2,
                                  displayRadius * 2);
                }

                // Рисуем чёрный контур круга
                using (Pen pen = new Pen(Color.DarkGreen, 2))
                {
                    g.DrawEllipse(pen,
                                  centerX - displayRadius,
                                  centerY - displayRadius,
                                  displayRadius * 2,
                                  displayRadius * 2);
                }
            }
        }

        // Построение кристаллов с кешированием.
        private void BuildCrystalsCached(float crystalWidthRaw, float crystalHeightRaw)
        {
            if (crystalWidthRaw == lastCrystalWidthRaw &&
                crystalHeightRaw == lastCrystalHeightRaw &&
                waferDiameter == lastWaferDiameter &&
                CrystalManager.Instance.Crystals.Count > 0)
            {
                return; // Кеш актуален
            }

            string cachePath = CrystalCache.GetCacheFilePath(crystalWidthRaw, crystalHeightRaw, waferDiameter);
            if (CrystalCache.TryLoad(cachePath, out var cachedCrystals))
            {
                CrystalManager.Instance.Crystals.Clear();
                CrystalManager.Instance.Crystals.AddRange(cachedCrystals);
            }
            else
            {
                BuildCrystals(crystalWidthRaw, crystalHeightRaw);

                var info = new WaferInfo
                {
                    SizeX = (uint)crystalWidthRaw,
                    SizeY = (uint)crystalHeightRaw,
                    WaferDiameter = (uint)waferDiameter
                };
                CrystalCache.Save(cachePath, info, CrystalManager.Instance.Crystals);
            }

            lastCrystalWidthRaw = crystalWidthRaw;
            lastCrystalHeightRaw = crystalHeightRaw;
            lastWaferDiameter = waferDiameter;
        }

        // Вычисление расположения кристаллов в логической системе координат.
        private void BuildCrystals(float crystalWidthRaw, float crystalHeightRaw)
        {
            CrystalManager.Instance.Crystals.Clear();
            nextCrystalIndex = 1;

            float waferRadius = waferDiameter / 2; // в мм

            // Преобразуем размеры кристаллов из микрометров в мм.
            float crystalWidth = crystalWidthRaw / 1000f;
            float crystalHeight = crystalHeightRaw / 1000f;

            // Задаём область размещения кристаллов в логической системе координат.
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

                    // Добавляем кристалл, если его центр находится внутри пластины.
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

        // Отрисовка кристаллов с поддержкой множественного выделения
        private void DrawCrystals(Graphics g)
        {
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

                // ===== УЛУЧШЕННАЯ ОТРИСОВКА С ПОДДЕРЖКОЙ МНОЖЕСТВЕННОГО ВЫДЕЛЕНИЯ =====

                // Определяем состояние кристалла
                bool isSelected = selectedCrystals.Contains(crystal.Index);
                bool isHovered = crystal.Index == GetHoveredCrystalIndex();

                // Выбираем цвета на основе состояния
                Color fillColor = Color.Empty;
                Color borderColor = Color.Blue;
                float borderWidth = 1;

                if (isSelected)
                {
                    fillColor = Color.Yellow;
                    borderColor = Color.DarkBlue;
                    borderWidth = 2;
                }
                else if (isHovered)
                {
                    fillColor = Color.LightBlue;
                    borderColor = Color.DarkBlue;
                }

                // Рисуем заливку, если нужно
                if (fillColor != Color.Empty)
                {
                    using (Brush fillBrush = new SolidBrush(fillColor))
                    {
                        g.FillRectangle(fillBrush,
                            crystal.DisplayLeft,
                            crystal.DisplayTop,
                            displayCrystalWidth,
                            displayCrystalHeight);
                    }
                }

                // Рисуем контур
                using (Pen pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawRectangle(pen,
                        crystal.DisplayLeft,
                        crystal.DisplayTop,
                        displayCrystalWidth,
                        displayCrystalHeight);
                }

                // Отображаем номер кристалла, если масштаб достаточно большой
                if (zoomFactor > 2.0f)
                {
                    using (Font font = new Font("Arial", 8))
                    using (Brush textBrush = new SolidBrush(Color.Black))
                    {
                        string text = crystal.Index.ToString();
                        SizeF textSize = g.MeasureString(text, font);
                        float textX = scaledCrystalX - textSize.Width / 2;
                        float textY = scaledCrystalY - textSize.Height / 2;
                        g.DrawString(text, font, textBrush, textX, textY);
                    }
                }
            }

            // Обновляем индекс выбранного кристалла
            if (selectedCrystals.Count == 1)
            {
                selectedCrystalIndex = selectedCrystals.First();
            }
            else if (selectedCrystals.Count == 0)
            {
                selectedCrystalIndex = -1;
            }
        }

        /// <summary>
        /// Получает индекс кристалла под курсором мыши
        /// </summary>
        private int GetHoveredCrystalIndex()
        {
            Point mousePos = pictureBox1.PointToClient(Cursor.Position);
            PointF transformedPos = TransformMousePoint(mousePos);

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                if (transformedPos.X >= crystal.DisplayLeft &&
                    transformedPos.X <= crystal.DisplayRight &&
                    transformedPos.Y >= crystal.DisplayTop &&
                    transformedPos.Y <= crystal.DisplayBottom)
                {
                    return crystal.Index;
                }
            }

            return -1;
        }

        // Проверка принадлежности точки кругу.
        private bool IsInsideCircle(float px, float py, float cx, float cy, float radiusSq)
        {
            float dx = px - cx;
            float dy = py - cy;
            return (dx * dx + dy * dy) <= radiusSq;
        }

        // ===== ОБНОВЛЕННЫЕ ОБРАБОТЧИКИ МЫШИ С ПОДДЕРЖКОЙ ТРАНСФОРМАЦИЙ =====

        // Обработчик перемещения мыши по pictureBox1.
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label3.Text = $"X: {e.X}";
            label4.Text = $"Y: {e.Y}";

            // Обработка панорамирования
            if (isPanning && e.Button == MouseButtons.Middle)
            {
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;

                panOffset.X += deltaX;
                panOffset.Y += deltaY;

                lastMousePosition = e.Location;
                pictureBox1.Invalidate();
                return;
            }

            // Обработка выделения области
            if (isSelecting)
            {
                // Обновляем прямоугольник выделения
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int width = Math.Abs(e.X - selectionStart.X);
                int height = Math.Abs(e.Y - selectionStart.Y);

                selectionRectangle = new Rectangle(x, y, width, height);

                // Обновляем выбранные кристаллы с учетом трансформаций
                selectedCrystals.Clear();

                foreach (var crystal in CrystalManager.Instance.Crystals)
                {
                    // Преобразуем координаты кристалла в экранные с учетом трансформаций
                    float screenX = crystal.DisplayX * zoomFactor + panOffset.X;
                    float screenY = crystal.DisplayY * zoomFactor + panOffset.Y;
                    float screenWidth = DisplayCrystalWidth * zoomFactor;
                    float screenHeight = DisplayCrystalHeight * zoomFactor;

                    Rectangle crystalRect = new Rectangle(
                        (int)(screenX - screenWidth / 2),
                        (int)(screenY - screenHeight / 2),
                        (int)screenWidth,
                        (int)screenHeight
                    );

                    if (selectionRectangle.IntersectsWith(crystalRect))
                    {
                        selectedCrystals.Add(crystal.Index);
                    }
                }

                UpdateSelectionLabel();
                pictureBox1.Invalidate();
                return;
            }

            // Отображение информации о кристалле под курсором
            PointF transformedPoint = TransformMousePoint(e.Location);

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                if (transformedPoint.X >= crystal.DisplayLeft &&
                    transformedPoint.X <= crystal.DisplayRight &&
                    transformedPoint.Y >= crystal.DisplayTop &&
                    transformedPoint.Y <= crystal.DisplayBottom)
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
            // Обработка панорамирования средней кнопкой
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = true;
                lastMousePosition = e.Location;
                pictureBox1.Cursor = Cursors.Hand;
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                PointF transformedPoint = TransformMousePoint(e.Location);
                bool hitCrystal = false;

                // Проверяем, попали ли в кристалл
                for (int i = CrystalManager.Instance.Crystals.Count - 1; i >= 0; i--)
                {
                    var crystal = CrystalManager.Instance.Crystals[i];

                    if (transformedPoint.X >= crystal.DisplayLeft &&
                        transformedPoint.X <= crystal.DisplayRight &&
                        transformedPoint.Y >= crystal.DisplayTop &&
                        transformedPoint.Y <= crystal.DisplayBottom)
                    {
                        hitCrystal = true;
                        int oldSelectedIndex = selectedCrystalIndex;

                        if (isCtrlPressed)
                        {
                            // Множественный выбор с Ctrl
                            if (selectedCrystals.Contains(crystal.Index))
                            {
                                selectedCrystals.Remove(crystal.Index);
                            }
                            else
                            {
                                selectedCrystals.Add(crystal.Index);
                            }
                        }
                        else
                        {
                            // Одиночный выбор - сохраняем в историю
                            var oldSelection = new HashSet<int>(selectedCrystals);
                            selectedCrystals.Clear();
                            selectedCrystals.Add(crystal.Index);
                            selectedCrystalIndex = crystal.Index;

                            // Добавляем команду в историю
                            commandHistory.ExecuteCommand(
                                new SelectCrystalCommand(
                                    oldSelectedIndex,
                                    crystal.Index,
                                    (index) => {
                                        selectedCrystalIndex = index;
                                        selectedCrystals.Clear();
                                        if (index > 0) selectedCrystals.Add(index);
                                    },
                                    () => pictureBox1.Invalidate()
                                )
                            );
                        }

                        UpdateSelectionLabel();
                        pictureBox1.Invalidate();
                        return;
                    }
                }

                // Если не попали в кристалл, начинаем выделение области
                if (!hitCrystal && !isCtrlPressed)
                {
                    isSelecting = true;
                    selectionStart = e.Location;
                    selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                    selectedCrystals.Clear();
                    UpdateSelectionLabel();
                }
            }
        }

        // Обработчик отпускания кнопки мыши
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = false;
                pictureBox1.Cursor = Cursors.Default;
            }
            else if (e.Button == MouseButtons.Left && isSelecting)
            {
                isSelecting = false;
                UpdateSelectionLabel();
                pictureBox1.Invalidate();
            }
        }
    }
}