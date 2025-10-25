using System;
using System.Drawing;
using System.Windows.Forms;
using CrystalTable.Models;

namespace CrystalTable
{
    internal sealed class WaferRunDialog : Form
    {
        private readonly TextBox operatorTextBox;
        private readonly TextBox noteTextBox;
        private readonly Label waferNumberLabel;

        public WaferRunDialog(MapMetadata mapMetadata, string waferNumber)
        {
            Text = "Новая пластина";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);

            var layout = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var lotLabel = new Label
            {
                Text = "Партия:",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };
            var lotValueLabel = new Label
            {
                Text = string.IsNullOrWhiteSpace(mapMetadata?.LotNumber) ? "-" : mapMetadata.LotNumber,
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(0, 6, 0, 0)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(lotLabel, 0, 0);
            layout.Controls.Add(lotValueLabel, 1, 0);

            var waferLabel = new Label
            {
                Text = "Номер пластины:",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };
            waferNumberLabel = new Label
            {
                Text = string.IsNullOrWhiteSpace(waferNumber) ? "001" : waferNumber,
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(0, 6, 0, 0)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(waferLabel, 0, 1);
            layout.Controls.Add(waferNumberLabel, 1, 1);

            operatorTextBox = CreateTextBox();
            AddRow(layout, "Оператор", operatorTextBox, 2);

            noteTextBox = CreateTextBox(multiline: true);
            noteTextBox.Height = 60;
            AddRow(layout, "Примечание", noteTextBox, 3);

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };

            var okButton = new Button
            {
                Text = "Начать",
                DialogResult = DialogResult.OK,
                AutoSize = true
            };
            var cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                AutoSize = true
            };

            buttonsPanel.Controls.Add(okButton);
            buttonsPanel.Controls.Add(cancelButton);

            layout.Controls.Add(buttonsPanel, 0, 4);
            layout.SetColumnSpan(buttonsPanel, 2);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            Controls.Add(layout);

            LotNumber = mapMetadata?.LotNumber ?? string.Empty;
        }

        public WaferRunMetadata Metadata => new WaferRunMetadata(
            LotNumber,
            waferNumberLabel.Text,
            operatorTextBox.Text,
            noteTextBox.Text);

        public string LotNumber { get; set; } = string.Empty;

        private static TextBox CreateTextBox(bool multiline = false)
        {
            return new TextBox
            {
                Width = 220,
                Multiline = multiline
            };
        }

        private static void AddRow(TableLayoutPanel layout, string labelText, Control editor, int row)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };

            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(editor, 1, row);
        }
    }
}
