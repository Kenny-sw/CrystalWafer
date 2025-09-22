using CrystalTable.Data;
using CrystalTable.Logic;
using CrystalTable.Controllers;
using System;
using System.Drawing;
using System.Globalization;
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

        // История операций
        private readonly CommandHistory commandHistory = new CommandHistory();

        // Предпросмотр маршрута
        private readonly RoutePreview routePreview = new RoutePreview();
        private bool showRoutePreview = false;

        // Для дросселирования обновления статус-бара по RX
        private DateTime _lastRxUiUpdate = DateTime.MinValue;

        public Form1()
        {
            InitializeComponent();
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

            // RX/STATE > статус-бар
            serialPortController.DataReceived += SerialPort_DataReceived;
            serialPortController.ConnectionStateChanged += SerialPort_ConnectionStateChanged;

            InitializeEventHandlers();
            LoadDefaultConfiguration();
            UpdateUI();
        }

        private void InitializeEventHandlers()
        {
            SizeX.Validated += (s, e) => uiController.ValidateInput(SizeX, ref waferController.SizeXtemp);
            SizeY.Validated += (s, e) => uiController.ValidateInput(SizeY, ref waferController.SizeYtemp);
            WaferDiameter.Validated += (s, e) => uiController.ValidateWaferDiameter(WaferDiameter, ref waferController.WaferDiameterTemp);

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
        }

        // ===== PictureBox =====
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) => mouseController.HandleMouseDown(e);
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) => mouseController.HandleMouseMove(e);
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) => mouseController.HandleMouseUp(e);
        private void Form1_Resize(object sender, EventArgs e) => pictureBox1.Invalidate();

        // ===== Загрузка набора =====
        private void loadDataComboBox_SelectedIndexChanged(object sender, EventArgs e) => SetFieldsFromComboBox();

        // ===== Кнопки =====
        private void SaveButton_Click(object sender, EventArgs e) =>
            exportImportController.SaveWaferInfo(SizeX.Text, SizeY.Text, WaferDiameter.Text);

        private void checkBoxFillWafer_CheckedChanged(object sender, EventArgs e)
        {
            waferController.WaferDisplayMode = checkBoxFillWafer.Checked;
            UpdateUI();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement hardware reset logic
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement start logic
        }

        private void toOriginButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement move to origin logic
        }

        // ===== Тулбар =====
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
        private void newToolStripMenuItem_Click(object sender, EventArgs e) => Create_Click(sender, e);
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
        private void buttonConnect_Click(object sender, EventArgs e) =>
            serialPortController.ToggleConnection(comboBoxPorts.Text, buttonConnect, comboBoxPorts);

        private void buttonUpdatePort_Click(object sender, EventArgs e) =>
            serialPortController.UpdatePortList(comboBoxPorts);

        private void MyserialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) =>
            serialPortController.HandleDataReceived();

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
                uiController.ClearInputFields(SizeX, SizeY, WaferDiameter);
                waferController.CreateNewWafer();
                mouseController.ClearSelection();
                CenterPointerAndView();   // <— центрируем
                commandHistory.Clear();
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

            SizeX.Text = info.SizeX.ToString();
            SizeY.Text = info.SizeY.ToString();
            WaferDiameter.Text = info.WaferDiameter.ToString(CultureSettings.NumericCulture);

            if (info.HasCalibration)
            {
                try
                {
                    if (lblFirstRef != null)
                    {
                        lblFirstRef.Text = $"Первый: {info.FirstReferenceX:F3} мм; {info.FirstReferenceY:F3} мм";
                    }

                    if (lblLastRef != null)
                    {
                        lblLastRef.Text = $"Последний: {info.LastReferenceX:F3} мм; {info.LastReferenceY:F3} мм";
                    }
                }
                catch
                {
                    // UI элемент может отсутствовать в режиме тестов
                }
            }

            try
            {
                UpdateCalibrationLabelsAfterBuild();
            }
            catch
            {
            }
        }
        private void UpdateWaferVisualization() => pictureBox1.Refresh();

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPortController?.Dispose();
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
        public ToolStripStatusLabel SelectedCrystalStatusLabel => selectedCrystalStatusLabel;
        public ToolStripStatusLabel TotalCrystalsStatusLabel => totalCrystalsStatusLabel;
        public ToolStripStatusLabel StatusLabel => statusLabel;
        public ToolStripStatusLabel FillPercentageLabel => fillPercentageLabel;
        public ToolStripStatusLabel ZoomLabel => zoomLabel;
        public ToolStripStatusLabel CoordinatesLabel => coordinatesLabel;
        public CheckBox CheckBoxFillWafer => checkBoxFillWafer;
        public ToolStripButton BtnRoutePreview => btnRoutePreview;
        public ToolStripMenuItem ShowRouteToolStripMenuItem => showRouteToolStripMenuItem;
        public SerialPortController SerialPortController => serialPortController;
        public ToolStripButton BtnUndo => btnUndo;
        public ToolStripButton BtnRedo => btnRedo;

        // ====== RX > статус-бар ======
        private void SerialPort_DataReceived(string data)
        {
            var now = DateTime.Now;
            if ((now - _lastRxUiUpdate).TotalMilliseconds < 150) return;
            _lastRxUiUpdate = now;

            string preview = FormatPacketPreview(data);
            if (StatusLabel != null)
                StatusLabel.Text = $"COM RX {now:HH:mm:ss}  {preview}";
            else
                Text = $"COM RX {now:HH:mm:ss}  {preview}";
        }

        private void SerialPort_ConnectionStateChanged(bool isOpen, string portName)
        {
            var msg = isOpen ? $"COM подключён: {portName}" : "COM отключён";
            if (StatusLabel != null) StatusLabel.Text = msg;
        }

        private static string FormatPacketPreview(string s)
        {
            if (string.IsNullOrEmpty(s)) return "(пусто)";
            s = s.Replace("\r", " ").Replace("\n", " ").Trim();
            const int max = 60;
            if (s.Length > max) s = s.Substring(0, max) + "…";
            return s;
        }

        // ====== ОБРАБОТЧИКИ ======

        // «Создать»
        private void Create_Click(object sender, EventArgs e)
        {
            string ReadMasked(MaskedTextBox box)
            {
                var prev = box.TextMaskFormat;
                try
                {
                    box.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
                    return (box.Text ?? string.Empty).Trim();
                }
                finally
                {
                    box.TextMaskFormat = prev;
                }
            }

            string sizeXRaw = ReadMasked(SizeX);
            string sizeYRaw = ReadMasked(SizeY);
            string diaRaw = ReadMasked(WaferDiameter);

            if (waferController.CreateWaferFromInput(sizeXRaw, sizeYRaw, diaRaw, out string errorMessage))
            {
                CenterPointerAndView();
                try { InitializeCalibrationUiState(); } catch { /* не критично */ }
                UpdateUI();
            }
            else
            {
                MessageBox.Show(errorMessage, "Новая пластина", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        // «Выбрать первый»
        private void btnSelectFirst_Click(object sender, EventArgs e)
        {
            var p = TryGetPointerOrZero();
            waferController.SetFirstReference(p.X, p.Y);
            try { if (lblFirstRef != null) lblFirstRef.Text = $"Первый: {p.X:F3} мм; {p.Y:F3} мм"; } catch { }
            try { UpdateBuildMapEnabled(); } catch { }
            UpdateUI();
        }

        // «Выбрать последний»
        private void btnSelectLast_Click(object sender, EventArgs e)
        {
            var p = TryGetPointerOrZero();
            waferController.SetLastReference(p.X, p.Y);
            try { if (lblLastRef != null) lblLastRef.Text = $"Последний: {p.X:F3} мм; {p.Y:F3} мм"; } catch { }
            try { UpdateBuildMapEnabled(); } catch { }
            UpdateUI();
        }

        // «Построить карту»
        private void btnBuildMap_Click(object sender, EventArgs e)
        {
            bool ok = false;

            if (waferController.IsCalibrationReady())
            {
                waferController.BuildMapFromReferences();
                ok = true;
            }
            else if (waferController.IsPresetReady())
            {
                waferController.BuildMapFromPreset();
                ok = true;
            }

            if (!ok)
            {
                MessageBox.Show("Построение карты недоступно. Задайте шаги (SizeX/SizeY) или выберите две опорные точки.",
                    "Построить карту", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try { UpdateCalibrationLabelsAfterBuild(); } catch { }
            pictureBox1?.Invalidate();
            UpdateUI();
        }

        // Вспомогательные
        private PointF TryGetPointerOrZero()
        {
            try { return GetPointerMm(); } // реализован в Form1.Movement.cs (partial)
            catch { return new PointF(0f, 0f); }
        }

        private void CenterPointerAndView()
        {
            try { CenterPointer(); } catch { }   // из Form1.Movement.cs
            zoomPanController.Reset();           // центр и 1.0x
        }

        private void cameraPictureBox_Click(object sender, EventArgs e)
        {

        }
    }
}

       

        
