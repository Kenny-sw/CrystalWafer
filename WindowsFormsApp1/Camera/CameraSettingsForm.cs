using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Camera;

namespace CrystalTable.Camera
{
    /// <summary>
    /// Форма настроек камеры
    /// </summary>
    public class CameraSettingsForm : Form
    {
        private readonly WindowsFormsApp1.Camera.CameraController cameraController;
        private CameraSettings workingSettings;

        // Контролы базовых параметров
        private TrackBar brightnessTrackBar;
        private TrackBar contrastTrackBar;
        private TrackBar saturationTrackBar;
        private TrackBar sharpnessTrackBar;
        private TrackBar gainTrackBar;

        // Контролы экспозиции
        private CheckBox autoExposureCheckBox;
        private NumericUpDown exposureNumericUpDown;

        // Контролы баланса белого
        private CheckBox autoWhiteBalanceCheckBox;
        private NumericUpDown whiteBalanceNumericUpDown;

        // Контролы разрешения
        private ComboBox resolutionComboBox;
        private ComboBox fpsComboBox;

        // Кнопки
        private Button applyButton;
        private Button resetButton;
        private Button closeButton;

        // Labels для отображения значений
        private Label brightnessLabel;
        private Label contrastLabel;
        private Label saturationLabel;
        private Label sharpnessLabel;
        private Label gainLabel;

        public CameraSettingsForm(WindowsFormsApp1.Camera.CameraController controller)
        {
            cameraController = controller;
            workingSettings = controller.Settings.Clone();
            
            InitializeComponent();
            LoadSettings();
            UpdateCameraStatus();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки камеры";
            this.Size = new Size(500, 650); // Увеличил высоту для статуса
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // === СТАТУС КАМЕРЫ (НОВОЕ) ===
            var statusPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 248, 255)
            };

            var statusTitleLabel = new Label
            {
                Text = "Статус камеры:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            var statusValueLabel = new Label
            {
                Name = "statusValueLabel",
                Text = cameraController.IsRunning ? "🟢 Камера работает" : "🔴 Камера не запущена",
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(10, 35),
                ForeColor = cameraController.IsRunning ? Color.Green : Color.Red
            };

            statusPanel.Controls.Add(statusTitleLabel);
            statusPanel.Controls.Add(statusValueLabel);
            this.Controls.Add(statusPanel);

            // === ОСНОВНАЯ ПАНЕЛЬ (СУЩНОСТЬ) ===
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true
            };

            // === БАЗОВЫЕ ПАРАМЕТРЫ ===
            var groupBasic = CreateGroupBox("Базовые параметры изображения");
            mainPanel.Controls.Add(groupBasic);
            
