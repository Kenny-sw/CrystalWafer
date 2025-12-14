using System;
using System.Drawing;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Forms
{
    /// <summary>
    /// Форма настроек Bin Map
    /// </summary>
    public class BinMapSettingsForm : Form
    {
        private readonly BinMapRenderer renderer;
        private readonly Action onSettingsChanged;

        private CheckBox chkEnabled;
        private CheckBox chkShowLegend;
        private CheckBox chkShowSymbol;
        private ComboBox cmbLegendPosition;
        private TrackBar trackOpacity;
        private Label lblOpacity;
        private Button btnResetBins;
        private Button btnOk;
        private Button btnCancel;

        public BinMapSettingsForm(BinMapRenderer renderer, Action onSettingsChanged)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            this.onSettingsChanged = onSettingsChanged;

            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            Text = "Настройки Bin Map";
            Size = new Size(400, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9f);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                RowCount = 7,
                ColumnCount = 2
            };

            // === Чекбоксы ===
            chkEnabled = new CheckBox
            {
                Text = "Включить Bin Map",
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            };
            chkEnabled.CheckedChanged += (s, e) => ApplySettings();
            mainPanel.Controls.Add(chkEnabled, 0, 0);
            mainPanel.SetColumnSpan(chkEnabled, 2);

            chkShowLegend = new CheckBox
            {
                Text = "Показывать легенду",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            chkShowLegend.CheckedChanged += (s, e) => ApplySettings();
            mainPanel.Controls.Add(chkShowLegend, 0, 1);
            mainPanel.SetColumnSpan(chkShowLegend, 2);

            chkShowSymbol = new CheckBox
            {
                Text = "Показывать символ на кристалле (✓✗!)",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            chkShowSymbol.CheckedChanged += (s, e) => ApplySettings();
            mainPanel.Controls.Add(chkShowSymbol, 0, 2);
            mainPanel.SetColumnSpan(chkShowSymbol, 2);

            // === Позиция легенды ===
            var lblPosition = new Label
            {
                Text = "Позиция легенды:",
                AutoSize = true,
                Margin = new Padding(0, 8, 10, 0)
            };
            mainPanel.Controls.Add(lblPosition, 0, 3);

            cmbLegendPosition = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180,
                Margin = new Padding(0, 5, 0, 10)
            };
            cmbLegendPosition.Items.AddRange(new object[]
            {
                "Левый верхний",
                "Правый верхний",
                "Левый нижний",
                "Правый нижний"
            });
            cmbLegendPosition.SelectedIndexChanged += (s, e) => ApplySettings();
            mainPanel.Controls.Add(cmbLegendPosition, 1, 3);

            // === Прозрачность ===
            var lblOpacityTitle = new Label
            {
                Text = "Прозрачность:",
                AutoSize = true,
                Margin = new Padding(0, 8, 10, 0)
            };
            mainPanel.Controls.Add(lblOpacityTitle, 0, 4);

            var opacityPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            };

            trackOpacity = new TrackBar
            {
                Minimum = 50,
                Maximum = 255,
                Width = 150,
                TickFrequency = 25,
                SmallChange = 10,
                LargeChange = 25
            };
            trackOpacity.ValueChanged += (s, e) =>
            {
                lblOpacity.Text = $"{trackOpacity.Value}";
                ApplySettings();
            };
            opacityPanel.Controls.Add(trackOpacity);

            lblOpacity = new Label
            {
                AutoSize = true,
                Text = "160",
                Margin = new Padding(5, 5, 0, 0)
            };
            opacityPanel.Controls.Add(lblOpacity);

            mainPanel.Controls.Add(opacityPanel, 1, 4);

            // === Сброс категорий ===
            btnResetBins = new Button
            {
                Text = "Сбросить все категории",
                Width = 200,
                Height = 30,
                Margin = new Padding(0, 15, 0, 0),
                BackColor = Color.FromArgb(255, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            btnResetBins.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
            btnResetBins.Click += BtnResetBins_Click;
            mainPanel.Controls.Add(btnResetBins, 0, 5);
            mainPanel.SetColumnSpan(btnResetBins, 2);

            // === Горячие клавиши (информация) ===
            var infoGroup = new GroupBox
            {
                Text = "Горячие клавиши",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(10)
            };

            var infoLabel = new Label
            {
                Text = "F1 — Годен    F2 — Брак    F3 — Проверить\n" +
                       "F4 — Доработка    F5 — Краевой\n" +
                       "Delete — Сбросить    B — Вкл/Выкл Bin Map",
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            infoGroup.Controls.Add(infoLabel);
            mainPanel.Controls.Add(infoGroup, 0, 6);
            mainPanel.SetColumnSpan(infoGroup, 2);

            Controls.Add(mainPanel);

            // === Кнопки ===
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnCancel = new Button
            {
                Text = "Закрыть",
                Width = 90,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };
            buttonPanel.Controls.Add(btnCancel);

            Controls.Add(buttonPanel);

            AcceptButton = btnCancel;
            CancelButton = btnCancel;
        }

        private void LoadSettings()
        {
            var settings = renderer.Settings;

            chkEnabled.Checked = settings.Enabled;
            chkShowLegend.Checked = settings.ShowLegend;
            chkShowSymbol.Checked = settings.ShowBinSymbol;
            trackOpacity.Value = Math.Max(trackOpacity.Minimum, Math.Min(trackOpacity.Maximum, settings.Opacity));
            lblOpacity.Text = settings.Opacity.ToString();

            switch (settings.LegendPosition)
            {
                case LegendPosition.TopLeft:
                    cmbLegendPosition.SelectedIndex = 0;
                    break;
                case LegendPosition.TopRight:
                    cmbLegendPosition.SelectedIndex = 1;
                    break;
                case LegendPosition.BottomLeft:
                    cmbLegendPosition.SelectedIndex = 2;
                    break;
                case LegendPosition.BottomRight:
                default:
                    cmbLegendPosition.SelectedIndex = 3;
                    break;
            }
        }

        private void ApplySettings()
        {
            var settings = renderer.Settings;

            settings.Enabled = chkEnabled.Checked;
            settings.ShowLegend = chkShowLegend.Checked;
            settings.ShowBinSymbol = chkShowSymbol.Checked;
            settings.Opacity = trackOpacity.Value;

            switch (cmbLegendPosition.SelectedIndex)
            {
                case 0:
                    settings.LegendPosition = LegendPosition.TopLeft;
                    break;
                case 1:
                    settings.LegendPosition = LegendPosition.TopRight;
                    break;
                case 2:
                    settings.LegendPosition = LegendPosition.BottomLeft;
                    break;
                case 3:
                default:
                    settings.LegendPosition = LegendPosition.BottomRight;
                    break;
            }

            renderer.UpdateBrushCache();
            onSettingsChanged?.Invoke();
        }

        private void BtnResetBins_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Сбросить категории годности для ВСЕХ кристаллов?\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                foreach (var crystal in CrystalManager.Instance.Crystals)
                {
                    crystal.ResetInspection();
                }
                
                onSettingsChanged?.Invoke();
                MessageBox.Show("Все категории сброшены.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
