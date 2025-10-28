using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Camera;
using WindowsFormsApp1.ComputerVision;
using CrystalTable.Camera;
using CrystalTable.Logic;

namespace CrystalTable
{
    public partial class Form1
    {
        private WindowsFormsApp1.Camera.CameraController cameraController;
        private CameraCalibrationController calibrationController;
        private ComputerVisionController cvController;
        
        // Кнопки в тулбаре
        private ToolStripButton btnCameraToggle;
        private ToolStripButton btnCameraSettings;
        private ToolStripButton btnCameraSnapshot;
        private ToolStripButton btnCameraFreeze;
        private ToolStripButton btnCameraCalibrate;

        /// <summary>
        /// Инициализация камеры и UI
        /// </summary>
        private void InitializeCamera()
        {
            cameraController = new WindowsFormsApp1.Camera.CameraController();
            cvController = new ComputerVisionController();
            
            // Подписка на события
            cameraController.FrameCaptured += CameraController_FrameCaptured;
            cameraController.StatusChanged += CameraController_StatusChanged;
            cvController.StatusChanged += CvController_StatusChanged;

            // Создаем контроллер калибровки
            calibrationController = new CameraCalibrationController(cameraController, this);

            // Создаем кнопки в тулбаре
            CreateCameraToolbarButtons();

            // Скрываем превью по умолчанию
            cameraPictureBox.Visible = false;
            cameraPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            cameraPictureBox.BackColor = Color.Black;
        }

        /// <summary>
        /// Создание кнопок управления камерой в тулбаре
        /// </summary>
        private void CreateCameraToolbarButtons()
        {
            // Разделитель
            toolStrip1.Items.Add(new ToolStripSeparator());

            // Кнопка включения/выключения камеры
            btnCameraToggle = new ToolStripButton
            {
                Text = "📷 Камера",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Включить/выключить камеру"
            };
            btnCameraToggle.Click += BtnCameraToggle_Click;
            toolStrip1.Items.Add(btnCameraToggle);

            // Кнопка настроек
            btnCameraSettings = new ToolStripButton
            {
                Text = "⚙️ Настройки",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Настройки камеры",
                Enabled = false
            };
            btnCameraSettings.Click += BtnCameraSettings_Click;
            toolStrip1.Items.Add(btnCameraSettings);

            // Кнопка калибровки (НОВАЯ!)
            btnCameraCalibrate = new ToolStripButton
            {
                Text = "🎯 Калибровка",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Автоматическая калибровка камеры (мм/пиксель)",
                Enabled = false
            };
            btnCameraCalibrate.Click += BtnCameraCalibrate_Click;
            toolStrip1.Items.Add(btnCameraCalibrate);

            // Кнопка снимка
            btnCameraSnapshot = new ToolStripButton
            {
                Text = "📸 Снимок",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Сохранить снимок",
                Enabled = false
            };
            btnCameraSnapshot.Click += BtnCameraSnapshot_Click;
            toolStrip1.Items.Add(btnCameraSnapshot);

            // Кнопка заморозки
            btnCameraFreeze = new ToolStripButton
            {
                Text = "❄️ Стоп-кадр",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Заморозить/разморозить изображение",
                Enabled = false,
                CheckOnClick = true
            };
            btnCameraFreeze.Click += BtnCameraFreeze_Click;
            toolStrip1.Items.Add(btnCameraFreeze);
        }