            var basicPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(5)
            };
            
            // Яркость
            AddSliderControl(basicPanel, "Яркость:", 0, 100, 
                out brightnessTrackBar, out brightnessLabel, 0);
            
            // Контраст
            AddSliderControl(basicPanel, "Контраст:", 0, 100, 
                out contrastTrackBar, out contrastLabel, 1);
            
            // Насыщенность
            AddSliderControl(basicPanel, "Насыщенность:", 0, 100, 
                out saturationTrackBar, out saturationLabel, 2);
            
            // Резкость
            AddSliderControl(basicPanel, "Резкость:", 0, 10, 
                out sharpnessTrackBar, out sharpnessLabel, 3);
            
            // Усиление
            AddSliderControl(basicPanel, "Усиление (Gain):", 0, 100, 
                out gainTrackBar, out gainLabel, 4);

            groupBasic.Controls.Add(basicPanel);

            // === ЭКСПОЗИЦИЯ ===
            var groupExposure = CreateGroupBox("Экспозиция");
            mainPanel.Controls.Add(groupExposure);
            
            var exposurePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(5)
            };

            autoExposureCheckBox = new CheckBox
            {
                Text = "Автоматическая экспозиция",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            autoExposureCheckBox.CheckedChanged += AutoExposureCheckBox_CheckedChanged;

            var exposureValuePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            
            exposureValuePanel.Controls.Add(new Label
            {
                Text = "Значение (мс):",
                AutoSize = true,
                Margin = new Padding(0, 5, 5, 0)
            });

            exposureNumericUpDown = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1000,
                Width = 80,
                Margin = new Padding(0, 3, 0, 0)
            };

            exposureValuePanel.Controls.Add(exposureNumericUpDown);
            
            exposurePanel.Controls.Add(autoExposureCheckBox);
            exposurePanel.Controls.Add(exposureValuePanel);
            groupExposure.Controls.Add(exposurePanel);

            // === БАЛАНС БЕЛОГО ===
            var groupWhiteBalance = CreateGroupBox("Баланс белого");
            mainPanel.Controls.Add(groupWhiteBalance);
            
            var whiteBalancePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(5)
            };

            autoWhiteBalanceCheckBox = new CheckBox
            {
                Text = "Автоматический баланс белого",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            autoWhiteBalanceCheckBox.CheckedChanged += AutoWhiteBalanceCheckBox_CheckedChanged;

            var whiteBalanceValuePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            
            whiteBalanceValuePanel.Controls.Add(new Label
            {
                Text = "Температура (K):",
                AutoSize = true,
                Margin = new Padding(0, 5, 5, 0)
            });

            whiteBalanceNumericUpDown = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 10000,
                Increment = 100,
                Width = 80,
                Margin = new Padding(0, 3, 0, 0)
            };

            whiteBalanceValuePanel.Controls.Add(whiteBalanceNumericUpDown);
            
            whiteBalancePanel.Controls.Add(autoWhiteBalanceCheckBox);
            whiteBalancePanel.Controls.Add(whiteBalanceValuePanel);
            groupWhiteBalance.Controls.Add(whiteBalancePanel);

            // === РАЗРЕШЕНИЕ И FPS ===
            var groupResolution = CreateGroupBox("Разрешение и частота кадров");
            mainPanel.Controls.Add(groupResolution);
            
            var resolutionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(5)
            };

            var resPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            
            resPanel.Controls.Add(new Label
            {
                Text = "Разрешение:",
                AutoSize = true,
                Margin = new Padding(0, 5, 5, 0),
                Width = 120
            });

            resolutionComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150
            };
            resolutionComboBox.Items.AddRange(new object[]
            {
                "640x480",
                "800x600",
                "1280x720",
                "1920x1080"
            });

            resPanel.Controls.Add(resolutionComboBox);
            
            var fpsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            
            fpsPanel.Controls.Add(new Label
            {
                Text = "Частота кадров:",
                AutoSize = true,
                Margin = new Padding(0, 5, 5, 0),
                Width = 120
            });

            fpsComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150
            };
            fpsComboBox.Items.AddRange(new object[] { 15, 30, 60 });

            fpsPanel.Controls.Add(fpsComboBox);
            
            resolutionPanel.Controls.Add(resPanel);
            resolutionPanel.Controls.Add(fpsPanel);
            groupResolution.Controls.Add(resolutionPanel);

            // === КНОПКИ ===
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10)
            };

            closeButton = new Button
            {
                Text = "Закрыть",
                Width = 100,
                Height = 30,
                Margin = new Padding(5)
            };
            closeButton.Click += CloseButton_Click;

            resetButton = new Button
            {
                Text = "Сброс",
                Width = 100,
                Height = 30,
                Margin = new Padding(5)
            };
            resetButton.Click += ResetButton_Click;

            applyButton = new Button
            {
                Text = "Применить",
                Width = 100,
                Height = 30,
                Margin = new Padding(5)
            };
            applyButton.Click += ApplyButton_Click;

            // НОВАЯ КНОПКА: Проверить поддержку
            var checkSupportButton = new Button
            {
                Text = "Проверить поддержку",
                Width = 150,
                Height = 30,
                Margin = new Padding(5)
            };
            checkSupportButton.Click += CheckSupportButton_Click;

            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(resetButton);
            buttonPanel.Controls.Add(applyButton);
            buttonPanel.Controls.Add(checkSupportButton);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);
        }

        private GroupBox CreateGroupBox(string title)
        {
            return new GroupBox
            {
                Text = title,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(5),
                Margin = new Padding(0, 0, 0, 10)
            };
        }

        private void AddSliderControl(TableLayoutPanel panel, string labelText, 
            int min, int max, out TrackBar trackBar, out Label valueLabel, int rowIndex)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            
            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Margin = new Padding(0, 8, 5, 5)
            };
            panel.Controls.Add(label, 0, rowIndex);

            trackBar = new TrackBar
            {
                Minimum = min,
                Maximum = max,
                TickFrequency = (max - min) / 10,
                Width = 250,
                Margin = new Padding(0, 3, 5, 3)
            };
            trackBar.ValueChanged += TrackBar_ValueChanged;
            panel.Controls.Add(trackBar, 1, rowIndex);

            valueLabel = new Label
            {
                AutoSize = true,
                Width = 40,
                Margin = new Padding(0, 8, 0, 5)
            };
            panel.Controls.Add(valueLabel, 2, rowIndex);
        }

        private void LoadSettings()
        {
            // Загружаем базовые параметры
            brightnessTrackBar.Value = workingSettings.Brightness;
            brightnessLabel.Text = workingSettings.Brightness.ToString();

            contrastTrackBar.Value = workingSettings.Contrast;
            contrastLabel.Text = workingSettings.Contrast.ToString();

            saturationTrackBar.Value = workingSettings.Saturation;
            saturationLabel.Text = workingSettings.Saturation.ToString();

            sharpnessTrackBar.Value = workingSettings.Sharpness;
            sharpnessLabel.Text = workingSettings.Sharpness.ToString();

            gainTrackBar.Value = workingSettings.Gain;
            gainLabel.Text = workingSettings.Gain.ToString();

            // Экспозиция
            autoExposureCheckBox.Checked = workingSettings.AutoExposure;
            exposureNumericUpDown.Value = workingSettings.Exposure;
            exposureNumericUpDown.Enabled = !workingSettings.AutoExposure;

            // Баланс белого
            autoWhiteBalanceCheckBox.Checked = workingSettings.AutoWhiteBalance;
            whiteBalanceNumericUpDown.Value = workingSettings.WhiteBalance;
            whiteBalanceNumericUpDown.Enabled = !workingSettings.AutoWhiteBalance;

            // Разрешение
            resolutionComboBox.SelectedItem = $"{workingSettings.Resolution.Width}x{workingSettings.Resolution.Height}";
            
            // FPS
            fpsComboBox.SelectedItem = workingSettings.FrameRate;
        }

        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            var trackBar = sender as TrackBar;
            
            if (trackBar == brightnessTrackBar)
            {
                brightnessLabel.Text = trackBar.Value.ToString();
                workingSettings.Brightness = trackBar.Value;
            }
            else if (trackBar == contrastTrackBar)
            {
                contrastLabel.Text = trackBar.Value.ToString();
                workingSettings.Contrast = trackBar.Value;
            }
            else if (trackBar == saturationTrackBar)
            {
                saturationLabel.Text = trackBar.Value.ToString();
                workingSettings.Saturation = trackBar.Value;
            }
            else if (trackBar == sharpnessTrackBar)
            {
                sharpnessLabel.Text = trackBar.Value.ToString();
                workingSettings.Sharpness = trackBar.Value;
            }
            else if (trackBar == gainTrackBar)
            {
                gainLabel.Text = trackBar.Value.ToString();
                workingSettings.Gain = trackBar.Value;
            }
        }

        private void AutoExposureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            workingSettings.AutoExposure = autoExposureCheckBox.Checked;
            exposureNumericUpDown.Enabled = !autoExposureCheckBox.Checked;
        }

        private void AutoWhiteBalanceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            workingSettings.AutoWhiteBalance = autoWhiteBalanceCheckBox.Checked;
            whiteBalanceNumericUpDown.Enabled = !autoWhiteBalanceCheckBox.Checked;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка: камера должна быть запущена
                if (!cameraController.IsRunning)
                {
                    MessageBox.Show("Камера не запущена!\n\nЗапустите камеру перед применением настроек.",
                        "Настройки камеры", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Сохраняем числовые значения
                workingSettings.Exposure = (int)exposureNumericUpDown.Value;
                workingSettings.WhiteBalance = (int)whiteBalanceNumericUpDown.Value;

                // Разрешение
                if (resolutionComboBox.SelectedItem != null)
                {
                    var parts = resolutionComboBox.SelectedItem.ToString().Split('x');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
                    {
                        workingSettings.Resolution = new Size(width, height);
                    }
                }

                // FPS
                if (fpsComboBox.SelectedItem != null && int.TryParse(fpsComboBox.SelectedItem.ToString(), out int fps))
                {
                    workingSettings.FrameRate = fps;
                }

                // Применяем настройки к камере
                cameraController.Settings = workingSettings;

                // Проверяем результат применения
                var caps = cameraController.GetCameraCapabilities();
                int supportedCount = 0;
                if (caps.SupportsBrightness) supportedCount++;
                if (caps.SupportsContrast) supportedCount++;
                if (caps.SupportsSaturation) supportedCount++;
                if (caps.SupportsSharpness) supportedCount++;
                if (caps.SupportsGain) supportedCount++;
                if (caps.SupportsWhiteBalance) supportedCount++;
                if (caps.SupportsExposure) supportedCount++;

                if (supportedCount == 0)
                {
                    MessageBox.Show("⚠️ Камера не поддерживает аппаратные настройки.\n\n" +
                        "Это может быть связано с:\n" +
                        "• Дешевой веб-камерой без DirectShow\n" +
                        "• Устаревшими драйверами\n" +
                        "• Виртуальной камерой (OBS, ManyCam)\n\n" +
                        "Попробуйте:\n" +
                        "1. Обновить драйверы камеры\n" +
                        "2. Использовать другую USB-камеру\n" +
                        "3. Нажать 'Проверить поддержку' для деталей",
                        "Настройки не применены", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"✓ Настройки применены!\n\n" +
                        $"Поддерживается: {supportedCount} из 7 настроек\n\n" +
                        $"Нажмите 'Проверить поддержку' для подробностей.",
                        "Настройки камеры", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка применения настроек:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Сбросить все настройки к значениям по умолчанию?",
                "Сброс настроек", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                workingSettings = new CameraSettings();
                LoadSettings();
                MessageBox.Show("Настройки сброшены. Нажмите 'Применить' для сохранения.",
                    "Сброс настроек", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Обновить отображение статуса камеры
        /// </summary>
        private void UpdateCameraStatus()
        {
            var statusLabel = this.Controls.Find("statusValueLabel", true).FirstOrDefault() as Label;
            if (statusLabel != null)
            {
                if (cameraController.IsRunning)
                {
                    statusLabel.Text = "🟢 Камера работает - настройки можно применять";
                    statusLabel.ForeColor = Color.Green;
                }
                else
                {
                    statusLabel.Text = "🔴 Камера не запущена - запустите камеру для настройки";
                    statusLabel.ForeColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// Проверка поддержки настроек камерой
        /// </summary>
        private void CheckSupportButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка: камера должна быть запущена
                if (!cameraController.IsRunning)
                {
                    MessageBox.Show("Камера не запущена!\n\nЗапустите камеру для проверки возможностей.",
                        "Проверка поддержки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var caps = cameraController.GetCameraCapabilities();

                var message = "🔍 Поддержка настроек вашей Kameroy:\n\n";
                message += "✓ = поддерживается    ✗ = не поддерживается\n\n";
                message += $"{(caps.SupportsBrightness ? "✓" : "✗")} Яркость (Brightness)\n";
                message += $"{(caps.SupportsContrast ? "✓" : "✗")} Контраст (Contrast)\n";
                message += $"{(caps.SupportsSaturation ? "✓" : "✗")} Насыщенность (Saturation)\n";
                message += $"{(caps.SupportsSharpness ? "✓" : "✗")} Резкость (Sharpness)\n";
                message += $"{(caps.SupportsGain ? "✓" : "✗")} Усиление (Gain)\n";
                message += $"{(caps.SupportsWhiteBalance ? "✓" : "✗")} Баланс белого (White Balance)\n";
                message += $"{(caps.SupportsExposure ? "✓" : "✗")} Экспозиция (Exposure)\n\n";

                int supportedCount = 0;
                if (caps.SupportsBrightness) supportedCount++;
                if (caps.SupportsContrast) supportedCount++;
                if (caps.SupportsSaturation) supportedCount++;
                if (caps.SupportsSharpness) supportedCount++;
                if (caps.SupportsGain) supportedCount++;
                if (caps.SupportsWhiteBalance) supportedCount++;
                if (caps.SupportsExposure) supportedCount++;

                message += $"📊 Поддерживается: {supportedCount} из 7 настроек\n\n";

                if (supportedCount == 0)
                {
                    message += "⚠️ ВНИМАНИЕ: Камера не поддерживает аппаратные настройки!\n\n";
                    message += "Возможные причины:\n";
                    message += "• Дешевая веб-камера без DirectShow API\n";
                    message += "• Драйвер не реализует IAMVideoProcAmp\n";
                    message += "• Виртуальная камера (OBS, ManyCam)\n";
                    message += "• Встроенная камера ноутбука с ограничениями\n";
                }
                else if (supportedCount < 7)
                {
                    message += "ℹ️ Примечание: Неподдерживаемые настройки будут игнорироваться.\n";
                    message += "Это нормально для большинства веб-камер.";
                }
                else
                {
                    message += "🎉 Отлично! Камера полностью поддерживает все настройки.";
                }

                MessageBox.Show(message, "Возможности камеры", 
                    MessageBoxButtons.OK, supportedCount > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки поддержки:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
