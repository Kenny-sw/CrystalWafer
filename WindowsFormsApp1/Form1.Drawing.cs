using CrystalTable.Logic;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            if (!IsInputValid()) return;

            // трансформации
            var st = g.Save();
            g.TranslateTransform(zoomPanController.PanOffset.X, zoomPanController.PanOffset.Y);
            g.ScaleTransform(zoomPanController.ZoomFactor, zoomPanController.ZoomFactor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            waferController.AutoSetScaleFactor(pictureBox1.Width, pictureBox1.Height);

            DrawWafer(g);

            // карта теперь НЕ строится в Paint — только рисуем
            DrawCrystals(g);

            // Подсветка First/Last (до BuildMap)
            DrawReferenceMarkers(g);

            // Указатель (адаптивного размера)
            DrawPointer(g);

            // Маршрут
            if (showRoutePreview && routePreview != null)
            {
                float cx = pictureBox1.Width / 2f, cy = pictureBox1.Height / 2f;
                routePreview.DrawRoutePreview(g, CrystalManager.Instance.Crystals, waferController.ScaleFactor, cx, cy);
            }

            g.Restore(st);

            DrawUIElements(g);
            uiController.DrawZoomInfo(g, zoomPanController.ZoomFactor);
        }

        private bool IsInputValid()
        {
            // ✅ Проверка через WaferController
            if (waferController.CrystalWidthRaw == 0 || waferController.CrystalHeightRaw == 0)
                return false;

            float diameter = waferController.WaferDiameter;
            if (diameter < Controllers.WaferController.MinWaferDiameter || 
                diameter > Controllers.WaferController.MaxWaferDiameter)
                return false;

            return true;
        }

        private void DrawWafer(Graphics g)
        {
            float cx = pictureBox1.Width / 2f, cy = pictureBox1.Height / 2f;
            float rMm = waferController.WaferDiameter / 2f;
            float rPx = rMm * waferController.ScaleFactor;

            if (waferController.WaferDisplayMode)
            {
                using var pen = new Pen(Color.Black, 2);
                g.DrawEllipse(pen, cx - rPx, cy - rPx, rPx * 2, rPx * 2);
            }
            else
            {
                using var fill = new SolidBrush(Color.LightGreen);
                using var pen = new Pen(Color.DarkGreen, 2);
                g.FillEllipse(fill, cx - rPx, cy - rPx, rPx * 2, rPx * 2);
                g.DrawEllipse(pen, cx - rPx, cy - rPx, rPx * 2, rPx * 2);
            }
        }

        private void DrawCrystals(Graphics g)
        {
            waferController.UpdateDisplayCache(pictureBox1.Width, pictureBox1.Height);

            var baseMap = waferController.GetWaferBitmap(pictureBox1.Width, pictureBox1.Height);
            if (baseMap != null)
            {
                g.DrawImageUnscaled(baseMap, Point.Empty);
            }

            var client = new RectangleF(0, 0, pictureBox1.Width, pictureBox1.Height);
            int drawnInView = 0;
            int hoveredIndex = GetHoveredCrystalIndex();
            var selected = mouseController.SelectedCrystals;

            using var selectedFill = new SolidBrush(Color.FromArgb(60, Color.Khaki));
            using var selectedBorder = new Pen(Color.FromArgb(220, Color.DarkGoldenrod), 2f);
            using var hoveredFill = new SolidBrush(Color.FromArgb(40, Color.LightSkyBlue));
            using var hoveredBorder = new Pen(Color.FromArgb(200, Color.RoyalBlue), 1.5f);

            foreach (var c in CrystalManager.Instance.Crystals)
            {
                if (c.DisplayRight <= client.Left || c.DisplayLeft >= client.Right ||
                    c.DisplayBottom <= client.Top || c.DisplayTop >= client.Bottom)
                {
                    continue;
                }

                drawnInView++;

                bool isSelected = selected.Contains(c.Index);
                bool isHovered = hoveredIndex == c.Index;
                if (!isSelected && !isHovered)
                {
                    continue;
                }

                float w = c.DisplayRight - c.DisplayLeft;
                float h = c.DisplayBottom - c.DisplayTop;

                if (isSelected)
                {
                    g.FillRectangle(selectedFill, c.DisplayLeft, c.DisplayTop, w, h);
                    g.DrawRectangle(selectedBorder, c.DisplayLeft, c.DisplayTop, w, h);
                }
                else
                {
                    g.FillRectangle(hoveredFill, c.DisplayLeft, c.DisplayTop, w, h);
                    g.DrawRectangle(hoveredBorder, c.DisplayLeft, c.DisplayTop, w, h);
                }
            }

            if (zoomPanController.ZoomFactor > 4.0f && drawnInView <= 200)
            {
                using var font = new Font("Arial", 8);
                using var tb = new SolidBrush(Color.Black);

                foreach (var c in CrystalManager.Instance.Crystals)
                {
                    if (c.DisplayRight <= 0 || c.DisplayLeft >= pictureBox1.Width ||
                        c.DisplayBottom <= 0 || c.DisplayTop >= pictureBox1.Height)
                    {
                        continue;
                    }

                    string text = c.Index.ToString();
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, tb, c.DisplayX - size.Width / 2f, c.DisplayY - size.Height / 2f);
                }
            }
        }
        private void DrawReferenceMarkers(Graphics g)
        {
            float cx = pictureBox1.Width / 2f, cy = pictureBox1.Height / 2f;

            float rMm = 0.4f * Math.Min(waferController.StepXmmOrDefault, waferController.StepYmmOrDefault);
            rMm = Math.Max(0.1f, Math.Min(1.0f, rMm));
            float r = rMm * waferController.ScaleFactor;

            if (waferController.HasFirstRef)
            {
                float x = waferController.FirstRefMm.X * waferController.ScaleFactor + cx;
                float y = waferController.FirstRefMm.Y * waferController.ScaleFactor + cy;
                using var br = new SolidBrush(Color.FromArgb(120, Color.Lime));
                using var pen = new Pen(Color.Green, 2);
                g.FillEllipse(br, x - r, y - r, 2 * r, 2 * r);
                g.DrawEllipse(pen, x - r, y - r, 2 * r, 2 * r);
            }

            if (waferController.HasLastRef)
            {
                float x = waferController.LastRefMm.X * waferController.ScaleFactor + cx;
                float y = waferController.LastRefMm.Y * waferController.ScaleFactor + cy;
                using var br = new SolidBrush(Color.FromArgb(120, Color.Orange));
                using var pen = new Pen(Color.DarkOrange, 2);
                g.FillEllipse(br, x - r, y - r, 2 * r, 2 * r);
                g.DrawEllipse(pen, x - r, y - r, 2 * r, 2 * r);
            }
        }

        private void DrawPointer(Graphics g)
        {
            var p = GetPointerMm();  // ← Исправлено

            float cx = pictureBox1.Width / 2f, cy = pictureBox1.Height / 2f;
            float x = p.X * waferController.ScaleFactor + cx;
            float y = p.Y * waferController.ScaleFactor + cy;

            // адаптивный размер: 0.3 шага, но не меньше 0.1 мм и не больше 1 мм
            float baseStep = Math.Max(waferController.StepXmmOrDefault, 0.0001f);
            baseStep = Math.Min(baseStep, waferController.StepYmmOrDefault > 0 ? waferController.StepYmmOrDefault : baseStep);
            float rMm = Math.Max(0.1f, Math.Min(1.0f, 0.3f * baseStep));
            float r = rMm * waferController.ScaleFactor;
            float arm = r * 1.6f;

            using var pen = new Pen(Color.DarkRed, 2);
            using var br = new SolidBrush(Color.FromArgb(60, Color.Red));
            g.FillEllipse(br, x - r, y - r, 2 * r, 2 * r);
            g.DrawEllipse(pen, x - r, y - r, 2 * r, 2 * r);
            g.DrawLine(pen, x - arm, y, x + arm, y);
            g.DrawLine(pen, x, y - arm, x, y + arm);
        }

        private void DrawUIElements(Graphics g)
        {
            var rect = mouseController.GetSelectionRectangle();
            if (mouseController.IsSelecting && !rect.IsEmpty)
            {
                using var pen = new Pen(Color.FromArgb(128, Color.Blue), 2) { DashStyle = DashStyle.Dash };
                using var br = new SolidBrush(Color.FromArgb(30, Color.Blue));
                g.DrawRectangle(pen, rect);
                g.FillRectangle(br, rect);
            }
        }

        private int GetHoveredCrystalIndex()
        {
            if (pictureBox1 == null) return -1;

            var mouseScreen = pictureBox1.PointToClient(Cursor.Position);
            var transformed = zoomPanController.TransformPoint(new PointF(mouseScreen.X, mouseScreen.Y));

            foreach (var c in CrystalManager.Instance.Crystals)
            {
                if (transformed.X >= c.DisplayLeft && transformed.X <= c.DisplayRight &&
                    transformed.Y >= c.DisplayTop && transformed.Y <= c.DisplayBottom)
                {
                    return c.Index;
                }
            }

            return -1;
        }
    }
}

