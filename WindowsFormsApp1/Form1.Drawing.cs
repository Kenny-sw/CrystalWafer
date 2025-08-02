using CrystalTable.Data;
using CrystalTable.Logic;
using CrystalTable.Controllers;
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
        /// <summary>
        /// Обработчик события перерисовки pictureBox1
        /// </summary>
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            // Проверяем корректность ввода
            if (!IsInputValid())
            {
                return;
            }

            // Сохраняем исходное состояние графики
            GraphicsState originalState = g.Save();

            // Применяем трансформации масштабирования и панорамирования
            g.TranslateTransform(zoomPanController.PanOffset.X, zoomPanController.PanOffset.Y);
            g.ScaleTransform(zoomPanController.ZoomFactor, zoomPanController.ZoomFactor);

            // Включаем сглаживание
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Автоматическая настройка масштаба
            waferController.AutoSetScaleFactor(pictureBox1.Width, pictureBox1.Height);

            // Рисуем пластину
            DrawWafer(g);

            // Строим кристаллы с кешированием
            waferController.BuildCrystalsCached();

            // Отрисовываем кристаллы
            DrawCrystals(g);

            // Рисуем предпросмотр маршрута
            if (showRoutePreview && routePreview != null)
            {
                float centerX = pictureBox1.Width / 2;
                float centerY = pictureBox1.Height / 2;
                routePreview.DrawRoutePreview(g, CrystalManager.Instance.Crystals,
                    waferController.ScaleFactor, centerX, centerY);
            }

            // Восстанавливаем состояние графики
            g.Restore(originalState);

            // Рисуем элементы интерфейса без трансформаций
            DrawUIElements(g);

            // Отображаем информацию о масштабе
            uiController.DrawZoomInfo(g, zoomPanController.ZoomFactor);
        }

        /// <summary>
        /// Проверка корректности введённых размеров
        /// </summary>
        private bool IsInputValid()
        {
            if (float.TryParse(SizeX.Text, out float widthRaw) &&
                float.TryParse(SizeY.Text, out float heightRaw) &&
                float.TryParse(WaferDiameter.Text, out float diameterRaw) &&
                widthRaw > 0 && heightRaw > 0 &&
                diameterRaw >= WaferController.MinWaferDiameter &&
                diameterRaw <= WaferController.MaxWaferDiameter)
            {
                waferController.CrystalWidthRaw = widthRaw;
                waferController.CrystalHeightRaw = heightRaw;
                waferController.WaferDiameter = diameterRaw;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Рисование пластины
        /// </summary>
        private void DrawWafer(Graphics g)
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float radius = waferController.WaferDiameter / 2;
            float displayRadius = radius * waferController.ScaleFactor;

            if (waferController.WaferDisplayMode) // Режим схемы
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
            else // Режим визуализации
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
        /// Отрисовка кристаллов
        /// </summary>
        private void DrawCrystals(Graphics g)
        {
            float displayCrystalWidth = (waferController.CrystalWidthRaw / 1000f) * waferController.ScaleFactor;
            float displayCrystalHeight = (waferController.CrystalHeightRaw / 1000f) * waferController.ScaleFactor;
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                float halfW = displayCrystalWidth / 2;
                float halfH = displayCrystalHeight / 2;

                // Преобразуем логические координаты в экранные
                float scaledCrystalX = crystal.RealX * waferController.ScaleFactor + centerX;
                float scaledCrystalY = crystal.RealY * waferController.ScaleFactor + centerY;

                // Сохраняем экранные координаты для обработки мыши
                crystal.DisplayX = scaledCrystalX;
                crystal.DisplayY = scaledCrystalY;
                crystal.DisplayLeft = scaledCrystalX - halfW;
                crystal.DisplayRight = scaledCrystalX + halfW;
                crystal.DisplayTop = scaledCrystalY - halfH;
                crystal.DisplayBottom = scaledCrystalY + halfH;

                // Определяем состояние кристалла
                bool isSelected = mouseController.SelectedCrystals.Contains(crystal.Index);
                bool isHovered = GetHoveredCrystalIndex() == crystal.Index;

                // Выбираем цвета
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

                // Рисуем заливку
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

                // Отображаем номер кристалла при большом масштабе
                if (zoomPanController.ZoomFactor > 2.0f)
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
        }

        /// <summary>
        /// Рисование элементов интерфейса
        /// </summary>
        private void DrawUIElements(Graphics g)
        {
            // Рисуем прямоугольник выделения
            var selectionRect = mouseController.GetSelectionRectangle();
            if (mouseController.IsSelecting && !selectionRect.IsEmpty)
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
        /// Получает индекс кристалла под курсором
        /// </summary>
        private int GetHoveredCrystalIndex()
        {
            Point mousePos = pictureBox1.PointToClient(Cursor.Position);
            PointF transformedPos = zoomPanController.TransformPoint(new PointF(mousePos.X, mousePos.Y));

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                // Применяем обратную трансформацию к координатам кристалла
                PointF crystalPos = zoomPanController.InverseTransformPoint(
                    new PointF(crystal.DisplayX, crystal.DisplayY));

                float halfW = (waferController.CrystalWidthRaw / 1000f) *
                             waferController.ScaleFactor * zoomPanController.ZoomFactor / 2;
                float halfH = (waferController.CrystalHeightRaw / 1000f) *
                             waferController.ScaleFactor * zoomPanController.ZoomFactor / 2;

                if (mousePos.X >= crystalPos.X - halfW &&
                    mousePos.X <= crystalPos.X + halfW &&
                    mousePos.Y >= crystalPos.Y - halfH &&
                    mousePos.Y <= crystalPos.Y + halfH)
                {
                    return crystal.Index;
                }
            }

            return -1;
        }
    }
}