using CrystalTable.Logic;
using CrystalTable.Controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

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

            if (float.TryParse(tb.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            {
                // ограничим допустимым диапазоном
                if (val < WaferController.MinWaferDiameter) val = WaferController.MinWaferDiameter;
                if (val > WaferController.MaxWaferDiameter) val = WaferController.MaxWaferDiameter;

                targetMm = val;
                tb.Text = val.ToString(CultureInfo.InvariantCulture);
                tb.SelectionStart = tb.Text.Length;
            }
            else
            {
                tb.Text = targetMm.ToString(CultureInfo.InvariantCulture); // откат
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

            // Строка состояния
            if (form.StatusLabel != null)
                form.StatusLabel.Text = "Готово";

            // Масштаб
            if (form.ZoomLabel != null && zoom != null)
                form.ZoomLabel.Text = $"Масштаб: {zoom.ZoomFactor:F1}x";

            // Кол-во кристаллов
            int count = CrystalManager.Instance.Crystals.Count;
            if (form.LabelTotalCrystals != null)
                form.LabelTotalCrystals.Text = $"Общее количество кристаллов: {count}";

            // Заполнение площади (приблизительно)
            if (form.FillPercentageLabel != null && wafer != null)
            {
                float cw = wafer.CrystalWidthRaw / 1000f;
                float ch = wafer.CrystalHeightRaw / 1000f;
                float waferArea = (float)(Math.PI * Math.Pow(wafer.WaferDiameter / 2f, 2));
                float fill = waferArea > 0 ? Math.Min(100f, Math.Max(0f, (count * cw * ch) / waferArea * 100f)) : 0f;
                form.FillPercentageLabel.Text = $"Заполнение: {fill:F1}%";
            }

            // Координаты указателя
            var p = form.GetPointerMm();
            if (form.CoordinatesLabel != null)
                form.CoordinatesLabel.Text = $"X: {p.X:F3} мм, Y: {p.Y:F3} мм";
        }

        /// <summary>Обновление метки выбранных кристаллов (индексация с 1 для UI)</summary>
        public void UpdateSelectionLabel(HashSet<int> selected)
        {
            if (form?.LabelSelectedCrystal == null) return;

            if (selected == null || selected.Count == 0)
            {
                form.LabelSelectedCrystal.Text = "Кристаллы не выбраны";
            }
            else if (selected.Count == 1)
            {
                int idx = selected.First();
                form.LabelSelectedCrystal.Text = $"Выбран кристалл: {idx + 1}";
            }
            else
            {
                var head = selected.OrderBy(i => i).Take(5).Select(i => (i + 1).ToString());
                string headStr = string.Join(", ", head);
                string suffix = selected.Count > 5 ? "…" : "";
                form.LabelSelectedCrystal.Text = $"Выбрано: {selected.Count} ({headStr}{suffix})";
            }
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

            string zoomText = $"Zoom: {zoomFactor:F1}x";
            using (Font font = new Font("Segoe UI", 9f))
            using (Brush brush = new SolidBrush(Color.Black))
            using (Brush bg = new SolidBrush(Color.FromArgb(210, Color.White)))
            using (Pen pen = new Pen(Color.Gray))
            {
                SizeF sz = g.MeasureString(zoomText, font);
                float x = 10, y = 10;
                g.FillRectangle(bg, x, y, sz.Width + 10, sz.Height + 6);
                g.DrawRectangle(pen, x, y, sz.Width + 10, sz.Height + 6);
                g.DrawString(zoomText, font, brush, x + 5, y + 3);
            }
        }
    }
}
