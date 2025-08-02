using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для управления элементами интерфейса
    /// </summary>
    public class UIController
    {
        private readonly Form1 form;

        public UIController(Form1 form)
        {
            this.form = form;
        }

        /// <summary>
        /// Валидация поля ввода
        /// </summary>
        public void ValidateInput(MaskedTextBox textBox, ref float tempValue)
        {
            if (!float.TryParse(textBox.Text, out float value) || value <= 0)
            {
                textBox.BackColor = Color.Red;
                tempValue = 0;
            }
            else
            {
                textBox.BackColor = Color.GreenYellow;
                if (value != tempValue)
                {
                    tempValue = value;
                }
            }
        }

        /// <summary>
        /// Валидация диаметра пластины
        /// </summary>
        public void ValidateWaferDiameter(MaskedTextBox textBox, ref float tempValue)
        {
            if (!float.TryParse(textBox.Text, out float value) ||
                value < WaferController.MinWaferDiameter ||
                value > WaferController.MaxWaferDiameter)
            {
                textBox.BackColor = Color.Red;
                tempValue = 0;
            }
            else
            {
                textBox.BackColor = Color.GreenYellow;
                if (value != tempValue)
                {
                    tempValue = value;
                }
            }
        }

        /// <summary>
        /// Обновление статусной строки
        /// </summary>
        public void UpdateStatusBar(WaferController waferController, ZoomPanController zoomPanController)
        {
            if (form.StatusLabel != null)
            {
                form.StatusLabel.Text = "Готово";
            }

            if (form.ZoomLabel != null)
            {
                form.ZoomLabel.Text = $"Масштаб: {zoomPanController.ZoomFactor:F1}x";
            }

            if (form.FillPercentageLabel != null && CrystalManager.Instance.Crystals.Count > 0)
            {
                var stats = waferController.GetStatistics();
                if (stats != null)
                {
                    float fillPercentage = stats.CalculateFillPercentage(
                        waferController.CrystalWidthRaw / 1000f,
                        waferController.CrystalHeightRaw / 1000f);
                    form.FillPercentageLabel.Text = $"Заполнение: {fillPercentage:F1}%";
                }
            }
        }

        /// <summary>
        /// Обновление метки выделения
        /// </summary>
        public void UpdateSelectionLabel(HashSet<int> selectedCrystals)
        {
            if (selectedCrystals.Count == 0)
            {
                form.LabelSelectedCrystal.Text = "Кристаллы не выбраны";
            }
            else if (selectedCrystals.Count == 1)
            {
                form.LabelSelectedCrystal.Text = $"Выбран кристалл: {selectedCrystals.GetEnumerator().Current}";
            }
            else
            {
                form.LabelSelectedCrystal.Text = $"Выбрано кристаллов: {selectedCrystals.Count}";
            }
        }

        /// <summary>
        /// Обновление состояния панели инструментов
        /// </summary>
        /// <summary>
        /// Обновление состояния панели инструментов
        /// </summary>
        public void UpdateToolbarState(CommandHistory commandHistory)
        {
            // Получаем кнопки из коллекции Items toolStrip1 по имени ключа
            var btnUndo = form.toolStrip1.Items["btnUndo"] as ToolStripButton;
            var btnRedo = form.toolStrip1.Items["btnRedo"] as ToolStripButton;

            if (btnUndo != null)
            {
                btnUndo.Enabled = commandHistory.CanUndo();
                if (commandHistory.CanUndo())
                {
                    string undoText = commandHistory.GetUndoDescription();
                    btnUndo.ToolTipText = $"Отменить: {undoText}";
                }
                else
                {
                    btnUndo.ToolTipText = "Отменить (Ctrl+Z)";
                }
            }

            if (btnRedo != null)
            {
                btnRedo.Enabled = commandHistory.CanRedo();
                if (commandHistory.CanRedo())
                {
                    string redoText = commandHistory.GetRedoDescription();
                    btnRedo.ToolTipText = $"Повторить: {redoText}";
                }
                else
                {
                    btnRedo.ToolTipText = "Повторить (Ctrl+Y)";
                }
            }
        }


        /// <summary>
        /// Очистка полей ввода
        /// </summary>
        public void ClearInputFields(params MaskedTextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                textBox.Text = "";
                textBox.BackColor = SystemColors.Window;
            }
        }

        /// <summary>
        /// Отображение информации о масштабе на PictureBox
        /// </summary>
        public void DrawZoomInfo(Graphics g, float zoomFactor)
        {
            string zoomText = $"Масштаб: {zoomFactor:F1}x";
            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                SizeF textSize = g.MeasureString(zoomText, font);
                float x = form.PictureBox.Width - textSize.Width - 10;
                float y = form.PictureBox.Height - textSize.Height - 10;

                // Фон для текста
                using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                {
                    g.FillRectangle(bgBrush, x - 5, y - 5, textSize.Width + 10, textSize.Height + 10);
                }

                g.DrawString(zoomText, font, brush, x, y);
            }
        }
    }
}