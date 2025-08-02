using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для управления отрисовкой
    /// </summary>
    public class DrawingController
    {
        private readonly WaferController waferController;
        private readonly ZoomPanController zoomPanController;
        private readonly MouseController mouseController;
        private readonly RoutePreview routePreview;

        public DrawingController(WaferController waferController,
            ZoomPanController zoomPanController,
            MouseController mouseController,
            RoutePreview routePreview)
        {
            this.waferController = waferController;
            this.zoomPanController = zoomPanController;
            this.mouseController = mouseController;
            this.routePreview = routePreview;
        }

        /// <summary>
        /// Основной метод отрисовки
        /// </summary>
        public void Draw(Graphics g, int width, int height, bool showRoute)
        {
            // Очистка фона
            g.Clear(Color.White);

            // Сохраняем состояние
            GraphicsState originalState = g.Save();

            // Применяем трансформации
            ApplyTransformations(g);

            // Рисуем содержимое
            DrawContent(g, width, height, showRoute);

            // Восстанавливаем состояние
            g.Restore(originalState);

            // Рисуем UI элементы
            DrawUIOverlay(g, width, height);
        }

        /// <summary>
        /// Применение трансформаций
        /// </summary>
        private void ApplyTransformations(Graphics g)
        {
            g.TranslateTransform(zoomPanController.PanOffset.X, zoomPanController.PanOffset.Y);
            g.ScaleTransform(zoomPanController.ZoomFactor, zoomPanController.ZoomFactor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
        }

        /// <summary>
        /// Отрисовка основного содержимого
        /// </summary>
        private void DrawContent(Graphics g, int width, int height, bool showRoute)
        {
            waferController.AutoSetScaleFactor(width, height);

            // Рисуем пластину
            DrawWafer(g, width, height);

            // Строим и рисуем кристаллы
            waferController.BuildCrystalsCached();
            DrawCrystals(g, width, height);

            // Рисуем маршрут
            if (showRoute && routePreview != null)
            {
                float centerX = width / 2;
                float centerY = height / 2;
                routePreview.DrawRoutePreview(g, CrystalManager.Instance.Crystals,
                    waferController.ScaleFactor, centerX, centerY);
            }
        }

        /// <summary>
        /// Отрисовка пластины
        /// </summary>
        private void DrawWafer(Graphics g, int width, int height)
        {
            float centerX = width / 2;
            float centerY = height / 2;
            float radius = waferController.WaferDiameter / 2;
            float displayRadius = radius * waferController.ScaleFactor;

            if (waferController.WaferDisplayMode)
            {
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
                using (Brush fillBrush = new SolidBrush(Color.LightGreen))
                {
                    g.FillEllipse(fillBrush,
                        centerX - displayRadius,
                        centerY - displayRadius,
                        displayRadius * 2,
                        displayRadius * 2);
                }

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

        /// <summary>
        /// Отрисовка кристаллов с оптимизацией
        /// </summary>
        private void DrawCrystals(Graphics g, int width, int height)
        {
            float displayCrystalWidth = (waferController.CrystalWidthRaw / 1000f) * waferController.ScaleFactor;
            float displayCrystalHeight = (waferController.CrystalHeightRaw / 1000f) * waferController.ScaleFactor;
            float centerX = width / 2;
            float centerY = height / 2;

            // Определяем видимую область с учетом трансформаций
            RectangleF visibleArea = GetVisibleArea(width, height);

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                // Вычисляем позицию кристалла
                float halfW = displayCrystalWidth / 2;
                float halfH = displayCrystalHeight / 2;
                float scaledCrystalX = crystal.RealX * waferController.ScaleFactor + centerX;
                float scaledCrystalY = crystal.RealY * waferController.ScaleFactor + centerY;

                // Сохраняем координаты
                crystal.DisplayX = scaledCrystalX;
                crystal.DisplayY = scaledCrystalY;
                crystal.DisplayLeft = scaledCrystalX - halfW;
                crystal.DisplayRight = scaledCrystalX + halfW;
                crystal.DisplayTop = scaledCrystalY - halfH;
                crystal.DisplayBottom = scaledCrystalY + halfH;

                // Пропускаем кристаллы вне видимой области
                if (!IsInVisibleArea(crystal, visibleArea))
                    continue;

                // Рисуем кристалл
                DrawCrystal(g, crystal, displayCrystalWidth, displayCrystalHeight);
            }
        }

        /// <summary>
        /// Отрисовка одного кристалла
        /// </summary>
        private void DrawCrystal(Graphics g, Crystal crystal, float width, float height)
        {
            bool isSelected = mouseController.SelectedCrystals.Contains(crystal.Index);

            // Определяем цвета
            Color fillColor = isSelected ? Color.Yellow : Color.Empty;
            Color borderColor = isSelected ? Color.DarkBlue : Color.Blue;
            float borderWidth = isSelected ? 2 : 1;

            // Заливка
            if (fillColor != Color.Empty)
            {
                using (Brush fillBrush = new SolidBrush(fillColor))
                {
                    g.FillRectangle(fillBrush,
                        crystal.DisplayLeft,
                        crystal.DisplayTop,
                        width,
                        height);
                }
            }

            // Контур
            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                g.DrawRectangle(pen,
                    crystal.DisplayLeft,
                    crystal.DisplayTop,
                    width,
                    height);
            }

            // Номер кристалла с учетом масштаба
            DrawCrystalNumber(g, crystal, width, height);
        }

        /// <summary>
        /// Отрисовка номера кристалла с оптимизацией
        /// </summary>
        private void DrawCrystalNumber(Graphics g, Crystal crystal, float width, float height)
        {
            float zoom = zoomPanController.ZoomFactor;

            // Не показываем номера при мелком масштабе
            if (zoom < 1.5f)
                return;

            // Адаптивный размер шрифта
            float fontSize = Math.Max(6, Math.Min(12, 8 * zoom));

            // Показываем только каждый N-й номер при среднем масштабе
            if (zoom < 3.0f && crystal.Index % 4 != 0)
                return;

            using (Font font = new Font("Arial", fontSize))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                string text = crystal.Index.ToString();
                SizeF textSize = g.MeasureString(text, font);

                // Проверяем, помещается ли текст в кристалл
                if (textSize.Width > width * 0.8f || textSize.Height > height * 0.8f)
                    return;

                float textX = crystal.DisplayX - textSize.Width / 2;
                float textY = crystal.DisplayY - textSize.Height / 2;
                g.DrawString(text, font, textBrush, textX, textY);
            }
        }

        /// <summary>
        /// Отрисовка UI элементов поверх основного содержимого
        /// </summary>
        private void DrawUIOverlay(Graphics g, int width, int height)
        {
            // Прямоугольник выделения
            DrawSelectionRectangle(g);

            // Информация о масштабе
            DrawZoomInfo(g, width, height);
        }

        /// <summary>
        /// Отрисовка прямоугольника выделения
        /// </summary>
        private void DrawSelectionRectangle(Graphics g)
        {
            var selectionRect = mouseController.GetSelectionRectangle();
            if (!selectionRect.IsEmpty)
            {
                using (Pen selectionPen = new Pen(Color.FromArgb(128, Color.Blue), 2))
                {
                    selectionPen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(selectionPen, selectionRect);
                }

                using (Brush selectionBrush = new SolidBrush(Color.FromArgb(30, Color.Blue)))
                {
                    g.FillRectangle(selectionBrush, selectionRect);
                }
            }
        }

        /// <summary>
        /// Отображение информации о масштабе
        /// </summary>
        private void DrawZoomInfo(Graphics g, int width, int height)
        {
            string zoomText = $"Масштаб: {zoomPanController.ZoomFactor:F1}x";
            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                SizeF textSize = g.MeasureString(zoomText, font);
                float x = width - textSize.Width - 10;
                float y = height - textSize.Height - 10;

                using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                {
                    g.FillRectangle(bgBrush, x - 5, y - 5, textSize.Width + 10, textSize.Height + 10);
                }

                g.DrawString(zoomText, font, brush, x, y);
            }
        }

        /// <summary>
        /// Получение видимой области с учетом трансформаций
        /// </summary>
        private RectangleF GetVisibleArea(int width, int height)
        {
            PointF topLeft = zoomPanController.TransformPoint(new PointF(0, 0));
            PointF bottomRight = zoomPanController.TransformPoint(new PointF(width, height));

            return new RectangleF(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y
            );
        }

        /// <summary>
        /// Проверка, находится ли кристалл в видимой области
        /// </summary>
        private bool IsInVisibleArea(Crystal crystal, RectangleF visibleArea)
        {
            return crystal.DisplayLeft <= visibleArea.Right &&
                   crystal.DisplayRight >= visibleArea.Left &&
                   crystal.DisplayTop <= visibleArea.Bottom &&
                   crystal.DisplayBottom >= visibleArea.Top;
        }
    }
}