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

            // Создаем панель управления камерой
            CreateCameraControlPanel();

            // Настройка превью камеры
            cameraPictureBox.Visible = false;
            cameraPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            cameraPictureBox.BackColor = Color.Black;
            cameraPictureBox.Cursor = Cursors.Hand;
            
            // Двойной клик для изменения размера
            cameraPictureBox.DoubleClick += CameraPictureBox_DoubleClick;
            
            // Подсказка
            var toolTip = new ToolTip();
            toolTip.SetToolTip(cameraPictureBox, "Двойной клик для изменения размера\nESC для возврата");
        }

        /// <summary>
        /// Создание панели управления камерой в tabPageCamera
        /// </summary>
        private void CreateCameraControlPanel()
        {
            var cameraPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            // Заголовок
            var titleLabel = new Label
            {
                Text = "Управление камерой",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Dock = DockStyle.Top,
                Height = 26
            };

            // Превью камеры
            var previewGroup = new GroupBox
            {
                Text = "Превью",
                Dock = DockStyle.Top,
                Height = 180,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(8)
            };

            var cameraPictureBoxInTab = new PictureBox
            {
                Name = "cameraPictureBoxInTab",
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            previewGroup.Controls.Add(cameraPictureBoxInTab);

            // Группа кнопок управления
            var controlGroup = new GroupBox
            {
                Text = "Управление",
                Dock = DockStyle.Top,
                Height = 180,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(8)
            };

            // TableLayoutPanel для кнопок - занимает всю ширину
            var buttonTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(0)
            };
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            buttonTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            buttonTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            buttonTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            buttonTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            var btnToggleTab = CreateCameraButtonNeutral("Включить камеру");
            btnToggleTab.Dock = DockStyle.Fill;
            btnToggleTab.Click += (s, e) => BtnCameraToggle_Click(s, e);

            var btnSettingsTab = CreateCameraButtonNeutral("Настройки");
            btnSettingsTab.Dock = DockStyle.Fill;
            btnSettingsTab.Click += (s, e) => BtnCameraSettings_Click(s, e);
            btnSettingsTab.Enabled = false;

            var btnCalibrateTab = CreateCameraButtonNeutral("Калибровка");
            btnCalibrateTab.Dock = DockStyle.Fill;
            btnCalibrateTab.Click += (s, e) => BtnCameraCalibrate_Click(s, e);
            btnCalibrateTab.Enabled = false;

            var btnSnapshotTab = CreateCameraButtonNeutral("Сохранить снимок");
            btnSnapshotTab.Dock = DockStyle.Fill;
            btnSnapshotTab.Click += (s, e) => BtnCameraSnapshot_Click(s, e);
            btnSnapshotTab.Enabled = false;

            buttonTable.Controls.Add(btnToggleTab, 0, 0);
            buttonTable.Controls.Add(btnSettingsTab, 0, 1);
            buttonTable.Controls.Add(btnCalibrateTab, 0, 2);
            buttonTable.Controls.Add(btnSnapshotTab, 0, 3);

            controlGroup.Controls.Add(buttonTable);

            // Группа статуса
            var statusGroup = new GroupBox
            {
                Text = "Статус",
                Dock = DockStyle.Top,
                Height = 70,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(8)
            };

            var statusLabel = new Label
            {
                Name = "cameraStatusLabel",
                Text = "Камера: не подключена",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            statusGroup.Controls.Add(statusLabel);

            // Собираем панель (порядок важен - снизу вверх)
            cameraPanel.Controls.Add(statusGroup);
            cameraPanel.Controls.Add(controlGroup);
            cameraPanel.Controls.Add(previewGroup);
            cameraPanel.Controls.Add(titleLabel);

            // Добавляем в tabPageCamera
            tabPageCamera.Controls.Add(cameraPanel);
        }

        /// <summary>
        /// Вспомогательный метод для создания нейтральных кнопок камеры
        /// </summary>
        private Button CreateCameraButtonNeutral(string text)
        {
            return new Button
            {
                Text = text,
                Height = 32,
                BackColor = Color.FromArgb(245, 245, 245),
                ForeColor = Color.FromArgb(64, 64, 64),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2)
            };
        }

        // Текущий режим отображения камеры
        private enum CameraDisplayMode
        {
            Small,      // 295x245 - в углу
            Large       // 640x480 - по центру
        }

        private CameraDisplayMode currentCameraMode = CameraDisplayMode.Small;
        private Point originalCameraLocation;
        private Size originalCameraSize;
        private AnchorStyles originalCameraAnchor;
        private Panel cameraOverlayPanel; // Затемнение

        /// <summary>
        /// Обработчик двойного клика для переключения размера камеры
        /// </summary>
        private void CameraPictureBox_DoubleClick(object sender, EventArgs e)
        {
            if (currentCameraMode == CameraDisplayMode.Small)
            {
                ShowLargeCameraPreview();
            }
            else
            {
                ShowSmallCameraPreview();
            }
        }

        /// <summary>
        /// Показать камеру в большом режиме (по центру)
        /// </summary>
        private void ShowLargeCameraPreview()
        {
            // Сохраняем текущие параметры
            originalCameraLocation = cameraPictureBox.Location;
            originalCameraSize = cameraPictureBox.Size;
            originalCameraAnchor = cameraPictureBox.Anchor;

            // Создаем затемненный фон
            cameraOverlayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(128, 0, 0, 0), // 50% прозрачности
                Cursor = Cursors.Default
            };

            // Добавляем подсказку на фон
            var hintLabel = new Label
            {
                Text = "ESC - закрыть    Двойной клик - вернуть",
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            hintLabel.Location = new Point(
                (pictureBox1.Width - hintLabel.Width) / 2,
                pictureBox1.Height - hintLabel.Height - 20
            );
            cameraOverlayPanel.Controls.Add(hintLabel);

            // Клик по фону закрывает большой режим
            cameraOverlayPanel.Click += (s, ev) => ShowSmallCameraPreview();

            pictureBox1.Controls.Add(cameraOverlayPanel);
            cameraOverlayPanel.BringToFront();

            // Меняем размер и позицию камеры
            cameraPictureBox.Anchor = AnchorStyles.None;
            cameraPictureBox.Size = new Size(640, 480);
            cameraPictureBox.Location = new Point(
                (pictureBox1.Width - 640) / 2,
                (pictureBox1.Height - 480) / 2
            );
            cameraPictureBox.BorderStyle = BorderStyle.Fixed3D;
            cameraPictureBox.BringToFront();

            currentCameraMode = CameraDisplayMode.Large;

            // Устанавливаем фокус для обработки ESC
            cameraPictureBox.Focus();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown_CameraOverlay;
        }

        /// <summary>
        /// Показать камеру в маленьком режиме (в углу)
        /// </summary>
        private void ShowSmallCameraPreview()
        {
            if (currentCameraMode == CameraDisplayMode.Small)
                return;

            // Удаляем затемнение
            if (cameraOverlayPanel != null)
            {
                pictureBox1.Controls.Remove(cameraOverlayPanel);
                cameraOverlayPanel.Dispose();
                cameraOverlayPanel = null;
            }

            // Восстанавливаем исходные параметры
            cameraPictureBox.Anchor = originalCameraAnchor;
            cameraPictureBox.Size = originalCameraSize;
            cameraPictureBox.Location = originalCameraLocation;
            cameraPictureBox.BorderStyle = BorderStyle.FixedSingle;

            currentCameraMode = CameraDisplayMode.Small;

            // Убираем обработчик ESC
            this.KeyDown -= Form1_KeyDown_CameraOverlay;
        }

        /// <summary>
        /// Обработчик ESC для закрытия большого режима камеры
        /// </summary>
        private void Form1_KeyDown_CameraOverlay(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && currentCameraMode == CameraDisplayMode.Large)
            {
                ShowSmallCameraPreview();
                e.Handled = true;
            }
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
            // Возвращаем маленький режим если был большой
            if (currentCameraMode == CameraDisplayMode.Large)
            {
                ShowSmallCameraPreview();
            }

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
