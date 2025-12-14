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

        // ✅ ДОБАВЛЕНО: Рендерер отладочных оверлеев
        private readonly DebugOverlayRenderer debugOverlay;

        // ✅ ДОБАВЛЕНО: Рендерер Bin Map (карта годности)
        private readonly BinMapRenderer binMapRenderer;
        private bool showBinMap = true;

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
   
      // ✅ ДОБАВЛЕНО: Инициализация отладочных оверлеев
debugOverlay = new DebugOverlayRenderer
      {
          ShowPositionDiagnostics = false,
          ShowCalibrationPoints = false
            };
            
            // ✅ ДОБАВЛЕНО: Инициализация Bin Map рендерера
            binMapRenderer = new BinMapRenderer();
            
            // ✅ ДОБАВЛЕНО: Добавление пунктов меню Bin Map в меню "Вид"
            InitializeBinMapMenu();
            
      InitializeMapBuilderUi();
            InitializeCamera();  // ← Инициализация камеры

        // RX/STATE > статус-бар
            serialPortController.UnsolicitedEventReceived += SerialPort_UnsolicitedEventReceived;
            serialPortController.ConnectionStateChanged += SerialPort_ConnectionStateChanged;
            serialPortController.ProfileDataReceived += SerialPort_ProfileDataReceived; // ✅ Подписка на данные профиля

            InitializeEventHandlers();
            
            // ✅ Автообновление списка COM-портов при запуске
            serialPortController.UpdatePortList(comboBoxPorts);
            
            LoadDefaultConfiguration();
            LoadCameraCalibration();  // ← Загружаем сохраненную калибровку камеры
            InitializeScanUI();  // ✅ Инициализация UI автообхода
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

            // ===== Горячие клавиши автообхода =====
            if (HandleScanHotKeys(e))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            // ===== Горячие клавиши для Bin Map (категории годности) =====
            // Работают только если есть выделенные кристаллы
            if (mouseController.SelectedCrystals.Count > 0)
            {
                BinCategory? newBin = null;
                
                switch (e.KeyCode)
                {
                    case Keys.F1:
                        newBin = BinCategory.Good;
                        break;
                    case Keys.F2:
                        newBin = BinCategory.Defective;
                        break;
                    case Keys.F3:
                        newBin = BinCategory.NeedsReview;
                        break;
                    case Keys.F4:
                        newBin = BinCategory.Rework;
                        break;
                    case Keys.F5:
                        newBin = BinCategory.Edge;
                        break;
                    case Keys.Delete:
                    case Keys.Back:
                        // Сброс категории
                        newBin = BinCategory.NotInspected;
                        break;
                }

                if (newBin.HasValue)
                {
                    SetBinForSelectedCrystals(newBin.Value);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            // ===== Переключение отображения Bin Map =====
            if (e.KeyCode == Keys.B && !e.Control && !e.Alt)
            {
                showBinMap = !showBinMap;
                binMapRenderer.Settings.Enabled = showBinMap;
                UpdateUI();
                e.Handled = true;
                return;
            }

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

        /// <summary>
        /// Установить категорию годности для выделенных кристаллов
        /// </summary>
        private void SetBinForSelectedCrystals(BinCategory bin)
        {
            var crystals = CrystalManager.Instance.Crystals;
            var selected = mouseController.SelectedCrystals;
            
            if (crystals == null || selected.Count == 0)
                return;

            int count = 0;
            foreach (var crystal in crystals)
            {
                if (selected.Contains(crystal.Index))
                {
                    crystal.SetBin(bin);
                    count++;
                }
            }

            if (count > 0)
            {
                string binName = BinMapSettings.GetBinName(bin);
                statusLabel.Text = $"{count} кристалл(ов) → {binName}";
                AppLogger.Info($"Bin Map: {count} кристаллов помечены как '{binName}'");
            }

            UpdateUI();
        }

        public void UpdateUI()
        {
            pictureBox1.Invalidate();
            uiController.UpdateStatusBar(waferController, zoomPanController);
            uiController.UpdateSelectionLabel(mouseController.SelectedCrystals);
            uiController.UpdateToolbarState(commandHistory);
            SyncMapBuilderUi();
            UpdateMiniMap();  // ✅ Обновление миникарты
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
            // Используем прямую ссылку на кнопку из Designer
            if (buttonLockToggle != null)
            {
                if (isLocked)
                {
                    buttonLockToggle.Text = "СБРОС";
                    buttonLockToggle.BackColor = Color.FromArgb(231, 76, 60); // Красный
                }
                else
                {
                    buttonLockToggle.Text = "ФИКСАЦИЯ";
                    buttonLockToggle.BackColor = Color.FromArgb(46, 204, 113); // Зеленый
                }
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

        /// <summary>
        /// Открыть настройки Bin Map
        /// </summary>
        private void ShowBinMapSettings()
        {
            try
            {
                var settingsForm = new Forms.BinMapSettingsForm(binMapRenderer, () =>
                {
                    showBinMap = binMapRenderer.Settings.Enabled;
                    UpdateUI();
                });
                settingsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка открытия настроек Bin Map", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Переключить отображение Bin Map
        /// </summary>
        private void ToggleBinMap()
        {
            showBinMap = !showBinMap;
            binMapRenderer.Settings.Enabled = showBinMap;
            UpdateUI();
        }

        /// <summary>
        /// Инициализация меню Bin Map
        /// </summary>
        private void InitializeBinMapMenu()
        {
            // Создаём разделитель и пункты меню для Bin Map
            var separator = new ToolStripSeparator();
            
            var showBinMapMenuItem = new ToolStripMenuItem
            {
                Text = "Показать Bin Map (B)",
                CheckOnClick = true,
                Checked = showBinMap
            };
            showBinMapMenuItem.Click += (s, e) =>
            {
                showBinMap = showBinMapMenuItem.Checked;
                binMapRenderer.Settings.Enabled = showBinMap;
                UpdateUI();
            };

            var binMapSettingsMenuItem = new ToolStripMenuItem
            {
                Text = "Настройки Bin Map..."
            };
            binMapSettingsMenuItem.Click += (s, e) => ShowBinMapSettings();

            // Вставляем после "Показать статистику"
            int insertIndex = viewToolStripMenuItem.DropDownItems.IndexOf(showStatisticsToolStripMenuItem) + 1;
            viewToolStripMenuItem.DropDownItems.Insert(insertIndex, separator);
            viewToolStripMenuItem.DropDownItems.Insert(insertIndex + 1, showBinMapMenuItem);
            viewToolStripMenuItem.DropDownItems.Insert(insertIndex + 2, binMapSettingsMenuItem);
        }

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
            debugOverlay?.Dispose();  // ✅ Освобождение ресурсов оверлеев
            binMapRenderer?.Dispose(); // ✅ Освобождение ресурсов Bin Map
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
        
        // ✅ ДОБАВЛЕНО: Свойства для Bin Map
        public BinMapRenderer BinMapRenderer => binMapRenderer;
        public bool ShowBinMap => showBinMap;

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

        /// <summary>
        /// ✅ НОВОЕ: Обработчик получения данных профиля от Arduino
        /// </summary>
        private void SerialPort_ProfileDataReceived(string profileData)
        {
            if (string.IsNullOrWhiteSpace(profileData))
                return;

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(new Action<string>(SerialPort_ProfileDataReceived), profileData);
                return;
            }

            try
            {
                var profile = MotionProfileManager.ParseProfileFromArduino(profileData);
                if (profile != null)
                {
                    AppLogger.Info($"Получен профиль от Arduino: minDelay={profile.MinDelayUs}, maxDelay={profile.MaxDelayUs}");
                    // Можно показать диалог или обновить UI
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка обработки данных профиля", ex);
            }
        }

        private void UpdateSensorStatusLabel(string text)
        {
            if (sensorStatusLabel != null)
            {
                sensorStatusLabel.Text = text;
            }
        }

        // ====== ОБРАБОТЧИКИ ОВЕРЛЕЕВ ОТЛАДКИ ======
        
        /// <summary>
        /// Переключение диагностики позиции
        /// </summary>
        private void inspectorCoordinatesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            debugOverlay.ShowPositionDiagnostics = inspectorCoordinatesToolStripMenuItem.Checked;
            pictureBox1.Invalidate();
            AppLogger.Debug($"Диагностика позиции: {(debugOverlay.ShowPositionDiagnostics ? "ВКЛ" : "ВЫКЛ")}");
        }

        /// <summary>
        /// Переключение точек калибровки
        /// </summary>
        private void inspectorCalibrationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            debugOverlay.ShowCalibrationPoints = inspectorCalibrationToolStripMenuItem.Checked;
            pictureBox1.Invalidate();
            AppLogger.Debug($"Точки калибровки: {(debugOverlay.ShowCalibrationPoints ? "ВКЛ" : "ВЫКЛ")}");
        }

        /// <summary>
        /// ✅ НОВОЕ: Открытие редактора профилей движения
        /// </summary>
        private void motionProfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var editorForm = new Forms.MotionProfileEditorForm(this);
                editorForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка открытия редактора профилей", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ✅ НОВОЕ: Быстрый выбор профиля движения
        /// </summary>
        private void quickSelectProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var manager = MotionProfileManager.Instance;
                var profiles = manager.Profiles;

                if (profiles.Count == 0)
                {
                    MessageBox.Show("Нет доступных профилей.", "Быстрый выбор",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Создаём простой диалог выбора
                using (var selectForm = new Form())
                {
                    selectForm.Text = "Быстрый выбор профиля";
                    selectForm.Size = new Size(400, 300);
                    selectForm.StartPosition = FormStartPosition.CenterParent;
                    selectForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    selectForm.MaximizeBox = false;
                    selectForm.MinimizeBox = false;

                    var listBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 10f),
                        DisplayMember = "Name"
                    };

                    foreach (var profile in profiles)
                    {
                        listBox.Items.Add(profile);
                    }

                    // Выделяем активный профиль
                    if (manager.ActiveProfile != null)
                    {
                        int index = listBox.Items.IndexOf(manager.ActiveProfile);
                        if (index >= 0)
                        {
                            listBox.SelectedIndex = index;
                        }
                    }

                    var buttonPanel = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 50,
                        FlowDirection = FlowDirection.RightToLeft,
                        Padding = new Padding(5)
                    };

                    var btnCancel = new Button
                    {
                        Text = "Отмена",
                        Width = 100,
                        DialogResult = DialogResult.Cancel
                    };

                    var btnOk = new Button
                    {
                        Text = "Применить",
                        Width = 100,
                        DialogResult = DialogResult.OK
                    };

                    buttonPanel.Controls.Add(btnCancel);
                    buttonPanel.Controls.Add(btnOk);

                    selectForm.Controls.Add(listBox);
                    selectForm.Controls.Add(buttonPanel);
                    selectForm.AcceptButton = btnOk;
                    selectForm.CancelButton = btnCancel;

                    // Двойной клик = выбор
                    listBox.DoubleClick += (s, args) =>
                    {
                        if (listBox.SelectedItem != null)
                        {
                            selectForm.DialogResult = DialogResult.OK;
                            selectForm.Close();
                        }
                    };

                    if (selectForm.ShowDialog(this) == DialogResult.OK && listBox.SelectedItem is MotionProfile selected)
                    {
                        manager.SetActiveProfile(selected);

                        // Применяем профиль к Arduino
                        var applyTask = manager.ApplyProfileToArduino(serialPortController, selected);
                        applyTask.ContinueWith(t =>
                        {
                            if (IsHandleCreated && !IsDisposed)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    if (t.Result)
                                    {
                                        MessageBox.Show($"Профиль '{selected.Name}' применён!", "Успех",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Профиль '{selected.Name}' активирован, но не удалось отправить в Arduino.\n" +
                                            "Проверьте подключение COM-порта.", "Частичный успех",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }));
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка быстрого выбора профиля", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====== ОБРАБОТЧИКИ ======
    }
}

