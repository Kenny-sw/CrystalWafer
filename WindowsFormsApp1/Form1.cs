using CrystalTable.Data;
using CrystalTable.Logic;
using CrystalTable.Controllers;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Контроллеры
        private readonly WaferController waferController;
        private readonly MouseController mouseController;
        private readonly ZoomPanController zoomPanController;
        private readonly UIController uiController;
        private readonly ExportImportController exportImportController;
        private readonly SerialPortController serialPortController;
        
        // ✅ ДОБАВЛЕНО: Упрощенный контроллер для тестирования
        private SimpleSerialController simpleSerialController;
        private bool useSimpleProtocol = false; // Флаг использования упрощенного протокола

        // История операций
        private readonly CommandHistory commandHistory = new CommandHistory();

        // Предпросмотр маршрута
        private readonly RoutePreview routePreview = new RoutePreview();
        private bool showRoutePreview = false;

        // РЕЖИМ ОТЛАДКИ: Работа без COM-порта
        private bool debugModeWithoutComPort = false;

        // Состояние фиксации
        private bool isLocked = false;

        // Для дросселирования обновления статус-бара по RX

        public Form1()
        {
            InitializeComponent();
            debugModeToolStripMenuItem.Checked = debugModeWithoutComPort;
            buttonStart.Text = "Старт";
            buttonStart.Visible = true;

            // Устраняем мерцания при перерисовке
            try
            {
                pictureBox1.GetType()
                    .GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .SetValue(pictureBox1, true, null);
            }
            catch { /* no-op */ }

            waferController = new WaferController(this);
            mouseController = new MouseController(this, waferController);
            zoomPanController = new ZoomPanController(this);
            uiController = new UIController(this);
            exportImportController = new ExportImportController(this, waferController);
            serialPortController = new SerialPortController(MyserialPort);
            InitializeMapBuilderUi();
            InitializeCamera();  // ← Инициализация камеры

            // RX/STATE > статус-бар
            serialPortController.UnsolicitedEventReceived += SerialPort_UnsolicitedEventReceived;
            serialPortController.ConnectionStateChanged += SerialPort_ConnectionStateChanged;

            InitializeEventHandlers();
            LoadDefaultConfiguration();
            LoadCameraCalibration();  // ← Загружаем сохраненную калибровку камеры
            UpdateUI();
        }

        private void InitializeEventHandlers()
        {
            // Удалены обработчики для SizeX, SizeY, WaferDiameter (теперь в MapBuilder)

            pictureBox1.MouseWheel += (s, e) =>
            {
                zoomPanController.HandleMouseWheel(e);
                UpdateUI();
            };

            KeyPreview = true;
            KeyDown += HandleKeyDown;
            KeyUp += (s, e) => mouseController.HandleKeyUp(e);

            commandHistory.HistoryChanged += (s, e) => uiController.UpdateToolbarState(commandHistory);

            FormClosing += (s, e) => SaveLastConfiguration();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            mouseController.HandleKeyDown(e);

            if (e.Control)
            {
                if (e.KeyCode == Keys.Z && commandHistory.CanUndo())
                {
                    commandHistory.Undo();
                    UpdateUI();
                }
                else if (e.KeyCode == Keys.Y && commandHistory.CanRedo())
                {
                    commandHistory.Redo();
                    UpdateUI();
                }
            }
        }

        public void UpdateUI()
        {
            pictureBox1.Invalidate();
            uiController.UpdateStatusBar(waferController, zoomPanController);
            uiController.UpdateSelectionLabel(mouseController.SelectedCrystals);
            uiController.UpdateToolbarState(commandHistory);
            SyncMapBuilderUi();
        }

        // ===== PictureBox =====
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) => mouseController.HandleMouseDown(e);
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) => mouseController.HandleMouseMove(e);
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) => mouseController.HandleMouseUp(e);
        private void Form1_Resize(object sender, EventArgs e) => pictureBox1.Invalidate();

        // ===== Кнопки =====
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Сохранение через WaferController
            string sizeXText = waferController.CrystalWidthRaw.ToString();
            string sizeYText = waferController.CrystalHeightRaw.ToString();
            string diameterText = waferController.WaferDiameter.ToString(System.Globalization.CultureInfo.InvariantCulture);
            exportImportController.SaveWaferInfo(sizeXText, sizeYText, diameterText);
        }

        private void checkBoxFillWafer_CheckedChanged(object sender, EventArgs e)
        {
            waferController.WaferDisplayMode = checkBoxFillWafer.Checked;
            pictureBox1.Invalidate();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            // Аппаратный сброс - отправка команды Unlock
            _ = ToggleLockAsync(false);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Запуск последовательности - отправка в Form1.Movement.cs
            _ = ExecuteLoadingSequenceAsync();
        }

        private void toOriginButton_Click(object sender, EventArgs e)
        {
            // Перемещение в центр (0,0)
            _ = MoveToCenterAsync();
        }
        
        /// <summary>
        /// Обработчик кнопки "Фиксация/Сброс" - переключатель состояния
        /// </summary>
        private async void buttonLockToggle_Click(object sender, EventArgs e)
        {
            await ToggleLockAsync(!isLocked);
        }
        
        /// <summary>
        /// Переключение состояния фиксации
        /// </summary>
        private async System.Threading.Tasks.Task ToggleLockAsync(bool lockState)
        {
            byte command = lockState ? Protocol.Commands.Lock : Protocol.Commands.Unlock;
            
            if (debugModeWithoutComPort)
            {
                AppLogger.Debug($"[DEBUG MODE] {(lockState ? "Фиксация" : "Сброс")} - команда 0x{command:X2}");
                isLocked = lockState;
                UpdateLockButtonState();
                UpdateUI();
                return;
            }
            
            if (!await TrySendAsync(command, 0))
            {
                MessageBox.Show($"Не удалось выполнить {(lockState ? "фиксацию" : "сброс")}.", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            isLocked = lockState;
            UpdateLockButtonState();
            UpdateUI();
            
            AppLogger.Info($"Состояние фиксации: {(isLocked ? "ЗАФИКСИРОВАНО" : "СБРОШЕНО")}");
        }
        
        /// <summary>
        /// Обновление визуального состояния кнопки фиксации
        /// </summary>
        private void UpdateLockButtonState()
        {
            // Кнопка будет найдена в Designer
            var lockButton = this.Controls.Find("buttonLockToggle", true).FirstOrDefault() as Button;
            if (lockButton != null)
            {
                lockButton.Text = isLocked ? "🔒 Сброс" : "🔓 Фиксация";
                lockButton.BackColor = isLocked ? Color.FromArgb(255, 200, 200) : Color.FromArgb(200, 255, 200);
            }
        }

        private void SetCalibrationZero_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка: если калибровка уже выполнена - запросить подтверждение
                if (waferController.IsCalibrated)
                {
                    var result = MessageBox.Show(
                        "Внимание! Текущая калибровка будет перезаписана.\n\nПродолжить?",
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result != DialogResult.Yes)
                    {
                        return; // Пользователь отменил
                    }
                }

                // Получаем физические координаты машины (где стоит ЛШД)
                var pointerMachine = GetPointerMachineMm();
                
                // Устанавливаем калибровку: запоминаем соответствие
                // виртуальной позиции первого кристалла и физической позиции ЛШД
                waferController.SetCalibrationZero(pointerMachine.X, pointerMachine.Y);
                
                // Получаем виртуальные координаты для отображения
                var pointerVirtual = GetPointerMm();
                var offsetX = waferController.CalibrationOffsetX;
                var offsetY = waferController.CalibrationOffsetY;
                
                MessageBox.Show(
                    $"Калибровка выполнена!\n\n" +
                    $"Базовый кристалл: №{waferController.CalibrationCrystalIndex} (левый верхний)\n" +
                    $"Виртуальная позиция ЛШД: ({pointerVirtual.X:F2}, {pointerVirtual.Y:F2}) мм\n" +
                    $"Смещение системы: ({offsetX:+0.00;-0.00;0}, {offsetY:+0.00;-0.00;0}) мм",
                    "Калибровка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                
                UpdateUI();
            }
            catch (InvalidOperationException ex)
            {
                // Ошибка: нет кристаллов для калибровки
                MessageBox.Show(ex.Message, "Ошибка калибровки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Обработчик переключения режима отладки (работа без COM-порта)
        /// </summary>
        private void debugModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            debugModeWithoutComPort = debugModeToolStripMenuItem.Checked;
            UpdateUI();
        }
        
        /// <summary>
        /// ✅ ДОБАВЛЕНО: Переключение упрощенного протокола для тестирования
        /// Временно: управляется через Shift+Click на кнопке Connect
        /// </summary>
        public void ToggleSimpleProtocol()
        {
            useSimpleProtocol = !useSimpleProtocol;
            
            if (useSimpleProtocol)
            {
                // Инициализация упрощенного контроллера
                if (simpleSerialController == null)
                {
                    simpleSerialController = new SimpleSerialController(MyserialPort);
                }
                
                AppLogger.Info("ПЕРЕКЛЮЧЕНО на упрощенный протокол (для тестирования)");
                MessageBox.Show(
                    "Включен упрощенный протокол.\n\n" +
                    "Особенности:\n" +
                    "• Синхронная отправка/прием\n" +
                    "• Нет отдельного listener thread\n" +
                    "• Простая диагностика\n\n" +
                    "Используйте для тестирования шагов ЛШД.\n\n" +
                    "Для возврата к стандартному - повторите Shift+Click на Connect.",
                    "Упрощенный протокол",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                AppLogger.Info("ПЕРЕКЛЮЧЕНО на стандартный протокол");
                MessageBox.Show(
                    "Возврат к стандартному протоколу.",
                    "Стандартный протокол",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            
            UpdateUI();
        }

        /// <summary>
        /// Обработчик сброса калибровки
        /// </summary>
        private void resetCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            waferController.ResetCalibration();
            MessageBox.Show("Калибровка сброшена.", "Калибровка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateUI();
        }
        
        /// <summary>
        /// Обработчик пункта меню "Просмотр логов"
        /// </summary>
        private void viewLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var logViewer = new CrystalTable.Forms.LogViewerForm();
                logViewer.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка открытия окна логов", ex);
                MessageBox.Show($"Ошибка открытия просмотрщика логов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnUndo_Click(object sender, EventArgs e) => HandleUndo();
        private void btnRedo_Click(object sender, EventArgs e) => HandleRedo();
        private void btnExport_Click(object sender, EventArgs e) => exportImportController.ExportData();
        private void btnImport_Click(object sender, EventArgs e) => ImportData();
        private void btnRoutePreview_Click(object sender, EventArgs e) => ToggleRoutePreview();
        private void btnStatistics_Click(object sender, EventArgs e) => ShowStatistics();
        private void btnZoomIn_Click(object sender, EventArgs e) => Zoom(0.2f);
        private void btnZoomOut_Click(object sender, EventArgs e) => Zoom(-0.2f);
        private void btnZoomReset_Click(object sender, EventArgs e) => ResetZoom();

        // ===== Меню =====
        private void newToolStripMenuItem_Click(object sender, EventArgs e) => CreateNewWafer();
        private void openToolStripMenuItem_Click(object sender, EventArgs e) => OpenFile();
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) => SaveButton_Click(sender, e);
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) => exportImportController.SaveAs();
        private void exportToolStripMenuItem_Click(object sender, EventArgs e) => exportImportController.ExportData();
        private void importToolStripMenuItem_Click(object sender, EventArgs e) => ImportData();
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => ExitApplication();

        private void undoToolStripMenuItem_Click(object sender, EventArgs e) => HandleUndo();
        private void redoToolStripMenuItem_Click(object sender, EventArgs e) => HandleRedo();
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) => SelectAll();
        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e) => ClearSelection();

        private void showRouteToolStripMenuItem_Click(object sender, EventArgs e) => ToggleRoutePreview();
        private void showStatisticsToolStripMenuItem_Click(object sender, EventArgs e) => ShowStatistics();
        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e) => Zoom(0.2f);
        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e) => Zoom(-0.2f);
        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e) => ResetZoom();

        // ===== COM-порт =====
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            // ✅ ДОБАВЛЕНО: Shift+Click переключает упрощенный протокол
            if (Control.ModifierKeys == Keys.Shift)
            {
                ToggleSimpleProtocol();
                return;
            }
            
            // ✅ Выбор контроллера в зависимости от режима
            if (useSimpleProtocol && simpleSerialController != null)
            {
                if (simpleSerialController.IsOpen)
                {
                    simpleSerialController.Disconnect();
                    buttonConnect.Text = "Connect";
                    StatusLabel.Text = "COM отключён";
                }
                else
                {
                    try
                    {
                        simpleSerialController.Connect(comboBoxPorts.Text);
                        buttonConnect.Text = "Disconnect";
                        StatusLabel.Text = $"COM подключён (SIMPLE): {comboBoxPorts.Text}";
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("[SIMPLE] Ошибка подключения", ex);
                        MessageBox.Show($"Ошибка подключения: {ex.Message}", "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // Стандартный протокол
                serialPortController.ToggleConnection(comboBoxPorts.Text, buttonConnect, comboBoxPorts);
            }
        }

        private void buttonUpdatePort_Click(object sender, EventArgs e) =>
            serialPortController.UpdatePortList(comboBoxPorts);

        // ===== Вспомогательные =====
        private void HandleUndo()
        {
            if (commandHistory.CanUndo())
            {
                commandHistory.Undo();
                UpdateUI();
            }
        }

        private void HandleRedo()
        {
            if (commandHistory.CanRedo())
            {
                commandHistory.Redo();
                UpdateUI();
            }
        }

        private void Zoom(float delta)
        {
            zoomPanController.Zoom(delta);
            UpdateUI();
        }

        private void ResetZoom()
        {
            zoomPanController.Reset();
            UpdateUI();
        }

        private void SelectAll()
        {
            mouseController.SelectAll(CrystalManager.Instance.Crystals);
            UpdateUI();
        }

        private void ClearSelection()
        {
            mouseController.ClearSelection();
            UpdateUI();
        }

        private void ToggleRoutePreview()
        {
            showRoutePreview = !showRoutePreview;
            btnRoutePreview.Checked = showRoutePreview;
            showRouteToolStripMenuItem.Checked = showRoutePreview;

            if (showRoutePreview)
                waferController.GenerateRoute(routePreview, mouseController.SelectedCrystals);

            UpdateUI();
        }

        private void ShowStatistics()
        {
            var stats = waferController.GetStatistics();
            if (stats != null)
            {
                var form = new StatisticsForm(
                    stats.GenerateFullReport(
                        waferController.CrystalWidthRaw / 1000f,
                        waferController.CrystalHeightRaw / 1000f),
                    stats, mouseController.SelectedCrystals);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("Нет данных для отображения статистики!",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateNewWafer()
        {
            if (MessageBox.Show("Создать новую пластину? Все несохраненные данные будут потеряны.",
                "Новая пластина", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                waferController.CreateNewWafer();
                mouseController.ClearSelection();
                CenterPointer();
                zoomPanController.Reset();
                commandHistory.Clear();
                SyncMapBuilderUi();  // Обновить MapBuilder
                UpdateUI();
            }
        }

        private void OpenFile()
        {
            var result = exportImportController.OpenFile();
            if (result.HasValue)
            {
                ApplyWaferInfoToUi(result.Value.info);
                UpdateUI();
            }
        }
        private void ImportData()
        {
            var result = exportImportController.ImportData();
            if (result.HasValue)
            {
                if (result.Value.info != null)
                {
                    ApplyWaferInfoToUi(result.Value.info);
                }

                UpdateUI();
            }
        }
        private void ExitApplication()
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void ApplyWaferInfoToUi(WaferInfo info)
        {
            if (info == null)
            {
                return;
            }

            // Синхронизация через WaferController
            waferController.CrystalWidthRaw = info.SizeX;
            waferController.CrystalHeightRaw = info.SizeY;
            waferController.WaferDiameter = info.WaferDiameter;
            waferController.SizeXtemp = info.SizeX;
            waferController.SizeYtemp = info.SizeY;
            waferController.WaferDiameterTemp = info.WaferDiameter;
            
            SyncMapBuilderUi();  // Обновить MapBuilder UI
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPortController?.Dispose();
            DisposeCameraResources();  // ← Освобождение ресурсов камеры
        }

        // Публичные свойства для UIController
        public PictureBox PictureBox => pictureBox1;
        public CommandHistory CommandHistory => commandHistory;
        public RoutePreview RoutePreview => routePreview;
        public ZoomPanController ZoomPanController => zoomPanController;
        public WaferController WaferController => waferController;
        public MouseController MouseController => mouseController;
        public bool ShowRoutePreview => showRoutePreview;
        public UIController UiController => uiController;
        public ToolStripStatusLabel StatusLabel => statusLabel;
        public ToolStripStatusLabel FillPercentageLabel => fillPercentageLabel;
        public ToolStripStatusLabel ZoomLabel => zoomLabel;
        public ToolStripStatusLabel CalibrationStatusLabel => calibrationStatusLabel;
        public ToolStripStatusLabel CoordinatesLabel => coordinatesLabel;
        public ToolStripStatusLabel SensorStatusLabel => sensorStatusLabel;
        public ToolStripStatusLabel TotalCrystalsStatusLabel => totalCrystalsStatusLabel;
        public ToolStripStatusLabel SelectedCrystalStatusLabel => selectedCrystalStatusLabel;
        public bool DebugModeWithoutComPort => debugModeWithoutComPort;
        public ToolStripButton BtnRoutePreview => btnRoutePreview;
        public ToolStripMenuItem ShowRouteToolStripMenuItem => showRouteToolStripMenuItem;
        public SerialPortController SerialPortController => serialPortController;
        public ToolStripButton BtnUndo => btnUndo;
        public ToolStripButton BtnRedo => btnRedo;

        // ====== RX > статус-бар ======
        private void SerialPort_UnsolicitedEventReceived(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(new Action<string>(SerialPort_UnsolicitedEventReceived), message);
                return;
            }

            if (!IsHandleCreated)
            {
                return;
            }

            string normalized = message.Trim();

            // Обработка событий датчика
            if (normalized.StartsWith("EV S:1", StringComparison.OrdinalIgnoreCase))
            {
                UpdateSensorStatusLabel("Датчик: ВКЛ");
            }
            else if (normalized.StartsWith("EV S:0", StringComparison.OrdinalIgnoreCase))
            {
                UpdateSensorStatusLabel("Датчик: ВЫКЛ");
            }
            // Обработка событий фиксации
            else if (Protocol.Events.TryParseLockEvent(normalized, out bool lockState))
            {
                isLocked = lockState;
                UpdateLockButtonState();
                UpdateSensorStatusLabel($"Фиксация: {(lockState ? "ВКЛ" : "ВЫКЛ")}");
                AppLogger.Info($"Получено событие фиксации от Arduino: {(lockState ? "LOCKED" : "UNLOCKED")}");
            }
            else
            {
                UpdateSensorStatusLabel($"Датчик: {normalized}");
            }
        }

        private void SerialPort_ConnectionStateChanged(bool isOpen, string portName)
        {
            var msg = isOpen ? $"COM подключён: {portName}" : "COM отключён";
            if (StatusLabel != null)
                StatusLabel.Text = msg;
            else
                Text = msg;

            UpdateSensorStatusLabel(isOpen ? "Датчик: ?" : "Датчик: -");
        }

        private void UpdateSensorStatusLabel(string text)
        {
            if (sensorStatusLabel != null)
            {
                sensorStatusLabel.Text = text;
            }
        }

        // ====== ОБРАБОТЧИКИ ======
    }
}

