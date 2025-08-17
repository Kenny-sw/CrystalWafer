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
            if (!uint.TryParse(SizeX.Text.Trim(), out var w)) return false;
            if (!uint.TryParse(SizeY.Text.Trim(), out var h)) return false;
            if (!float.TryParse(WaferDiameter.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return false;

            if (w == 0 || h == 0) return false;
            if (d < Controllers.WaferController.MinWaferDiameter || d > Controllers.WaferController.MaxWaferDiameter) return false;

            waferController.CrystalWidthRaw = w;
            waferController.CrystalHeightRaw = h;
            waferController.WaferDiameter = d;
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
            float wPx = (waferController.CrystalWidthRaw / 1000f) * waferController.ScaleFactor;
            float hPx = (waferController.CrystalHeightRaw / 1000f) * waferController.ScaleFactor;
            float cx = pictureBox1.Width / 2f, cy = pictureBox1.Height / 2f;

            var client = new RectangleF(0, 0, pictureBox1.Width, pictureBox1.Height);
            int drawnInView = 0;

            foreach (var c in CrystalManager.Instance.Crystals)
            {
                float x = c.RealX * waferController.ScaleFactor + cx;
                float y = c.RealY * waferController.ScaleFactor + cy;

                float left = x - wPx / 2f, top = y - hPx / 2f;

                // вьюпорт-куллинг
                if (left > client.Right || top > client.Bottom || left + wPx < client.Left || top + hPx < client.Top)
                    continue;

                c.DisplayX = x; c.DisplayY = y;
                c.DisplayLeft = left; c.DisplayTop = top;
                c.DisplayRight = left + wPx; c.DisplayBottom = top + hPx;

                bool selected = mouseController.SelectedCrystals.Contains(c.Index);
                bool hovered = (GetHoveredCrystalIndex() == c.Index);

                Color fill = Color.Empty, border = Color.Blue;
                float bw = 1f;
                if (selected) { fill = Color.Yellow; border = Color.DarkBlue; bw = 2f; }
                else if (hovered) { fill = Color.LightBlue; border = Color.DarkBlue; }

                if (fill != Color.Empty) { using var b = new SolidBrush(fill); g.FillRectangle(b, left, top, wPx, hPx); }
                using (var p = new Pen(border, bw)) g.DrawRectangle(p, left, top, wPx, hPx);

                drawnInView++;
            }

            // Подписи индексов — только при большом зуме и если в кадре не слишком много кристаллов
            if (zoomPanController.ZoomFactor > 4.0f && drawnInView <= 200)
            {
                using var font = new Font("Arial", 8);
                using var tb = new SolidBrush(Color.Black);

                foreach (var c in CrystalManager.Instance.Crystals)
                {
                    if (c.DisplayRight <= 0 || c.DisplayLeft >= pictureBox1.Width ||
                        c.DisplayBottom <= 0 || c.DisplayTop >= pictureBox1.Height)
                        continue;

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
            var p = TryGetPointerOrZero();

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
            var m = pictureBox1.PointToClient(Cursor.Position);

            foreach (var c in CrystalManager.Instance.Crystals)
            {
                if (m.X >= c.DisplayLeft && m.X <= c.DisplayRight &&
                    m.Y >= c.DisplayTop && m.Y <= c.DisplayBottom)
                    return c.Index;
            }
            return -1;
        }
    }
}
