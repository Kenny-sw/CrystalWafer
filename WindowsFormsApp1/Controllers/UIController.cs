using CrystalTable.Logic;
using CrystalTable.Controllers;
using CrystalTable.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CrystalTable;

namespace CrystalTable.Controllers
{
    /// <summary>Контроллер для управления элементами интерфейса</summary>
    public class UIController
    {
        private readonly Form1 form;

        public UIController(Form1 form) => this.form = form;

        // === ВАЛИДАЦИЯ ===

        // Принимаем TextBoxBase, чтобы работали и TextBox, и MaskedTextBox
        public void ValidateInput(TextBoxBase tb, ref uint targetUm)
        {
            if (tb == null) return;

            if (uint.TryParse(tb.Text.Trim(), out var val) && val > 0)
            {
                targetUm = val;
            }
            else
            {
                // откат к последнему валидному
                tb.Text = targetUm.ToString();
                tb.SelectionStart = tb.Text.Length;
            }
        }

        public void ValidateWaferDiameter(TextBoxBase tb, ref float targetMm)
        {
            if (tb == null) return;

            if (float.TryParse(tb.Text.Trim(), NumberStyles.Float, CultureSettings.NumericCulture, out var val))
            {
                // ограничим допустимым диапазоном
                if (val < WaferController.MinWaferDiameter) val = WaferController.MinWaferDiameter;
                if (val > WaferController.MaxWaferDiameter) val = WaferController.MaxWaferDiameter;

                targetMm = val;
                tb.Text = val.ToString(CultureSettings.NumericCulture);
                tb.SelectionStart = tb.Text.Length;
            }
            else
            {
                tb.Text = targetMm.ToString(CultureSettings.NumericCulture); // откат
                tb.SelectionStart = tb.Text.Length;
            }
        }

        public void ClearInputFields(params TextBoxBase[] textBoxes)
        {
            if (textBoxes == null) return;
            foreach (var textBox in textBoxes)
            {
                if (textBox == null) continue;
                textBox.Text = "";
                textBox.BackColor = SystemColors.Window;
            }
        }

        // === СТАТУС/МЕТКИ ===

        /// <summary>Обновление статус-бара, счётчиков и координат указателя</summary>
        public void UpdateStatusBar(WaferController wafer, ZoomPanController zoom)
        {
            if (form == null) return;

            if (form.StatusLabel != null)
            {
                form.StatusLabel.Text = form.DebugModeWithoutComPort ? "🔧 Debug" : "✓ Готово";
                form.StatusLabel.ForeColor = form.DebugModeWithoutComPort ? Color.OrangeRed : Color.ForestGreen;
            }

            if (form.ZoomLabel != null && zoom != null)
            {
                form.ZoomLabel.Text = $"🔍 ×{zoom.ZoomFactor:F1}";
            }

            int count = CrystalManager.Instance.Crystals.Count;
            if (form.TotalCrystalsStatusLabel != null)
            {
                form.TotalCrystalsStatusLabel.Text = $"💎 {count}";
            }

            if (form.FillPercentageLabel != null && wafer != null)
            {
                float cw = wafer.CrystalWidthRaw / 1000f;
                float ch = wafer.CrystalHeightRaw / 1000f;
                float waferArea = (float)(Math.PI * Math.Pow(wafer.WaferDiameter / 2f, 2));
                float fill = waferArea > 0 ? Math.Min(100f, Math.Max(0f, (count * cw * ch) / waferArea * 100f)) : 0f;
                form.FillPercentageLabel.Text = $"📊 {fill:F0}%";
                
                // Цветовая индикация заполнения
                if (fill > 75)
                    form.FillPercentageLabel.ForeColor = Color.ForestGreen;
                else if (fill > 40)
                    form.FillPercentageLabel.ForeColor = Color.DarkOrange;
                else
                    form.FillPercentageLabel.ForeColor = Color.Crimson;
            }

            var pointer = form.GetPointerMm();
            if (form.CoordinatesLabel != null)
            {
                form.CoordinatesLabel.Text = $"📍 ({pointer.X:F2}, {pointer.Y:F2})";
            }

            if (form.CalibrationStatusLabel != null && wafer != null)
            {
                if (wafer.IsCalibrated)
                {
                    form.CalibrationStatusLabel.Text = $"⚙️ #{wafer.CalibrationCrystalIndex}";
                    form.CalibrationStatusLabel.ForeColor = Color.DarkGreen;
                }
                else
                {
                    form.CalibrationStatusLabel.Text = "⚙️ —";
                    form.CalibrationStatusLabel.ForeColor = Color.Gray;
                }
            }
        }

        public void UpdateSelectionLabel(HashSet<int> selected)
        {
            if (form?.SelectedCrystalStatusLabel == null) return;

            if (selected == null || selected.Count == 0)
            {
                form.SelectedCrystalStatusLabel.Text = "✓ 0";
                form.SelectedCrystalStatusLabel.ForeColor = Color.Gray;
            }
            else if (selected.Count == 1)
            {
                int idx = selected.First();
                form.SelectedCrystalStatusLabel.Text = $"✓ #{idx + 1}";
                form.SelectedCrystalStatusLabel.ForeColor = Color.RoyalBlue;
            }
            else
            {
                form.SelectedCrystalStatusLabel.Text = $"✓ {selected.Count}";
                form.SelectedCrystalStatusLabel.ForeColor = Color.RoyalBlue;
            }
        }

        public void ShowHoveredCrystal(Crystal crystal)
        {
            if (form?.SelectedCrystalStatusLabel == null) return;

            if (crystal != null)
            {
                form.SelectedCrystalStatusLabel.Text = string.Format(
                    CultureSettings.NumericCulture,
                    "Кристалл: {0} (X: {1:F2} мм, Y: {2:F2} мм, Z: {3:F2})",
                    crystal.Index + 1,
                    crystal.RealX,
                    crystal.RealY,
                    crystal.Z);
                return;
            }

            var selected = form.MouseController?.SelectedCrystals;
            UpdateSelectionLabel(selected ?? new HashSet<int>());
        }

        // === ТУЛБАР ===

        public void UpdateToolbarState(CommandHistory history)
        {
            if (history == null || form == null) return;

            if (form.BtnUndo != null)
            {
                bool canUndo = history.CanUndo();
                form.BtnUndo.Enabled = canUndo;
                form.BtnUndo.ToolTipText = canUndo
                    ? $"Отменить: {history.GetUndoDescription()}"
                    : "Отменить (Ctrl+Z)";
            }

            if (form.BtnRedo != null)
            {
                bool canRedo = history.CanRedo();
                form.BtnRedo.Enabled = canRedo;
                form.BtnRedo.ToolTipText = canRedo
                    ? $"Повторить: {history.GetRedoDescription()}"
                    : "Повторить (Ctrl+Y)";
            }
        }

        // === Оверлей масштаба на картинке ===

        public void DrawZoomInfo(Graphics g, float zoomFactor)
        {
            if (g == null || form?.PictureBox == null) return;

            string zoomText = $"×{zoomFactor:F1}";
            
            using (Font font = new Font("Segoe UI Semibold", 10f))
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(180, 52, 73, 94)))
            {
                SizeF sz = g.MeasureString(zoomText, font);
                float padding = 8;
                float x = 12, y = 12;
                float width = sz.Width + padding * 2;
                float height = sz.Height + padding;
                float radius = 6;
                
                // Рисуем скругленный прямоугольник
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();
                    
                    g.FillPath(bgBrush, path);
                }
                
                g.DrawString(zoomText, font, textBrush, x + padding, y + padding / 2);
            }
        }
    }
}