        /// <summary>
        /// Обработчик включения/выключения камеры
        /// </summary>
        private void BtnCameraToggle_Click(object sender, EventArgs e)
        {
            if (!cameraController.IsRunning)
            {
                // Попытка запустить камеру
                var cameras = cameraController.GetAvailableCameras();
                
                if (cameras == null || cameras.Count == 0)
                {
                    MessageBox.Show("Камеры не обнаружены.\n\nУбедитесь, что USB-камера подключена к компьютеру.",
                        "Камера не найдена", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Если camer несколько - даем выбрать
                int selectedCamera = 0;
                if (cameras.Count > 1)
                {
                    using (var selectForm = new CameraSelectForm(cameras))
                    {
                        if (selectForm.ShowDialog() == DialogResult.OK)
                        {
                            selectedCamera = selectForm.SelectedCameraIndex;
                        }
                        else
                        {
                            return; // Пользователь отменил выбор
                        }
                    }
                }

                // Инициализация и запуск
                if (cameraController.Initialize(selectedCamera))
                {
                    if (cameraController.Start())
                    {
                        cameraPictureBox.Visible = true;
                        btnCameraToggle.Text = "🔴 Остановить";
                        btnCameraSettings.Enabled = true;
                        btnCameraCalibrate.Enabled = true;  // Активируем калибровку
                        btnCameraSnapshot.Enabled = true;
                        btnCameraFreeze.Enabled = true;
                    }
                }
            }
            else
            {
                // Остановка камеры
                cameraController.Stop();
                cameraPictureBox.Visible = false;
                cameraPictureBox.Image = null;
                btnCameraToggle.Text = "📷 Камера";
                btnCameraSettings.Enabled = false;
                btnCameraCalibrate.Enabled = false;
                btnCameraSnapshot.Enabled = false;
                btnCameraFreeze.Enabled = false;
                btnCameraFreeze.Checked = false;
            }
        }

        /// <summary>
        /// Обработчик открытия настроек камеры
        /// </summary>
        private void BtnCameraSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new CameraSettingsForm(cameraController))
            {
                settingsForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// Обработчик запуска калибровки камеры (НОВЫЙ!)
        /// </summary>
        private void BtnCameraCalibrate_Click(object sender, EventArgs e)
        {
            if (!cameraController.IsRunning)
            {
                MessageBox.Show("Включите камеру перед калибровкой!", 
                    "Калибровка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Открываем мастер калибровки
            using (var wizard = new CameraCalibrationWizard(calibrationController))
            {
                if (wizard.ShowDialog() == DialogResult.OK)
                {
                    // Получаем результат калибровки
                    var result = calibrationController.GetCalibrationResult();
                    
                    if (result != null && result.IsValid())
                    {
                        // Устанавливаем масштаб в CV контроллер
                        cvController.SetCameraScale(result.MillimetersPerPixel);
                        
                        // Сохраняем в настройки
                        SaveCameraCalibration(result);
                        
                        MessageBox.Show(
                            $"Автоматическая калибровка выполнена успешно!\n\n" +
                            $"Масштаб камеры: {result.MillimetersPerPixel:F5} мм/пиксель\n" +
                            $"                ({result.MillimetersPerPixel * 1000:F2} мкм/пиксель)\n\n" +
                            $"Калибровка сохранена и будет использоваться для всех измерений.",
                            "Калибровка завершена",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        
                        UpdateUI();
                    }
                }
            }
        }

        /// <summary>
        /// Сохранение результатов калибровки
        /// </summary>
        private void SaveCameraCalibration(CameraCalibrationData data)
        {
            try
            {
                // Сохраняем в настройки приложения
                Properties.Settings.Default.CameraCalibrationScale = data.MillimetersPerPixel;
                Properties.Settings.Default.CameraCalibrationDate = data.CalibrationDate;
                Properties.Settings.Default.CameraCalibrationResolution = $"{data.ImageResolution.Width}x{data.ImageResolution.Height}";
                Properties.Settings.Default.Save();
                
                AppLogger.Info($"Калибровка камеры сохранена: {data.MillimetersPerPixel:F5} мм/px");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Ошибка сохранения калибровки: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Загрузка сохраненной калибровки
        /// </summary>
        private void LoadCameraCalibration()
        {
            try
            {
                double scale = Properties.Settings.Default.CameraCalibrationScale;
                
                if (scale > 0)
                {
                    cvController.SetCameraScale(scale);
                    var date = Properties.Settings.Default.CameraCalibrationDate;
                    var resolution = Properties.Settings.Default.CameraCalibrationResolution;
                    
                    AppLogger.Info($"Калибровка камеры загружена: {scale:F5} мм/px (от {date:yyyy-MM-dd}, {resolution})");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Не удалось загрузить калибровку камеры: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик сохранения снимка
        /// </summary>
        private void BtnCameraSnapshot_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Title = "Сохранить снимок с камеры",
                Filter = "PNG файлы (*.png)|*.png|JPEG файлы (*.jpg)|*.jpg",
                DefaultExt = "png",
                FileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (cameraController.SaveSnapshot(dialog.FileName))
                    {
                        MessageBox.Show($"Снимок сохранен:\n{dialog.FileName}", 
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить снимок.", 
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик заморозки кадра
        /// </summary>
        private void BtnCameraFreeze_Click(object sender, EventArgs e)
        {
            cameraController.FreezeFrame(btnCameraFreeze.Checked);
            btnCameraToggle.Text = btnCameraFreeze.Checked ? "❄️ Заморожено" : "🔴 Остановить";
        }

        /// <summary>
        /// Обработка нового кадра с камеры
        /// </summary>
        private void CameraController_FrameCaptured(object sender, Bitmap frame)
        {
            if (cameraPictureBox.InvokeRequired)
            {
                cameraPictureBox.BeginInvoke(new Action(() =>
                {
                    UpdateCameraPreview(frame);
                }));
            }
            else
            {
                UpdateCameraPreview(frame);
            }
        }

        /// <summary>
        /// Обновление превью камеры
        /// </summary>
        private void UpdateCameraPreview(Bitmap frame)
        {
            try
            {
                var oldImage = cameraPictureBox.Image;
                cameraPictureBox.Image = (Bitmap)frame.Clone();
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Ошибка обновления превью: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Обработка изменения статуса камеры
        /// </summary>
        private void CameraController_StatusChanged(object sender, string message)
        {
            if (StatusLabel != null && StatusLabel.Owner != null)
            {
                if (StatusLabel.Owner.InvokeRequired)
                {
                    StatusLabel.Owner.BeginInvoke(new Action(() =>
                    {
                        StatusLabel.Text = $"Камера: {message}";
                    }));
                }
                else
                {
                    StatusLabel.Text = $"Камера: {message}";
                }
            }
        }

        /// <summary>
        /// Обработка изменения статуса CV
        /// </summary>
        private void CvController_StatusChanged(object sender, string message)
        {
            AppLogger.Debug($"CV: {message}");
        }

        /// <summary>
        /// Освобождение ресурсов камеры
        /// </summary>
        private void DisposeCameraResources()
        {
            calibrationController?.Dispose();
            cameraController?.Dispose();
        }
    }

    /// <summary>
    /// Форма выбора камеры (если подключено несколько)
    /// </summary>
    internal class CameraSelectForm : Form
    {
        private ComboBox cameraComboBox;
        public int SelectedCameraIndex { get; private set; }

        public CameraSelectForm(AForge.Video.DirectShow.FilterInfoCollection cameras)
        {
            this.Text = "Выбор камеры";
            this.Size = new Size(400, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var label = new Label
            {
                Text = "Обнаружено несколько камер.\nВыберите камеру для использования:",
                Location = new Point(10, 10),
                AutoSize = true
            };

            cameraComboBox = new ComboBox
            {
                Location = new Point(10, 50),
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (AForge.Video.DirectShow.FilterInfo camera in cameras)
            {
                cameraComboBox.Items.Add(camera.Name);
            }
            cameraComboBox.SelectedIndex = 0;

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(210, 80),
                Width = 80
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(300, 80),
                Width = 80
            };

            this.Controls.AddRange(new Control[] { label, cameraComboBox, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            btnOk.Click += (s, e) => SelectedCameraIndex = cameraComboBox.SelectedIndex;
        }
    }
}
