using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrystalTable.Models;

namespace CrystalTable
{
    internal sealed class MapMetadataDialog : Form
    {
        private readonly TemplateManager templateManager;
        private readonly ComboBox templateComboBox;
        private readonly TextBox lotTextBox;
        private readonly TextBox noteTextBox;
        private readonly string initialWaferNumber;

        public MapMetadataDialog(TemplateManager templateManager, MapMetadata initial = null, string defaultTemplateName = null)
        {
            this.templateManager = templateManager;
            initialWaferNumber = initial?.WaferNumber ?? string.Empty;

            Text = initial == null ? "Новая карта" : "Редактирование карты";
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

            templateComboBox = new ComboBox
            {
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            if (templateManager != null)
            {
                var names = templateManager.GetTemplateNames();
                if (names.Count > 0)
                {
                    templateComboBox.Items.AddRange(names.Cast<object>().ToArray());
                }

                templateComboBox.SelectedIndexChanged += TemplateSelected;

                if (!string.IsNullOrWhiteSpace(defaultTemplateName))
                {
                    var index = templateComboBox.Items.IndexOf(defaultTemplateName);
                    if (index >= 0)
                    {
                        templateComboBox.SelectedIndex = index;
                    }
                }
            }
            else
            {
                templateComboBox.Enabled = false;
            }

            var initialLot = initial?.LotNumber;
            if (string.IsNullOrWhiteSpace(initialLot) && !string.IsNullOrWhiteSpace(defaultTemplateName))
            {
                initialLot = defaultTemplateName;
            }

            lotTextBox = CreateTextBox(initialLot);
            noteTextBox = CreateTextBox(initial?.Note);
            noteTextBox.Multiline = true;
            noteTextBox.Height = 60;

            AddRow(layout, "Шаблон", templateComboBox, 0);
            AddRow(layout, "Название партии", lotTextBox, 1);
            AddRow(layout, "Примечание", noteTextBox, 2);

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };

            var okButton = new Button
            {
                Text = "Продолжить",
                DialogResult = DialogResult.OK,
                AutoSize = true
            };

            var cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                AutoSize = true
            };

            okButton.Click += ValidateAndClose;

            buttonsPanel.Controls.Add(okButton);
            buttonsPanel.Controls.Add(cancelButton);

            layout.Controls.Add(buttonsPanel, 0, 3);
            layout.SetColumnSpan(buttonsPanel, 2);

            Controls.Add(layout);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            if (templateComboBox.SelectedIndex >= 0)
            {
                TemplateSelected(this, EventArgs.Empty);
            }
        }

        public MapMetadata Metadata => new MapMetadata(lotTextBox.Text, initialWaferNumber, noteTextBox.Text);

        public MapTemplate SelectedTemplate
        {
            get
            {
                if (templateManager == null || templateComboBox.SelectedItem == null)
                {
                    return null;
                }

                return templateManager.GetTemplate(templateComboBox.SelectedItem.ToString());
            }
        }

        private void TemplateSelected(object sender, EventArgs e)
        {
            if (templateManager == null || templateComboBox.SelectedItem == null)
            {
                return;
            }

            var template = templateManager.GetTemplate(templateComboBox.SelectedItem.ToString());
            if (template == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(lotTextBox.Text))
            {
                lotTextBox.Text = template.Metadata?.LotNumber ?? string.Empty;
            }

            noteTextBox.Text = template.Metadata?.Note ?? string.Empty;
        }

        private static TextBox CreateTextBox(string initial)
        {
            return new TextBox
            {
                Width = 220,
                Text = initial ?? string.Empty
            };
        }

        private static void AddRow(TableLayoutPanel layout, string labelText, Control editor, int row)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText,
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };

            editor.Margin = new Padding(0, 3, 0, 3);

            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(editor, 1, row);
        }

        private void ValidateAndClose(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lotTextBox.Text))
            {
                MessageBox.Show(this,
                    "Укажите название партии.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }
    }
}
