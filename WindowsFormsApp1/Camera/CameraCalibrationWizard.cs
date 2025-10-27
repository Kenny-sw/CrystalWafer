using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1.Camera
{
    /// <summary>
    /// Мастер калибровки камеры (4 шага)
    /// </summary>
    public partial class CameraCalibrationWizard : Form
    {
        private readonly CameraCalibrationController calibrationController;
        private int currentStep = 1;
        private float calibrationStepX = 5.0f;
        private float calibrationStepY = 0.0f;

        // UI элементы
        private Panel stepPanel;
        private Label titleLabel;
        private TextBox statusTextBox;
        // previewPictureBox используется для будущих улучшений (превью шаблона)
        // private PictureBox previewPictureBox;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnCancel;
        private RadioButton rbAxisX;
        private RadioButton rbAxisY;
        private NumericUpDown nudStep;

        public CameraCalibrationWizard(CameraCalibrationController controller)
        {
            calibrationController = controller ?? throw new ArgumentNullException(nameof(controller));
            
            InitializeComponent();
            InitializeEventHandlers();
            UpdateStepUI();
        }

        private void InitializeComponent()
        {
            this.Text = "🎯 Калибровка камеры";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            titleLabel = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(650, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Text = "Шаг 1 из 4: Подготовка"
            };
            this.Controls.Add(titleLabel);

            // Панель содержимого шага
            stepPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(650, 380),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(stepPanel);

            // Статус-бар
            statusTextBox = new TextBox
            {
                Location = new Point(20, 460),
                Size = new Size(650, 60),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(statusTextBox);

            // Кнопки навигации
            btnPrevious = new Button
            {
                Location = new Point(350, 530),
                Size = new Size(100, 30),
                Text = "◄ Назад",
                Enabled = false
            };
            this.Controls.Add(btnPrevious);

            btnNext = new Button
            {
                Location = new Point(460, 530),
                Size = new Size(100, 30),
                Text = "Далее ►"
            };
            this.Controls.Add(btnNext);

            btnCancel = new Button
            {
                Location = new Point(570, 530),
                Size = new Size(100, 30),
                Text = "✕ Отмена",
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);
        }

        private void InitializeEventHandlers()
        {
            btnNext.Click += BtnNext_Click;
            btnPrevious.Click += BtnPrevious_Click;
            
            calibrationController.StatusMessage += (s, msg) =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendStatus(msg)));
                }
                else
                {
                    AppendStatus(msg);
                }
            };
        }

        private void UpdateStepUI()
        {
            stepPanel.Controls.Clear();

            switch (currentStep)
            {
                case 1:
                    ShowStep1_Preparation();
                    break;
                case 2:
                    ShowStep2_CaptureReference();
                    break;
                case 3:
                    ShowStep3_Movement();
                    break;
                case 4:
                    ShowStep4_Results();
                    break;
            }

            btnPrevious.Enabled = currentStep > 1 && currentStep < 4;
            btnNext.Enabled = true;
        }

        private void ShowStep1_Preparation()
        {
            titleLabel.Text = "Шаг 1 из 4: Подготовка";

            var instructions = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(600, 120),
                Text = "Для точной калибровки камеры выполните следующие действия:\n\n" +
                       "✓ Убедитесь, что камера включена и работает\n" +
                       "✓ Наведите ЛШД на область с четкими деталями (кристаллы, метки)\n" +
                       "✓ Проверьте, что изображение резкое и хорошо освещено\n" +
                       "✓ Убедитесь, что путь перемещения свободен (5-10 мм)",
                Font = new Font("Segoe UI", 10)
            };
            stepPanel.Controls.Add(instructions);

            var noteLabel = new Label
            {
                Location = new Point(20, 160),
                Size = new Size(600, 60),
                Text = "💡 Совет: Точность калибровки зависит от качества изображения.\n" +
                       "   Используйте контрастные объекты и равномерное освещение.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Blue
            };
            stepPanel.Controls.Add(noteLabel);

            btnNext.Text = "Далее ►";
        }

        private void ShowStep2_CaptureReference()
        {
            titleLabel.Text = "Шаг 2 из 4: Захват эталонного кадра";

            var instructions = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(600, 60),
                Text = "Текущее положение ЛШД будет сохранено как эталонное.\n" +
                       "Система запомнит изображение для последующего сравнения.",
                Font = new Font("Segoe UI", 10)
            };
            stepPanel.Controls.Add(instructions);

            // Настройки смещения
            var settingsGroup = new GroupBox
            {
                Location = new Point(20, 100),
                Size = new Size(600, 120),
                Text = "Настройки калибровочного смещения"
            };
            stepPanel.Controls.Add(settingsGroup);

            var axisLabel = new Label
            {
                Location = new Point(20, 30),
                Size = new Size(80, 20),
                Text = "Ось движения:"
            };
            settingsGroup.Controls.Add(axisLabel);

            rbAxisX = new RadioButton
            {
                Location = new Point(120, 30),
                Size = new Size(60, 20),
                Text = "X",
                Checked = true
            };
            rbAxisX.CheckedChanged += (s, e) => UpdateCalibrationAxis();
            settingsGroup.Controls.Add(rbAxisX);

            rbAxisY = new RadioButton
            {
                Location = new Point(200, 30),
                Size = new Size(60, 20),
                Text = "Y"
            };
            rbAxisY.CheckedChanged += (s, e) => UpdateCalibrationAxis();
            settingsGroup.Controls.Add(rbAxisY);

            var stepLabel = new Label
            {
                Location = new Point(20, 65),
                Size = new Size(80, 20),
                Text = "Шаг (мм):"
            };
            settingsGroup.Controls.Add(stepLabel);

            nudStep = new NumericUpDown
            {
                Location = new Point(120, 63),
                Size = new Size(100, 20),
                Minimum = 1,
                Maximum = 50,
                DecimalPlaces = 1,
                Increment = 0.5m,
                Value = 5.0m
            };
            settingsGroup.Controls.Add(nudStep);

            var recommendLabel = new Label
            {
                Location = new Point(240, 65),
                Size = new Size(300, 20),
                Text = "(рекомендуется 5-10 мм для лучшей точности)",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            settingsGroup.Controls.Add(recommendLabel);

            var warningLabel = new Label
            {
                Location = new Point(20, 240),
                Size = new Size(600, 40),
                Text = "⚠️ После захвата машина переместится на указанный шаг.\n" +
                       "   Убедитесь, что путь свободен!",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkOrange
            };
            stepPanel.Controls.Add(warningLabel);

            btnNext.Text = "Захватить и переместить ►";
        }

        private void ShowStep3_Movement()
        {
            titleLabel.Text = "Шаг 3 из 4: Анализ изображения";

            var instructions = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(600, 40),
                Text = "Смещение выполнено. Система анализирует изображение\n" +
                       "для определения реального смещения...",
                Font = new Font("Segoe UI", 10)
            };
            stepPanel.Controls.Add(instructions);

            var progressLabel = new Label
            {
                Location = new Point(20, 80),
                Size = new Size(600, 100),
                Text = "⏳ Выполняется сопоставление изображений...\n\n" +
                       "Это может занять несколько секунд.\n" +
                       "Пожалуйста, подождите.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter
            };
            stepPanel.Controls.Add(progressLabel);

            btnNext.Text = "Вычислить ►";
            btnNext.Enabled = false;

            // Автоматически выполняем измерение
            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(500); // Небольшая задержка для UI
                
                bool success = calibrationController.Step3_MeasureDisplacement();
                
                BeginInvoke(new Action(() =>
                {
                    if (success)
                    {
                        progressLabel.Text = "✓ Анализ завершен!\n\n" +
                                           "Смещение успешно измерено.";
                        progressLabel.ForeColor = Color.Green;
                        btnNext.Enabled = true;
                    }
                    else
                    {
                        progressLabel.Text = "❌ Ошибка анализа\n\n" +
                                           "Не удалось сопоставить изображения.\n" +
                                           "Попробуйте снова с другими настройками.";
                        progressLabel.ForeColor = Color.Red;
                        btnPrevious.Enabled = true;
                    }
                }));
            });
        }

        private void ShowStep4_Results()
        {
            titleLabel.Text = "Шаг 4 из 4: Результат калибровки";

            var result = calibrationController.GetCalibrationResult();

            if (result == null || !result.IsValid())
            {
                var errorLabel = new Label
                {
                    Location = new Point(20, 20),
                    Size = new Size(600, 300),
                    Text = "❌ ОШИБКА КАЛИБРОВКИ\n\n" +
                           "Полученные данные некорректны.\n" +
                           "Возможные причины:\n" +
                           "• Слишком малое или большое смещение\n" +
                           "• Плохое качество изображения\n" +
                           "• Недостаточная освещенность\n\n" +
                           "Пожалуйста, повторите калибровку.",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Red,
                    TextAlign = ContentAlignment.TopLeft
                };
                stepPanel.Controls.Add(errorLabel);

                btnNext.Text = "🔄 Повторить";
                return;
            }

            var successLabel = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(600, 30),
                Text = "✅ КАЛИБРОВКА ВЫПОЛНЕНА УСПЕШНО!",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Green
            };
            stepPanel.Controls.Add(successLabel);

            var resultsGroup = new GroupBox
            {
                Location = new Point(20, 60),
                Size = new Size(600, 200),
                Text = "📊 Результаты калибровки"
            };
            stepPanel.Controls.Add(resultsGroup);

            int yPos = 25;

            // Масштаб камеры (главный результат)
            var scaleLabel = new Label
            {
                Location = new Point(20, yPos),
                Size = new Size(560, 50),
                Text = $"МАСШТАБ КАМЕРЫ:\n" +
                       $"{result.MillimetersPerPixel:F5} мм/пиксель  ({result.MillimetersPerPixel * 1000:F2} мкм/пиксель)",
                Font = new Font("Consolas", 11, FontStyle.Bold),
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter
            };
            resultsGroup.Controls.Add(scaleLabel);
            yPos += 60;

            // Детали
            var detailsLabel = new Label
            {
                Location = new Point(20, yPos),
                Size = new Size(560, 110),
                Text = $"Командное смещение:  {result.CommandedStepX:F3} × {result.CommandedStepY:F3} мм\n" +
                       $"Измеренное смещение: {result.MeasuredStepPxX:F1} × {result.MeasuredStepPxY:F1} пикселей\n\n" +
                       $"Масштаб X: {result.ScaleX:F5} мм/px\n" +
                       $"Масштаб Y: {result.ScaleY:F5} мм/px\n" +
                       $"Расхождение: {result.ErrorEstimatePercent:F2}%\n" +
                       $"Точность: {(result.ErrorEstimatePercent < 5 ? "✓ ОТЛИЧНО" : result.ErrorEstimatePercent < 10 ? "⚠ ПРИЕМЛЕМО" : "❌ ТРЕБУЕТСЯ ПОВТОР")}",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.TopLeft
            };
            resultsGroup.Controls.Add(detailsLabel);

            var infoLabel = new Label
            {
                Location = new Point(20, 270),
                Size = new Size(600, 60),
                Text = "💡 Калибровка сохранена и будет использоваться для всех измерений.\n" +
                       "   Рекомендуется повторять калибровку при изменении:\n" +
                       "   • Расстояния от камеры до объекта\n" +
                       "   • Разрешения или настроек камеры",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Blue
            };
            stepPanel.Controls.Add(infoLabel);

            btnNext.Text = "✓ Сохранить и закрыть";
        }

        private void UpdateCalibrationAxis()
        {
            if (rbAxisX.Checked)
            {
                calibrationStepX = (float)nudStep.Value;
                calibrationStepY = 0f;
            }
            else if (rbAxisY.Checked)
            {
                calibrationStepX = 0f;
                calibrationStepY = (float)nudStep.Value;
            }
        }

        private async void BtnNext_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            btnPrevious.Enabled = false;

            try
            {
                switch (currentStep)
                {
                    case 1:
                        // Переход к шагу 2
                        currentStep = 2;
                        UpdateStepUI();
                        break;

                    case 2:
                        // Захват эталонного кадра и смещение
                        UpdateCalibrationAxis();
                        
                        if (!calibrationController.Step1_CaptureReferenceFrame())
                        {
                            MessageBox.Show("Не удалось захватить эталонный кадр!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnNext.Enabled = true;
                            btnPrevious.Enabled = true;
                            return;
                        }

                        if (!await calibrationController.Step2_ExecuteMovement(calibrationStepX, calibrationStepY))
                        {
                            MessageBox.Show("Не удалось выполнить смещение!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnNext.Enabled = true;
                            btnPrevious.Enabled = true;
                            return;
                        }

                        currentStep = 3;
                        UpdateStepUI();
                        break;

                    case 3:
                        // Вычисление калибровки
                        if (!calibrationController.Step4_CalculateCalibration())
                        {
                            MessageBox.Show("Не удалось вычислить калибровку!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnNext.Enabled = true;
                            btnPrevious.Enabled = true;
                            return;
                        }

                        currentStep = 4;
                        UpdateStepUI();
                        break;

                    case 4:
                        // Завершение
                        var result = calibrationController.GetCalibrationResult();
                        if (result != null && result.IsValid())
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            // Повторить
                            calibrationController.Reset();
                            currentStep = 1;
                            UpdateStepUI();
                        }
                        break;
                }
            }
            finally
            {
                btnNext.Enabled = true;
                btnPrevious.Enabled = currentStep > 1 && currentStep < 4;
            }
        }

        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (currentStep > 1)
            {
                currentStep--;
                UpdateStepUI();
            }
        }

        private void AppendStatus(string message)
        {
            statusTextBox.AppendText(message + Environment.NewLine);
            statusTextBox.SelectionStart = statusTextBox.Text.Length;
            statusTextBox.ScrollToCaret();
        }
    }
}
