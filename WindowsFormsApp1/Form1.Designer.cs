namespace CrystalTable
{
    partial class Form1
    {
        /// <summary>Обязательная переменная конструктора.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Освободить все используемые ресурсы.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRouteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetZoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnUndo = new System.Windows.Forms.ToolStripButton();
            this.btnRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRoutePreview = new System.Windows.Forms.ToolStripButton();
            this.btnStatistics = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnZoomIn = new System.Windows.Forms.ToolStripButton();
            this.btnZoomOut = new System.Windows.Forms.ToolStripButton();
            this.btnZoomReset = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fillPercentageLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.zoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.coordinatesLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MyserialPort = new System.IO.Ports.SerialPort(this.components);
            this.mainPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.topInfoPanel = new System.Windows.Forms.Panel();
            this.labelSelectedCrystal = new System.Windows.Forms.Label();
            this.labelTotalCrystals = new System.Windows.Forms.Label();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.groupBoxCalibration = new System.Windows.Forms.GroupBox();
            this.btnBuildMap = new System.Windows.Forms.Button();
            this.lblRows = new System.Windows.Forms.Label();
            this.btnSelectLast = new System.Windows.Forms.Button();
            this.lblLastRef = new System.Windows.Forms.Label();
            this.lblCols = new System.Windows.Forms.Label();
            this.btnSelectFirst = new System.Windows.Forms.Button();
            this.lblFirstRef = new System.Windows.Forms.Label();
            this.lblPitchY = new System.Windows.Forms.Label();
            this.lblPitchX = new System.Windows.Forms.Label();
            this.groupBoxManualControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonMoveRight = new System.Windows.Forms.Button();
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.buttonMoveLeft = new System.Windows.Forms.Button();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.scan = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.checkBoxDiscreteStep = new System.Windows.Forms.CheckBox();
            this.groupBoxParameters = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.WaferDiameter = new System.Windows.Forms.MaskedTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SizeY = new System.Windows.Forms.MaskedTextBox();
            this.loadDataComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SizeX = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Create = new System.Windows.Forms.Button();
            this.checkBoxFillWafer = new System.Windows.Forms.CheckBox();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.buttonUpdatePort = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.comboBoxPorts = new System.Windows.Forms.ComboBox();
            this.labelIndex = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.topInfoPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.groupBoxCalibration.SuspendLayout();
            this.groupBoxManualControl.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxParameters.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBoxConnection.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1262, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exportToolStripMenuItem,
            this.importToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(59, 24);
            this.fileToolStripMenuItem.Text = "Файл";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.newToolStripMenuItem.Text = "Новый";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.openToolStripMenuItem.Text = "Открыть...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.saveToolStripMenuItem.Text = "Сохранить";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.saveAsToolStripMenuItem.Text = "Сохранить как...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(223, 6);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.exportToolStripMenuItem.Text = "Экспорт...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.importToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.importToolStripMenuItem.Text = "Импорт...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(223, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.exitToolStripMenuItem.Text = "Выход";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripMenuItem3,
            this.selectAllToolStripMenuItem,
            this.clearSelectionToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
            this.editToolStripMenuItem.Text = "Правка";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(276, 26);
            this.undoToolStripMenuItem.Text = "Отменить";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(276, 26);
            this.redoToolStripMenuItem.Text = "Повторить";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(273, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(276, 26);
            this.selectAllToolStripMenuItem.Text = "Выделить всё";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // clearSelectionToolStripMenuItem
            // 
            this.clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
            this.clearSelectionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.clearSelectionToolStripMenuItem.Size = new System.Drawing.Size(276, 26);
            this.clearSelectionToolStripMenuItem.Text = "Снять выделение";
            this.clearSelectionToolStripMenuItem.Click += new System.EventHandler(this.clearSelectionToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showRouteToolStripMenuItem,
            this.showStatisticsToolStripMenuItem,
            this.toolStripMenuItem4,
            this.zoomInToolStripMenuItem,
            this.zoomOutToolStripMenuItem,
            this.resetZoomToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            this.viewToolStripMenuItem.Text = "Вид";
            // 
            // showRouteToolStripMenuItem
            // 
            this.showRouteToolStripMenuItem.CheckOnClick = true;
            this.showRouteToolStripMenuItem.Name = "showRouteToolStripMenuItem";
            this.showRouteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.showRouteToolStripMenuItem.Size = new System.Drawing.Size(295, 26);
            this.showRouteToolStripMenuItem.Text = "Показать маршрут";
            this.showRouteToolStripMenuItem.Click += new System.EventHandler(this.showRouteToolStripMenuItem_Click);
            // 
            // showStatisticsToolStripMenuItem
            // 
            this.showStatisticsToolStripMenuItem.Name = "showStatisticsToolStripMenuItem";
            this.showStatisticsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.showStatisticsToolStripMenuItem.Size = new System.Drawing.Size(295, 26);
            this.showStatisticsToolStripMenuItem.Text = "Показать статистику";
            this.showStatisticsToolStripMenuItem.Click += new System.EventHandler(this.showStatisticsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(292, 6);
            // 
            // zoomInToolStripMenuItem
            // 
            this.zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
            this.zoomInToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.zoomInToolStripMenuItem.Size = new System.Drawing.Size(295, 26);
            this.zoomInToolStripMenuItem.Text = "Увеличить";
            this.zoomInToolStripMenuItem.Click += new System.EventHandler(this.zoomInToolStripMenuItem_Click);
            // 
            // zoomOutToolStripMenuItem
            // 
            this.zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
            this.zoomOutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.zoomOutToolStripMenuItem.Size = new System.Drawing.Size(295, 26);
            this.zoomOutToolStripMenuItem.Text = "Уменьшить";
            this.zoomOutToolStripMenuItem.Click += new System.EventHandler(this.zoomOutToolStripMenuItem_Click);
            // 
            // resetZoomToolStripMenuItem
            // 
            this.resetZoomToolStripMenuItem.Name = "resetZoomToolStripMenuItem";
            this.resetZoomToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0)));
            this.resetZoomToolStripMenuItem.Size = new System.Drawing.Size(295, 26);
            this.resetZoomToolStripMenuItem.Text = "Сбросить масштаб";
            this.resetZoomToolStripMenuItem.Click += new System.EventHandler(this.resetZoomToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnUndo,
            this.btnRedo,
            this.toolStripSeparator1,
            this.btnExport,
            this.btnImport,
            this.toolStripSeparator2,
            this.btnRoutePreview,
            this.btnStatistics,
            this.toolStripSeparator3,
            this.btnZoomIn,
            this.btnZoomOut,
            this.btnZoomReset});
            this.toolStrip1.Location = new System.Drawing.Point(0, 28);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1262, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnUndo
            // 
            this.btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUndo.Image = global::CrystalTable.Properties.Resources.undo;
            this.btnUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(29, 24);
            this.btnUndo.Text = "Отменить (Ctrl+Z)";
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnRedo
            // 
            this.btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRedo.Image = global::CrystalTable.Properties.Resources.redo;
            this.btnRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(29, 24);
            this.btnRedo.Text = "Повторить (Ctrl+Y)";
            this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnExport
            // 
            this.btnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExport.Image = global::CrystalTable.Properties.Resources.export;
            this.btnExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(29, 24);
            this.btnExport.Text = "Экспорт (Ctrl+E)";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnImport.Image = global::CrystalTable.Properties.Resources.import;
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(29, 24);
            this.btnImport.Text = "Импорт (Ctrl+I)";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRoutePreview
            // 
            this.btnRoutePreview.CheckOnClick = true;
            this.btnRoutePreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRoutePreview.Image = global::CrystalTable.Properties.Resources.route;
            this.btnRoutePreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRoutePreview.Name = "btnRoutePreview";
            this.btnRoutePreview.Size = new System.Drawing.Size(29, 24);
            this.btnRoutePreview.Text = "Маршрут (Ctrl+R)";
            this.btnRoutePreview.Click += new System.EventHandler(this.btnRoutePreview_Click);
            // 
            // btnStatistics
            // 
            this.btnStatistics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStatistics.Image = global::CrystalTable.Properties.Resources.statistics;
            this.btnStatistics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStatistics.Name = "btnStatistics";
            this.btnStatistics.Size = new System.Drawing.Size(29, 24);
            this.btnStatistics.Text = "Статистика (Ctrl+T)";
            this.btnStatistics.Click += new System.EventHandler(this.btnStatistics_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomIn.Image = global::CrystalTable.Properties.Resources.zoom_in;
            this.btnZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(29, 24);
            this.btnZoomIn.Text = "Увеличить (Ctrl++)";
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOut.Image = global::CrystalTable.Properties.Resources.zoom_out;
            this.btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(29, 24);
            this.btnZoomOut.Text = "Уменьшить (Ctrl+-)";
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnZoomReset
            // 
            this.btnZoomReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomReset.Image = global::CrystalTable.Properties.Resources.zoom_reset;
            this.btnZoomReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomReset.Name = "btnZoomReset";
            this.btnZoomReset.Size = new System.Drawing.Size(29, 24);
            this.btnZoomReset.Text = "Сбросить масштаб (Ctrl+0)";
            this.btnZoomReset.Click += new System.EventHandler(this.btnZoomReset_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.fillPercentageLabel,
            this.zoomLabel,
            this.coordinatesLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 647);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1262, 26);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(57, 20);
            this.statusLabel.Text = "Готово";
            // 
            // fillPercentageLabel
            // 
            this.fillPercentageLabel.Name = "fillPercentageLabel";
            this.fillPercentageLabel.Size = new System.Drawing.Size(121, 20);
            this.fillPercentageLabel.Text = "Заполнение: 0%";
            // 
            // zoomLabel
            // 
            this.zoomLabel.Name = "zoomLabel";
            this.zoomLabel.Size = new System.Drawing.Size(105, 20);
            this.zoomLabel.Text = "Масштаб: 1.0x";
            // 
            // coordinatesLabel
            // 
            this.coordinatesLabel.Name = "coordinatesLabel";
            this.coordinatesLabel.Size = new System.Drawing.Size(63, 20);
            this.coordinatesLabel.Text = "X: 0, Y: 0";
            // 
            // MyserialPort
            // 
            this.MyserialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.MyserialPort_DataReceived);
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.pictureBox1);
            this.mainPanel.Controls.Add(this.topInfoPanel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 55);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(10);
            this.mainPanel.Size = new System.Drawing.Size(952, 592);
            this.mainPanel.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(10, 42);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(932, 540);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // topInfoPanel
            // 
            this.topInfoPanel.Controls.Add(this.labelSelectedCrystal);
            this.topInfoPanel.Controls.Add(this.labelTotalCrystals);
            this.topInfoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topInfoPanel.Location = new System.Drawing.Point(10, 10);
            this.topInfoPanel.Name = "topInfoPanel";
            this.topInfoPanel.Size = new System.Drawing.Size(932, 32);
            this.topInfoPanel.TabIndex = 0;
            // 
            // labelSelectedCrystal
            // 
            this.labelSelectedCrystal.AutoSize = true;
            this.labelSelectedCrystal.Location = new System.Drawing.Point(313, 8);
            this.labelSelectedCrystal.Name = "labelSelectedCrystal";
            this.labelSelectedCrystal.Size = new System.Drawing.Size(158, 16);
            this.labelSelectedCrystal.TabIndex = 1;
            this.labelSelectedCrystal.Text = "Кристаллы не выбраны";
            this.labelSelectedCrystal.Click += new System.EventHandler(this.labelSelectedCrystal_Click);
            // 
            // labelTotalCrystals
            // 
            this.labelTotalCrystals.AutoSize = true;
            this.labelTotalCrystals.Location = new System.Drawing.Point(4, 8);
            this.labelTotalCrystals.Name = "labelTotalCrystals";
            this.labelTotalCrystals.Size = new System.Drawing.Size(223, 16);
            this.labelTotalCrystals.TabIndex = 0;
            this.labelTotalCrystals.Text = "Общее количество кристаллов: 0";
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.groupBoxCalibration);
            this.rightPanel.Controls.Add(this.groupBoxManualControl);
            this.rightPanel.Controls.Add(this.groupBoxParameters);
            this.rightPanel.Controls.Add(this.groupBoxConnection);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightPanel.Location = new System.Drawing.Point(952, 55);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.rightPanel.Size = new System.Drawing.Size(310, 592);
            this.rightPanel.TabIndex = 3;
            // 
            // groupBoxCalibration
            // 
            this.groupBoxCalibration.Controls.Add(this.btnBuildMap);
            this.groupBoxCalibration.Controls.Add(this.lblRows);
            this.groupBoxCalibration.Controls.Add(this.btnSelectLast);
            this.groupBoxCalibration.Controls.Add(this.lblLastRef);
            this.groupBoxCalibration.Controls.Add(this.lblCols);
            this.groupBoxCalibration.Controls.Add(this.btnSelectFirst);
            this.groupBoxCalibration.Controls.Add(this.lblFirstRef);
            this.groupBoxCalibration.Controls.Add(this.lblPitchY);
            this.groupBoxCalibration.Controls.Add(this.lblPitchX);
            this.groupBoxCalibration.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCalibration.Location = new System.Drawing.Point(10, 493);
            this.groupBoxCalibration.Name = "groupBoxCalibration";
            this.groupBoxCalibration.Size = new System.Drawing.Size(290, 240);
            this.groupBoxCalibration.TabIndex = 3;
            this.groupBoxCalibration.TabStop = false;
            this.groupBoxCalibration.Text = "Калибровка по 2 точкам";
            // 
            // btnBuildMap
            // 
            this.btnBuildMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuildMap.Location = new System.Drawing.Point(9, 196);
            this.btnBuildMap.Name = "btnBuildMap";
            this.btnBuildMap.Size = new System.Drawing.Size(275, 28);
            this.btnBuildMap.TabIndex = 2;
            this.btnBuildMap.Text = "Построить карту";
            this.btnBuildMap.UseVisualStyleBackColor = true;
            this.btnBuildMap.Click += new System.EventHandler(this.btnBuildMap_Click);
            // 
            // lblRows
            // 
            this.lblRows.AutoSize = true;
            this.lblRows.Location = new System.Drawing.Point(145, 168);
            this.lblRows.Name = "lblRows";
            this.lblRows.Size = new System.Drawing.Size(82, 16);
            this.lblRows.TabIndex = 8;
            this.lblRows.Text = "Rows (Ny): 0";
            // 
            // btnSelectLast
            // 
            this.btnSelectLast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectLast.Location = new System.Drawing.Point(9, 57);
            this.btnSelectLast.Name = "btnSelectLast";
            this.btnSelectLast.Size = new System.Drawing.Size(275, 28);
            this.btnSelectLast.TabIndex = 1;
            this.btnSelectLast.Text = "Выбрать последний";
            this.btnSelectLast.UseVisualStyleBackColor = true;
            this.btnSelectLast.Click += new System.EventHandler(this.btnSelectLast_Click);
            // 
            // lblLastRef
            // 
            this.lblLastRef.AutoSize = true;
            this.lblLastRef.Location = new System.Drawing.Point(6, 116);
            this.lblLastRef.Name = "lblLastRef";
            this.lblLastRef.Size = new System.Drawing.Size(110, 16);
            this.lblLastRef.TabIndex = 4;
            this.lblLastRef.Text = "Последний: —, —";
            // 
            // lblCols
            // 
            this.lblCols.AutoSize = true;
            this.lblCols.Location = new System.Drawing.Point(145, 142);
            this.lblCols.Name = "lblCols";
            this.lblCols.Size = new System.Drawing.Size(74, 16);
            this.lblCols.TabIndex = 7;
            this.lblCols.Text = "Cols (Nx): 0";
            // 
            // btnSelectFirst
            // 
            this.btnSelectFirst.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFirst.Location = new System.Drawing.Point(9, 23);
            this.btnSelectFirst.Name = "btnSelectFirst";
            this.btnSelectFirst.Size = new System.Drawing.Size(275, 28);
            this.btnSelectFirst.TabIndex = 0;
            this.btnSelectFirst.Text = "Выбрать первый";
            this.btnSelectFirst.UseVisualStyleBackColor = true;
            this.btnSelectFirst.Click += new System.EventHandler(this.btnSelectFirst_Click);
            // 
            // lblFirstRef
            // 
            this.lblFirstRef.AutoSize = true;
            this.lblFirstRef.Location = new System.Drawing.Point(6, 91);
            this.lblFirstRef.Name = "lblFirstRef";
            this.lblFirstRef.Size = new System.Drawing.Size(88, 16);
            this.lblFirstRef.TabIndex = 3;
            this.lblFirstRef.Text = "Первый: —, —";
            // 
            // lblPitchY
            // 
            this.lblPitchY.AutoSize = true;
            this.lblPitchY.Location = new System.Drawing.Point(6, 168);
            this.lblPitchY.Name = "lblPitchY";
            this.lblPitchY.Size = new System.Drawing.Size(75, 16);
            this.lblPitchY.TabIndex = 6;
            this.lblPitchY.Text = "PitchY: 0.00";
            // 
            // lblPitchX
            // 
            this.lblPitchX.AutoSize = true;
            this.lblPitchX.Location = new System.Drawing.Point(6, 142);
            this.lblPitchX.Name = "lblPitchX";
            this.lblPitchX.Size = new System.Drawing.Size(74, 16);
            this.lblPitchX.TabIndex = 5;
            this.lblPitchX.Text = "PitchX: 0.00";
            // 
            // groupBoxManualControl
            // 
            this.groupBoxManualControl.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManualControl.Controls.Add(this.checkBoxDiscreteStep);
            this.groupBoxManualControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxManualControl.Location = new System.Drawing.Point(10, 303);
            this.groupBoxManualControl.Name = "groupBoxManualControl";
            this.groupBoxManualControl.Size = new System.Drawing.Size(290, 190);
            this.groupBoxManualControl.TabIndex = 2;
            this.groupBoxManualControl.TabStop = false;
            this.groupBoxManualControl.Text = "Ручное управление";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.buttonMoveRight, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonMoveUp, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonMoveLeft, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonMoveDown, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.scan, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonStart, 0, 3);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(9, 21);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(193, 122);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // buttonMoveRight
            // 
            this.buttonMoveRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveRight.Location = new System.Drawing.Point(131, 33);
            this.buttonMoveRight.Name = "buttonMoveRight";
            this.buttonMoveRight.Size = new System.Drawing.Size(59, 24);
            this.buttonMoveRight.TabIndex = 3;
            this.buttonMoveRight.Text = ">";
            this.buttonMoveRight.UseVisualStyleBackColor = true;
            this.buttonMoveRight.Click += new System.EventHandler(this.buttonMoveRight_Click);
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveUp.Location = new System.Drawing.Point(67, 3);
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(58, 24);
            this.buttonMoveUp.TabIndex = 0;
            this.buttonMoveUp.Text = "^";
            this.buttonMoveUp.UseVisualStyleBackColor = true;
            this.buttonMoveUp.Click += new System.EventHandler(this.buttonMoveUp_Click);
            // 
            // buttonMoveLeft
            // 
            this.buttonMoveLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveLeft.Location = new System.Drawing.Point(3, 33);
            this.buttonMoveLeft.Name = "buttonMoveLeft";
            this.buttonMoveLeft.Size = new System.Drawing.Size(58, 24);
            this.buttonMoveLeft.TabIndex = 2;
            this.buttonMoveLeft.Text = "<";
            this.buttonMoveLeft.UseVisualStyleBackColor = true;
            this.buttonMoveLeft.Click += new System.EventHandler(this.buttonMoveLeft_Click);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveDown.Location = new System.Drawing.Point(67, 63);
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(58, 24);
            this.buttonMoveDown.TabIndex = 1;
            this.buttonMoveDown.Text = "v";
            this.buttonMoveDown.UseVisualStyleBackColor = true;
            this.buttonMoveDown.Click += new System.EventHandler(this.buttonMoveDown_Click);
            // 
            // scan
            // 
            this.scan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scan.Location = new System.Drawing.Point(67, 33);
            this.scan.Name = "scan";
            this.scan.Size = new System.Drawing.Size(58, 24);
            this.scan.TabIndex = 4;
            this.scan.Text = "SCAN";
            this.scan.UseVisualStyleBackColor = true;
            this.scan.Click += new System.EventHandler(this.scan_Click);
            // 
            // buttonStart
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.buttonStart, 3);
            this.buttonStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStart.Location = new System.Drawing.Point(3, 96);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(187, 26);
            this.buttonStart.TabIndex = 4;
            this.buttonStart.Text = "Старт";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // checkBoxDiscreteStep
            // 
            this.checkBoxDiscreteStep.AutoSize = true;
            this.checkBoxDiscreteStep.Checked = true;
            this.checkBoxDiscreteStep.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDiscreteStep.Location = new System.Drawing.Point(208, 21);
            this.checkBoxDiscreteStep.Name = "checkBoxDiscreteStep";
            this.checkBoxDiscreteStep.Size = new System.Drawing.Size(81, 20);
            this.checkBoxDiscreteStep.TabIndex = 1;
            this.checkBoxDiscreteStep.Text = "По шагу";
            this.checkBoxDiscreteStep.UseVisualStyleBackColor = true;
            // 
            // groupBoxParameters
            // 
            this.groupBoxParameters.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxParameters.Controls.Add(this.Create);
            this.groupBoxParameters.Controls.Add(this.checkBoxFillWafer);
            this.groupBoxParameters.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxParameters.Location = new System.Drawing.Point(10, 81);
            this.groupBoxParameters.Name = "groupBoxParameters";
            this.groupBoxParameters.Size = new System.Drawing.Size(290, 222);
            this.groupBoxParameters.TabIndex = 1;
            this.groupBoxParameters.TabStop = false;
            this.groupBoxParameters.Text = "Параметры пластины";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.WaferDiameter, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.SizeY, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.loadDataComboBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.SizeX, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 21);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(278, 124);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // WaferDiameter
            // 
            this.WaferDiameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WaferDiameter.Location = new System.Drawing.Point(139, 96);
            this.WaferDiameter.Mask = "000";
            this.WaferDiameter.Name = "WaferDiameter";
            this.WaferDiameter.Size = new System.Drawing.Size(136, 22);
            this.WaferDiameter.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(130, 31);
            this.label6.TabIndex = 0;
            this.label6.Text = "Тип изделия";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 31);
            this.label1.TabIndex = 2;
            this.label1.Text = "Размер X, мкм";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SizeY
            // 
            this.SizeY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SizeY.Location = new System.Drawing.Point(139, 65);
            this.SizeY.Mask = "00000";
            this.SizeY.Name = "SizeY";
            this.SizeY.Size = new System.Drawing.Size(136, 22);
            this.SizeY.TabIndex = 5;
            this.SizeY.ValidatingType = typeof(int);
            // 
            // loadDataComboBox
            // 
            this.loadDataComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadDataComboBox.FormattingEnabled = true;
            this.loadDataComboBox.Location = new System.Drawing.Point(139, 3);
            this.loadDataComboBox.Name = "loadDataComboBox";
            this.loadDataComboBox.Size = new System.Drawing.Size(136, 24);
            this.loadDataComboBox.TabIndex = 1;
            this.loadDataComboBox.SelectedIndexChanged += new System.EventHandler(this.loadDataComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 31);
            this.label2.TabIndex = 4;
            this.label2.Text = "Размер Y, мкм";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SizeX
            // 
            this.SizeX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SizeX.Location = new System.Drawing.Point(139, 34);
            this.SizeX.Mask = "00000";
            this.SizeX.Name = "SizeX";
            this.SizeX.Size = new System.Drawing.Size(136, 22);
            this.SizeX.TabIndex = 3;
            this.SizeX.ValidatingType = typeof(int);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 93);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 31);
            this.label5.TabIndex = 6;
            this.label5.Text = "Диаметр пластины";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Create
            // 
            this.Create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Create.Location = new System.Drawing.Point(184, 184);
            this.Create.Name = "Create";
            this.Create.Size = new System.Drawing.Size(100, 28);
            this.Create.TabIndex = 2;
            this.Create.Text = "Создать";
            this.Create.UseVisualStyleBackColor = true;
            this.Create.Click += new System.EventHandler(this.Create_Click);
            // 
            // checkBoxFillWafer
            // 
            this.checkBoxFillWafer.AutoSize = true;
            this.checkBoxFillWafer.Location = new System.Drawing.Point(9, 151);
            this.checkBoxFillWafer.Name = "checkBoxFillWafer";
            this.checkBoxFillWafer.Size = new System.Drawing.Size(114, 20);
            this.checkBoxFillWafer.TabIndex = 1;
            this.checkBoxFillWafer.Text = "Режим схемы";
            this.checkBoxFillWafer.UseVisualStyleBackColor = true;
            this.checkBoxFillWafer.CheckedChanged += new System.EventHandler(this.checkBoxFillWafer_CheckedChanged);
            // 
            // groupBoxConnection
            // 
            this.groupBoxConnection.Controls.Add(this.buttonUpdatePort);
            this.groupBoxConnection.Controls.Add(this.buttonConnect);
            this.groupBoxConnection.Controls.Add(this.comboBoxPorts);
            this.groupBoxConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxConnection.Location = new System.Drawing.Point(10, 10);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(290, 71);
            this.groupBoxConnection.TabIndex = 0;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "Подключение";
            // 
            // buttonUpdatePort
            // 
            this.buttonUpdatePort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdatePort.Location = new System.Drawing.Point(184, 25);
            this.buttonUpdatePort.Name = "buttonUpdatePort";
            this.buttonUpdatePort.Size = new System.Drawing.Size(35, 24);
            this.buttonUpdatePort.TabIndex = 1;
            this.buttonUpdatePort.Text = "??";
            this.buttonUpdatePort.UseVisualStyleBackColor = true;
            this.buttonUpdatePort.Click += new System.EventHandler(this.buttonUpdatePort_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnect.Location = new System.Drawing.Point(225, 25);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(59, 24);
            this.buttonConnect.TabIndex = 2;
            this.buttonConnect.Text = "?";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // comboBoxPorts
            // 
            this.comboBoxPorts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPorts.FormattingEnabled = true;
            this.comboBoxPorts.Location = new System.Drawing.Point(9, 25);
            this.comboBoxPorts.Name = "comboBoxPorts";
            this.comboBoxPorts.Size = new System.Drawing.Size(169, 24);
            this.comboBoxPorts.TabIndex = 0;
            // 
            // labelIndex
            // 
            this.labelIndex.Location = new System.Drawing.Point(0, 0);
            this.labelIndex.Name = "labelIndex";
            this.labelIndex.Size = new System.Drawing.Size(100, 23);
            this.labelIndex.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 0;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(0, 0);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 0;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(1024, 720);
            this.Name = "Form1";
            this.Text = "CrystalTable - Управление полупроводниковыми пластинами";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.topInfoPanel.ResumeLayout(false);
            this.topInfoPanel.PerformLayout();
            this.rightPanel.ResumeLayout(false);
            this.groupBoxCalibration.ResumeLayout(false);
            this.groupBoxCalibration.PerformLayout();
            this.groupBoxManualControl.ResumeLayout(false);
            this.groupBoxManualControl.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBoxParameters.ResumeLayout(false);
            this.groupBoxParameters.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBoxConnection.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showRouteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showStatisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem zoomInToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetZoomToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnUndo;
        private System.Windows.Forms.ToolStripButton btnRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnExport;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnRoutePreview;
        private System.Windows.Forms.ToolStripButton btnStatistics;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.ToolStripButton btnZoomReset;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel fillPercentageLabel;
        private System.Windows.Forms.ToolStripStatusLabel zoomLabel;
        private System.Windows.Forms.ToolStripStatusLabel coordinatesLabel;
        private System.IO.Ports.SerialPort MyserialPort;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel topInfoPanel;
        private System.Windows.Forms.Label labelSelectedCrystal;
        private System.Windows.Forms.Label labelTotalCrystals;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.Button buttonUpdatePort;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ComboBox comboBoxPorts;
        private System.Windows.Forms.GroupBox groupBoxParameters;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.MaskedTextBox WaferDiameter;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox SizeY;
        private System.Windows.Forms.ComboBox loadDataComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox SizeX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button Create;
        private System.Windows.Forms.CheckBox checkBoxFillWafer;
        private System.Windows.Forms.GroupBox groupBoxManualControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonMoveRight;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveLeft;
        private System.Windows.Forms.Button buttonMoveDown;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button scan;
        private System.Windows.Forms.CheckBox checkBoxDiscreteStep;
        private System.Windows.Forms.GroupBox groupBoxCalibration;
        private System.Windows.Forms.Button btnBuildMap;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.Button btnSelectLast;
        private System.Windows.Forms.Label lblLastRef;
        private System.Windows.Forms.Label lblCols;
        private System.Windows.Forms.Button btnSelectFirst;
        private System.Windows.Forms.Label lblFirstRef;
        private System.Windows.Forms.Label lblPitchY;
        private System.Windows.Forms.Label lblPitchX;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label labelIndex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button saveButton;
    }
}











