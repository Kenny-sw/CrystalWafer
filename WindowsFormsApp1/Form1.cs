using CrystalTable.Data;
using CrystalTable.Logic;
using CrystalTable.Controllers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Контроллеры для разделения логики
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

        public Form1()
        {
            InitializeComponent();

            // Инициализация контроллеров
            waferController = new WaferController(this);
            mouseController = new MouseController(this, waferController);
            zoomPanController = new ZoomPanController(this);
            uiController = new UIController(this);
            exportImportController = new ExportImportController(this, waferController);
            serialPortController = new SerialPortController(MyserialPort);

            InitializeEventHandlers();
            LoadDefaultConfiguration();
            UpdateUI();
        }

        /// <summary>
        /// Инициализация обработчиков событий
        /// </summary>
        private void InitializeEventHandlers()
        {
            // Валидация полей ввода
            SizeX.Validated += (s, e) => uiController.ValidateInput(SizeX, ref waferController.SizeXtemp);
            SizeY.Validated += (s, e) => uiController.ValidateInput(SizeY, ref waferController.SizeYtemp);
            WaferDiameter.Validated += (s, e) => uiController.ValidateWaferDiameter(
                WaferDiameter, ref waferController.WaferDiameterTemp);

            // События мыши
            pictureBox1.MouseWheel += (s, e) => {
                zoomPanController.HandleMouseWheel(e);
                UpdateUI();
            };

            // События клавиатуры
            this.KeyPreview = true;
            this.KeyDown += HandleKeyDown;
            this.KeyUp += (s, e) => mouseController.HandleKeyUp(e);

            // События изменения данных
            commandHistory.HistoryChanged += (s, e) => uiController.UpdateToolbarState(commandHistory);

            // События формы
            this.FormClosing += (s, e) => SaveLastConfiguration();
        }

        /// <summary>
        /// Обработчик нажатий клавиш
        /// </summary>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            mouseController.HandleKeyDown(e);

            // Undo/Redo
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
        /// Обновление всего интерфейса
        /// </summary>
        private void UpdateUI()
        {
            pictureBox1.Invalidate();
            uiController.UpdateStatusBar(waferController, zoomPanController);
            uiController.UpdateSelectionLabel(mouseController.SelectedCrystals);
            uiController.UpdateToolbarState(commandHistory);
        }

        // ===== ОБРАБОТЧИКИ СОБЫТИЙ ЭЛЕМЕНТОВ УПРАВЛЕНИЯ =====

        // События PictureBox
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseController.HandleMouseDown(e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseController.HandleMouseMove(e);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseController.HandleMouseUp(e);
        }

        // События формы
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void loadDataComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetFieldsFromComboBox();
        }

        // ===== ОБРАБОТЧИКИ КНОПОК =====

        private void Create_Click(object sender, EventArgs e)
        {
            waferController.CreateNewWafer();
            zoomPanController.Reset();
            mouseController.ClearSelection();
            commandHistory.Clear();
            UpdateUI();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            exportImportController.SaveWaferInfo(SizeX.Text, SizeY.Text, WaferDiameter.Text);
        }

        private void checkBoxFillWafer_CheckedChanged(object sender, EventArgs e)
        {
            waferController.WaferDisplayMode = checkBoxFillWafer.Checked;
            UpdateUI();
        }

        // ===== ПАНЕЛЬ ИНСТРУМЕНТОВ =====

        private void btnUndo_Click(object sender, EventArgs e) => HandleUndo();
        private void btnRedo_Click(object sender, EventArgs e) => HandleRedo();
        private void btnExport_Click(object sender, EventArgs e) => exportImportController.ExportData();
        private void btnImport_Click(object sender, EventArgs e) => ImportData();
        private void btnRoutePreview_Click(object sender, EventArgs e) => ToggleRoutePreview();
        private void btnStatistics_Click(object sender, EventArgs e) => ShowStatistics();
        private void btnZoomIn_Click(object sender, EventArgs e) => Zoom(0.2f);
        private void btnZoomOut_Click(object sender, EventArgs e) => Zoom(-0.2f);
        private void btnZoomReset_Click(object sender, EventArgs e) => ResetZoom();

        // ===== МЕНЮ =====

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

        // ===== COM-ПОРТ =====

        private void buttonConnect_Click(object sender, EventArgs e) =>
            serialPortController.ToggleConnection(comboBoxPorts.Text, buttonConnect, comboBoxPorts);

        private void buttonUpdatePort_Click(object sender, EventArgs e) =>
            serialPortController.UpdatePortList(comboBoxPorts);

        private void MyserialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) =>
            serialPortController.HandleDataReceived();

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

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
            {
                waferController.GenerateRoute(routePreview, mouseController.SelectedCrystals);
            }

            UpdateUI();
        }

        private void ShowStatistics()
        {
            var stats = waferController.GetStatistics();
            if (stats != null)
            {
                var form = new StatisticsForm(stats.GenerateFullReport(
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
                zoomPanController.Reset();
                commandHistory.Clear();
                UpdateUI();
            }
        }

        private void OpenFile()
        {
            var result = exportImportController.OpenFile();
            if (result.HasValue)
            {
                SizeX.Text = result.Value.info.SizeX.ToString();
                SizeY.Text = result.Value.info.SizeY.ToString();
                WaferDiameter.Text = result.Value.info.WaferDiameter.ToString();
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
                    SizeX.Text = result.Value.info.SizeX.ToString();
                    SizeY.Text = result.Value.info.SizeY.ToString();
                    WaferDiameter.Text = result.Value.info.WaferDiameter.ToString();
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

        private void UpdateWaferVisualization()
        {
            pictureBox1.Refresh();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPortController?.Dispose();
        }

        // ===== ПУБЛИЧНЫЕ СВОЙСТВА ДЛЯ КОНТРОЛЛЕРОВ =====

        public PictureBox PictureBox => pictureBox1;
        public CommandHistory CommandHistory => commandHistory;
        public RoutePreview RoutePreview => routePreview;
        public bool ShowRoutePreview => showRoutePreview;
        public Label LabelX => label3;
        public Label LabelY => label4;
        public Label LabelIndex => labelIndex;
        public Label LabelSelectedCrystal => labelSelectedCrystal;
        public Label LabelTotalCrystals => labelTotalCrystals;
        public ToolStripStatusLabel StatusLabel => statusLabel;
        public ToolStripStatusLabel FillPercentageLabel => fillPercentageLabel;
        public ToolStripStatusLabel ZoomLabel => zoomLabel;
        public CheckBox CheckBoxFillWafer => checkBoxFillWafer;
        public ToolStripButton BtnRoutePreview => btnRoutePreview;
        public ToolStripMenuItem ShowRouteToolStripMenuItem => showRouteToolStripMenuItem;
        public SerialPortController SerialPortController => serialPortController;
    }
}