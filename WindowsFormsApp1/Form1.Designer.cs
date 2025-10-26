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
            this.cameraPictureBox = new System.Windows.Forms.PictureBox();
            this.groupBoxMainControl = new System.Windows.Forms.GroupBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.toOriginButton = new System.Windows.Forms.Button();
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
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.totalCrystalsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.selectedCrystalStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fillPercentageLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.zoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.calibrationStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.coordinatesLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.sensorStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MyserialPort = new System.IO.Ports.SerialPort(this.components);
            this.mainPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.groupBoxManualControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonMoveRight = new System.Windows.Forms.Button();
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.buttonMoveLeft = new System.Windows.Forms.Button();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.scan = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonCalibrateZero = new System.Windows.Forms.Button();
            this.checkBoxDiscreteStep = new System.Windows.Forms.CheckBox();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.buttonUpdatePort = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.comboBoxPorts = new System.Windows.Forms.ComboBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox)).BeginInit();
            this.groupBoxMainControl.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pictureBox1.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.groupBoxManualControl.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxConnection.SuspendLayout();
            this.SuspendLayout();
            // 
            // cameraPictureBox
            // 
            this.cameraPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cameraPictureBox.Location = new System.Drawing.Point(769, 3);
            this.cameraPictureBox.Name = "cameraPictureBox";
            this.cameraPictureBox.Size = new System.Drawing.Size(160, 120);
            this.cameraPictureBox.TabIndex = 2;
            this.cameraPictureBox.TabStop = false;
            // 
            // groupBoxMainControl
            // 
            this.groupBoxMainControl.Controls.Add(this.resetButton);
            this.groupBoxMainControl.Controls.Add(this.startButton);
            this.groupBoxMainControl.Controls.Add(this.toOriginButton);
            this.groupBoxMainControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxMainControl.Location = new System.Drawing.Point(10, 10);
            this.groupBoxMainControl.Name = "groupBoxMainControl";
            this.groupBoxMainControl.Size = new System.Drawing.Size(290, 65);
            this.groupBoxMainControl.TabIndex = 4;
            this.groupBoxMainControl.TabStop = false;
            this.groupBoxMainControl.Text = "Главное управление";
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(197, 23);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(85, 28);
            this.resetButton.TabIndex = 2;
            this.resetButton.Text = "Сброс";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(101, 23);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(85, 28);
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Пуск";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // toOriginButton
            // 
            this.toOriginButton.Location = new System.Drawing.Point(9, 23);
            this.toOriginButton.Name = "toOriginButton";
            this.toOriginButton.Size = new System.Drawing.Size(85, 28);
            this.toOriginButton.TabIndex = 0;
            this.toOriginButton.Text = "В исходное";
            this.toOriginButton.UseVisualStyleBackColor = true;
            this.toOriginButton.Click += new System.EventHandler(this.toOriginButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.debugToolStripMenuItem});
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
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugModeToolStripMenuItem,
            this.resetCalibrationToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(86, 24);
            this.debugToolStripMenuItem.Text = "Наладка";
            // 
            // debugModeToolStripMenuItem
            // 
            this.debugModeToolStripMenuItem.CheckOnClick = true;
            this.debugModeToolStripMenuItem.Name = "debugModeToolStripMenuItem";
            this.debugModeToolStripMenuItem.Size = new System.Drawing.Size(250, 26);
            this.debugModeToolStripMenuItem.Text = "Работа без COM-порта";
            this.debugModeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.debugModeToolStripMenuItem_CheckedChanged);
            // 
            // resetCalibrationToolStripMenuItem
            // 
            this.resetCalibrationToolStripMenuItem.Name = "resetCalibrationToolStripMenuItem";
            this.resetCalibrationToolStripMenuItem.Size = new System.Drawing.Size(250, 26);
            this.resetCalibrationToolStripMenuItem.Text = "Сбросить калибровку";
            this.resetCalibrationToolStripMenuItem.Click += new System.EventHandler(this.resetCalibrationToolStripMenuItem_Click);
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
            this.totalCrystalsStatusLabel,
            this.selectedCrystalStatusLabel,
            this.fillPercentageLabel,
            this.zoomLabel,
            this.calibrationStatusLabel,
            this.coordinatesLabel,
            this.sensorStatusLabel});
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
            // totalCrystalsStatusLabel
            // 
            this.totalCrystalsStatusLabel.Name = "totalCrystalsStatusLabel";
            this.totalCrystalsStatusLabel.Size = new System.Drawing.Size(238, 20);
            this.totalCrystalsStatusLabel.Text = "Общее количество кристаллов: 0";
            // 
            // selectedCrystalStatusLabel
            // 
            this.selectedCrystalStatusLabel.Name = "selectedCrystalStatusLabel";
            this.selectedCrystalStatusLabel.Size = new System.Drawing.Size(174, 20);
            this.selectedCrystalStatusLabel.Text = "Кристаллы не выбраны";
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
            // calibrationStatusLabel
            // 
            this.calibrationStatusLabel.Name = "calibrationStatusLabel";
            this.calibrationStatusLabel.Size = new System.Drawing.Size(148, 20);
            this.calibrationStatusLabel.Text = "Калибровка: нет";
            // 
            // coordinatesLabel
            // 
            this.coordinatesLabel.Name = "coordinatesLabel";
            this.coordinatesLabel.Size = new System.Drawing.Size(63, 20);
            this.coordinatesLabel.Text = "X: 0, Y: 0";
            // 
            // sensorStatusLabel
            // 
            this.sensorStatusLabel.Name = "sensorStatusLabel";
            this.sensorStatusLabel.Size = new System.Drawing.Size(71, 20);
            this.sensorStatusLabel.Text = "Датчик: ?";
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.pictureBox1);
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
            this.pictureBox1.Controls.Add(this.cameraPictureBox);
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(10, 10);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(932, 572);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.groupBoxManualControl);
            this.rightPanel.Controls.Add(this.groupBoxConnection);
            this.rightPanel.Controls.Add(this.groupBoxMainControl);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightPanel.Location = new System.Drawing.Point(952, 55);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.rightPanel.Size = new System.Drawing.Size(310, 592);
            this.rightPanel.TabIndex = 3;
            // 
            // groupBoxManualControl
            // 
            this.groupBoxManualControl.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManualControl.Controls.Add(this.checkBoxDiscreteStep);
            this.groupBoxManualControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxManualControl.Location = new System.Drawing.Point(10, 368);
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
            this.tableLayoutPanel2.Controls.Add(this.buttonCalibrateZero, 2, 3);
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
            this.tableLayoutPanel2.SetColumnSpan(this.buttonStart, 2);
            this.buttonStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStart.Location = new System.Drawing.Point(3, 96);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(125, 26);
            this.buttonStart.TabIndex = 4;
            this.buttonStart.Text = "Старт";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonCalibrateZero
            // 
            this.buttonCalibrateZero.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCalibrateZero.Location = new System.Drawing.Point(131, 96);
            this.buttonCalibrateZero.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.buttonCalibrateZero.Name = "buttonCalibrateZero";
            this.buttonCalibrateZero.Size = new System.Drawing.Size(59, 26);
            this.buttonCalibrateZero.TabIndex = 5;
            this.buttonCalibrateZero.Text = "Калибр.";
            this.buttonCalibrateZero.UseVisualStyleBackColor = true;
            this.buttonCalibrateZero.Click += new System.EventHandler(this.SetCalibrationZero_Click);
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
            // groupBoxConnection
            // 
            this.groupBoxConnection.Controls.Add(this.buttonUpdatePort);
            this.groupBoxConnection.Controls.Add(this.buttonConnect);
            this.groupBoxConnection.Controls.Add(this.comboBoxPorts);
            this.groupBoxConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxConnection.Location = new System.Drawing.Point(10, 75);
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
            this.buttonUpdatePort.Text = "Обнов.";
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
            this.buttonConnect.Text = "Подкл.";
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
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox)).EndInit();
            this.groupBoxMainControl.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pictureBox1.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.groupBoxManualControl.ResumeLayout(false);
            this.groupBoxManualControl.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCalibrationToolStripMenuItem;
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
        private System.Windows.Forms.ToolStripStatusLabel totalCrystalsStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel selectedCrystalStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel fillPercentageLabel;
        private System.Windows.Forms.ToolStripStatusLabel zoomLabel;
        private System.Windows.Forms.ToolStripStatusLabel calibrationStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel coordinatesLabel;
        private System.Windows.Forms.ToolStripStatusLabel sensorStatusLabel;
        private System.IO.Ports.SerialPort MyserialPort;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.GroupBox groupBoxMainControl;
        private System.Windows.Forms.Button toOriginButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.GroupBox groupBoxManualControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonMoveRight;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveLeft;
        private System.Windows.Forms.Button buttonMoveDown;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonCalibrateZero;
        private System.Windows.Forms.Button scan;
        private System.Windows.Forms.CheckBox checkBoxDiscreteStep;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.Button buttonUpdatePort;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ComboBox comboBoxPorts;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.PictureBox cameraPictureBox;
    }
}

