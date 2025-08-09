// Класс для инициализации компонентов интерфейса Windows Forms
using System;
using System.Windows.Forms;

namespace CrystalTable
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

// Класс для инициализации интерфейса формы
namespace CrystalTable
{
    partial class Form1
    {
        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonUpdatePort = new System.Windows.Forms.Button();
            this.comboBoxPorts = new System.Windows.Forms.ComboBox();
            this.MyserialPort = new System.IO.Ports.SerialPort(this.components);
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.buttonMoveLeft = new System.Windows.Forms.Button();
            this.buttonMoveRight = new System.Windows.Forms.Button();
            this.SizeX = new System.Windows.Forms.MaskedTextBox();
            this.SizeY = new System.Windows.Forms.MaskedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.WaferDiameter = new System.Windows.Forms.MaskedTextBox();
            this.labelTotalCrystals = new System.Windows.Forms.Label();
            this.labelSelectedCrystal = new System.Windows.Forms.Label();
            this.Create = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelIndex = new System.Windows.Forms.Label();
            this.loadDataComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.scan = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
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
            this.checkBoxFillWafer = new System.Windows.Forms.CheckBox();
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
            this.groupBoxCalibration = new System.Windows.Forms.GroupBox();
            this.btnBuildMap = new System.Windows.Forms.Button();
            this.btnSelectLast = new System.Windows.Forms.Button();
            this.btnSelectFirst = new System.Windows.Forms.Button();
            this.lblRows = new System.Windows.Forms.Label();
            this.lblCols = new System.Windows.Forms.Label();
            this.lblPitchY = new System.Windows.Forms.Label();
            this.lblPitchX = new System.Windows.Forms.Label();
            this.lblLastRef = new System.Windows.Forms.Label();
            this.lblFirstRef = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBoxCalibration.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonConnect.Location = new System.Drawing.Point(1271, 98);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(4);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(137, 28);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Подключить";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonUpdatePort
            // 
            this.buttonUpdatePort.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonUpdatePort.Location = new System.Drawing.Point(1271, 134);
            this.buttonUpdatePort.Margin = new System.Windows.Forms.Padding(4);
            this.buttonUpdatePort.Name = "buttonUpdatePort";
            this.buttonUpdatePort.Size = new System.Drawing.Size(137, 28);
            this.buttonUpdatePort.TabIndex = 1;
            this.buttonUpdatePort.Text = "Список портов";
            this.buttonUpdatePort.UseVisualStyleBackColor = true;
            this.buttonUpdatePort.Click += new System.EventHandler(this.buttonUpdatePort_Click);
            // 
            // comboBoxPorts
            // 
            this.comboBoxPorts.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.comboBoxPorts.FormattingEnabled = true;
            this.comboBoxPorts.Location = new System.Drawing.Point(1102, 102);
            this.comboBoxPorts.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPorts.Name = "comboBoxPorts";
            this.comboBoxPorts.Size = new System.Drawing.Size(160, 24);
            this.comboBoxPorts.TabIndex = 2;
            // 
            // MyserialPort
            // 
            this.MyserialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.MyserialPort_DataReceived);
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonMoveUp.Location = new System.Drawing.Point(1200, 308);
            this.buttonMoveUp.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(100, 28);
            this.buttonMoveUp.TabIndex = 3;
            this.buttonMoveUp.Text = "Вверх";
            this.buttonMoveUp.UseVisualStyleBackColor = true;
            this.buttonMoveUp.Click += new System.EventHandler(this.buttonMoveUp_Click);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonMoveDown.Location = new System.Drawing.Point(1200, 344);
            this.buttonMoveDown.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(100, 28);
            this.buttonMoveDown.TabIndex = 4;
            this.buttonMoveDown.Text = "Вниз";
            this.buttonMoveDown.UseVisualStyleBackColor = true;
            this.buttonMoveDown.Click += new System.EventHandler(this.buttonMoveDown_Click);
            // 
            // buttonMoveLeft
            // 
            this.buttonMoveLeft.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonMoveLeft.Location = new System.Drawing.Point(1092, 343);
            this.buttonMoveLeft.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMoveLeft.Name = "buttonMoveLeft";
            this.buttonMoveLeft.Size = new System.Drawing.Size(100, 28);
            this.buttonMoveLeft.TabIndex = 5;
            this.buttonMoveLeft.Text = "Влево";
            this.buttonMoveLeft.UseVisualStyleBackColor = true;
            this.buttonMoveLeft.Click += new System.EventHandler(this.buttonMoveLeft_Click);
            // 
            // buttonMoveRight
            // 
            this.buttonMoveRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonMoveRight.Location = new System.Drawing.Point(1308, 343);
            this.buttonMoveRight.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMoveRight.Name = "buttonMoveRight";
            this.buttonMoveRight.Size = new System.Drawing.Size(100, 28);
            this.buttonMoveRight.TabIndex = 6;
            this.buttonMoveRight.Text = "Вправо";
            this.buttonMoveRight.UseVisualStyleBackColor = true;
            this.buttonMoveRight.Click += new System.EventHandler(this.buttonMoveRight_Click);
            // 
            // SizeX
            // 
            this.SizeX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.SizeX.BeepOnError = true;
            this.SizeX.Location = new System.Drawing.Point(1275, 380);
            this.SizeX.Margin = new System.Windows.Forms.Padding(4);
            this.SizeX.Mask = "00000";
            this.SizeX.Name = "SizeX";
            this.SizeX.PromptChar = ' ';
            this.SizeX.Size = new System.Drawing.Size(132, 22);
            this.SizeX.TabIndex = 7;
            this.SizeX.ValidatingType = typeof(int);
            // 
            // SizeY
            // 
            this.SizeY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.SizeY.BeepOnError = true;
            this.SizeY.Location = new System.Drawing.Point(1275, 412);
            this.SizeY.Margin = new System.Windows.Forms.Padding(4);
            this.SizeY.Mask = "00000";
            this.SizeY.Name = "SizeY";
            this.SizeY.PromptChar = ' ';
            this.SizeY.Size = new System.Drawing.Size(132, 22);
            this.SizeY.TabIndex = 8;
            this.SizeY.ValidatingType = typeof(int);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1193, 380);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 16);
            this.label1.TabIndex = 9;
            this.label1.Text = "Шаг по Х";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1197, 419);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Шаг по У";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(41, 82);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(847, 638);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1130, 446);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 16);
            this.label5.TabIndex = 15;
            this.label5.Text = "Диаметр пластины";
            // 
            // WaferDiameter
            // 
            this.WaferDiameter.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.WaferDiameter.BeepOnError = true;
            this.WaferDiameter.Location = new System.Drawing.Point(1275, 440);
            this.WaferDiameter.Margin = new System.Windows.Forms.Padding(4);
            this.WaferDiameter.Mask = "00000";
            this.WaferDiameter.Name = "WaferDiameter";
            this.WaferDiameter.PromptChar = ' ';
            this.WaferDiameter.Size = new System.Drawing.Size(132, 22);
            this.WaferDiameter.TabIndex = 14;
            this.WaferDiameter.ValidatingType = typeof(int);
            // 
            // labelTotalCrystals
            // 
            this.labelTotalCrystals.AutoSize = true;
            this.labelTotalCrystals.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.labelTotalCrystals.Location = new System.Drawing.Point(227, 58);
            this.labelTotalCrystals.Name = "labelTotalCrystals";
            this.labelTotalCrystals.Size = new System.Drawing.Size(244, 18);
            this.labelTotalCrystals.TabIndex = 13;
            this.labelTotalCrystals.Text = "Общее количество кристаллов: 0";
            // 
            // labelSelectedCrystal
            // 
            this.labelSelectedCrystal.AutoSize = true;
            this.labelSelectedCrystal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.labelSelectedCrystal.Location = new System.Drawing.Point(563, 58);
            this.labelSelectedCrystal.Name = "labelSelectedCrystal";
            this.labelSelectedCrystal.Size = new System.Drawing.Size(173, 18);
            this.labelSelectedCrystal.TabIndex = 16;
            this.labelSelectedCrystal.Text = "Кристаллы не выбраны";
            // 
            // Create
            // 
            this.Create.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Create.Location = new System.Drawing.Point(1271, 653);
            this.Create.Margin = new System.Windows.Forms.Padding(4);
            this.Create.Name = "Create";
            this.Create.Size = new System.Drawing.Size(100, 28);
            this.Create.TabIndex = 17;
            this.Create.Text = "Создать";
            this.Create.UseVisualStyleBackColor = true;
            this.Create.Click += new System.EventHandler(this.Create_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1092, 685);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 16);
            this.label3.TabIndex = 18;
            this.label3.Text = "X: 0";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1095, 718);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 16);
            this.label4.TabIndex = 19;
            this.label4.Text = "Y: 0";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelIndex
            // 
            this.labelIndex.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelIndex.AutoSize = true;
            this.labelIndex.Location = new System.Drawing.Point(1070, 751);
            this.labelIndex.Name = "labelIndex";
            this.labelIndex.Size = new System.Drawing.Size(137, 16);
            this.labelIndex.TabIndex = 20;
            this.labelIndex.Text = "Индекс кристалла: -";
            // 
            // loadDataComboBox
            // 
            this.loadDataComboBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.loadDataComboBox.FormattingEnabled = true;
            this.loadDataComboBox.Location = new System.Drawing.Point(1275, 478);
            this.loadDataComboBox.Name = "loadDataComboBox";
            this.loadDataComboBox.Size = new System.Drawing.Size(133, 24);
            this.loadDataComboBox.TabIndex = 21;
            this.loadDataComboBox.SelectedIndexChanged += new System.EventHandler(this.loadDataComboBox_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1165, 486);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 16);
            this.label6.TabIndex = 22;
            this.label6.Text = "Тип изделия";
            // 
            // scan
            // 
            this.scan.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.scan.Location = new System.Drawing.Point(1255, 685);
            this.scan.Name = "scan";
            this.scan.Size = new System.Drawing.Size(126, 57);
            this.scan.TabIndex = 23;
            this.scan.Text = "Сканирование";
            this.scan.UseVisualStyleBackColor = true;
            this.scan.Click += new System.EventHandler(this.scan_Click);
            // 
            // saveButton
            // 
            this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.saveButton.Location = new System.Drawing.Point(1271, 617);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(100, 28);
            this.saveButton.TabIndex = 24;
            this.saveButton.Text = "Сохранить";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.LightGray;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1426, 28);
            this.menuStrip1.TabIndex = 25;
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
            // checkBoxFillWafer
            // 
            this.checkBoxFillWafer.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.checkBoxFillWafer.AutoSize = true;
            this.checkBoxFillWafer.Location = new System.Drawing.Point(1267, 517);
            this.checkBoxFillWafer.Name = "checkBoxFillWafer";
            this.checkBoxFillWafer.Size = new System.Drawing.Size(114, 20);
            this.checkBoxFillWafer.TabIndex = 26;
            this.checkBoxFillWafer.Text = "Режим схемы";
            this.checkBoxFillWafer.UseVisualStyleBackColor = true;
            this.checkBoxFillWafer.CheckedChanged += new System.EventHandler(this.checkBoxFillWafer_CheckedChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
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
            this.toolStrip1.Size = new System.Drawing.Size(1426, 27);
            this.toolStrip1.TabIndex = 27;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnUndo
            // 
            this.btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUndo.Image = global::CrystalTable.Properties.Resources.undo;
            this.btnUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(29, 24);
            this.btnUndo.Text = "Отменить";
            this.btnUndo.ToolTipText = "Отменить (Ctrl+Z)";
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnRedo
            // 
            this.btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRedo.Image = global::CrystalTable.Properties.Resources.redo;
            this.btnRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(29, 24);
            this.btnRedo.Text = "Повторить";
            this.btnRedo.ToolTipText = "Повторить (Ctrl+Y)";
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
            this.btnExport.Text = "Экспорт";
            this.btnExport.ToolTipText = "Экспорт данных (Ctrl+E)";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnImport.Image = global::CrystalTable.Properties.Resources.import;
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(29, 24);
            this.btnImport.Text = "Импорт";
            this.btnImport.ToolTipText = "Импорт данных (Ctrl+I)";
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
            this.btnRoutePreview.Text = "Маршрут";
            this.btnRoutePreview.ToolTipText = "Предпросмотр маршрута (Ctrl+R)";
            this.btnRoutePreview.Click += new System.EventHandler(this.btnRoutePreview_Click);
            // 
            // btnStatistics
            // 
            this.btnStatistics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStatistics.Image = global::CrystalTable.Properties.Resources.statistics;
            this.btnStatistics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStatistics.Name = "btnStatistics";
            this.btnStatistics.Size = new System.Drawing.Size(29, 24);
            this.btnStatistics.Text = "Статистика";
            this.btnStatistics.ToolTipText = "Показать статистику (Ctrl+T)";
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
            this.btnZoomIn.Text = "Увеличить";
            this.btnZoomIn.ToolTipText = "Увеличить (Ctrl++)";
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOut.Image = global::CrystalTable.Properties.Resources.zoom_out;
            this.btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(29, 24);
            this.btnZoomOut.Text = "Уменьшить";
            this.btnZoomOut.ToolTipText = "Уменьшить (Ctrl+-)";
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnZoomReset
            // 
            this.btnZoomReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomReset.Image = global::CrystalTable.Properties.Resources.zoom_reset;
            this.btnZoomReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomReset.Name = "btnZoomReset";
            this.btnZoomReset.Size = new System.Drawing.Size(29, 24);
            this.btnZoomReset.Text = "Сбросить масштаб";
            this.btnZoomReset.ToolTipText = "Сбросить масштаб (Ctrl+0)";
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 769);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1426, 26);
            this.statusStrip1.TabIndex = 28;
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
            // groupBoxCalibration
            // 
            this.groupBoxCalibration.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.groupBoxCalibration.Controls.Add(this.btnBuildMap);
            this.groupBoxCalibration.Controls.Add(this.btnSelectLast);
            this.groupBoxCalibration.Controls.Add(this.btnSelectFirst);
            this.groupBoxCalibration.Controls.Add(this.lblRows);
            this.groupBoxCalibration.Controls.Add(this.lblCols);
            this.groupBoxCalibration.Controls.Add(this.lblPitchY);
            this.groupBoxCalibration.Controls.Add(this.lblPitchX);
            this.groupBoxCalibration.Controls.Add(this.lblLastRef);
            this.groupBoxCalibration.Controls.Add(this.lblFirstRef);
            this.groupBoxCalibration.Location = new System.Drawing.Point(894, 69);
            this.groupBoxCalibration.Name = "groupBoxCalibration";
            this.groupBoxCalibration.Size = new System.Drawing.Size(184, 273);
            this.groupBoxCalibration.TabIndex = 30;
            this.groupBoxCalibration.TabStop = false;
            this.groupBoxCalibration.Text = "Калибровка (2 точки)";
            // 
            // btnBuildMap
            // 
            this.btnBuildMap.Location = new System.Drawing.Point(16, 215);
            this.btnBuildMap.Name = "btnBuildMap";
            this.btnBuildMap.Size = new System.Drawing.Size(153, 32);
            this.btnBuildMap.TabIndex = 8;
            this.btnBuildMap.Text = "Построить карту";
            this.btnBuildMap.UseVisualStyleBackColor = true;
            this.btnBuildMap.Click += new System.EventHandler(this.btnBuildMap_Click);
            // 
            // btnSelectLast
            // 
            this.btnSelectLast.Location = new System.Drawing.Point(16, 69);
            this.btnSelectLast.Name = "btnSelectLast";
            this.btnSelectLast.Size = new System.Drawing.Size(153, 28);
            this.btnSelectLast.TabIndex = 2;
            this.btnSelectLast.Text = "Выбрать последний";
            this.btnSelectLast.UseVisualStyleBackColor = true;
            this.btnSelectLast.Click += new System.EventHandler(this.btnSelectLast_Click);
            // 
            // btnSelectFirst
            // 
            this.btnSelectFirst.Location = new System.Drawing.Point(16, 17);
            this.btnSelectFirst.Name = "btnSelectFirst";
            this.btnSelectFirst.Size = new System.Drawing.Size(153, 28);
            this.btnSelectFirst.TabIndex = 1;
            this.btnSelectFirst.Text = "Выбрать первый";
            this.btnSelectFirst.UseVisualStyleBackColor = true;
            this.btnSelectFirst.Click += new System.EventHandler(this.btnSelectFirst_Click);
            // 
            // lblRows
            // 
            this.lblRows.AutoSize = true;
            this.lblRows.Location = new System.Drawing.Point(13, 191);
            this.lblRows.Name = "lblRows";
            this.lblRows.Size = new System.Drawing.Size(82, 16);
            this.lblRows.TabIndex = 7;
            this.lblRows.Text = "Rows (Ny): 0";
            // 
            // lblCols
            // 
            this.lblCols.AutoSize = true;
            this.lblCols.Location = new System.Drawing.Point(13, 173);
            this.lblCols.Name = "lblCols";
            this.lblCols.Size = new System.Drawing.Size(74, 16);
            this.lblCols.TabIndex = 6;
            this.lblCols.Text = "Cols (Nx): 0";
            // 
            // lblPitchY
            // 
            this.lblPitchY.AutoSize = true;
            this.lblPitchY.Location = new System.Drawing.Point(13, 155);
            this.lblPitchY.Name = "lblPitchY";
            this.lblPitchY.Size = new System.Drawing.Size(75, 16);
            this.lblPitchY.TabIndex = 5;
            this.lblPitchY.Text = "PitchY: 0.00";
            // 
            // lblPitchX
            // 
            this.lblPitchX.AutoSize = true;
            this.lblPitchX.Location = new System.Drawing.Point(13, 137);
            this.lblPitchX.Name = "lblPitchX";
            this.lblPitchX.Size = new System.Drawing.Size(74, 16);
            this.lblPitchX.TabIndex = 4;
            this.lblPitchX.Text = "PitchX: 0.00";
            // 
            // lblLastRef
            // 
            this.lblLastRef.AutoSize = true;
            this.lblLastRef.Location = new System.Drawing.Point(13, 118);
            this.lblLastRef.Name = "lblLastRef";
            this.lblLastRef.Size = new System.Drawing.Size(110, 16);
            this.lblLastRef.TabIndex = 3;
            this.lblLastRef.Text = "Последний: —, —";
            // 
            // lblFirstRef
            // 
            this.lblFirstRef.AutoSize = true;
            this.lblFirstRef.Location = new System.Drawing.Point(13, 100);
            this.lblFirstRef.Name = "lblFirstRef";
            this.lblFirstRef.Size = new System.Drawing.Size(88, 16);
            this.lblFirstRef.TabIndex = 0;
            this.lblFirstRef.Text = "Первый: —, —";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(1426, 795);
            this.Controls.Add(this.groupBoxCalibration);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.checkBoxFillWafer);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.scan);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.loadDataComboBox);
            this.Controls.Add(this.labelIndex);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Create);
            this.Controls.Add(this.labelSelectedCrystal);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.WaferDiameter);
            this.Controls.Add(this.labelTotalCrystals);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SizeY);
            this.Controls.Add(this.SizeX);
            this.Controls.Add(this.buttonMoveRight);
            this.Controls.Add(this.buttonMoveLeft);
            this.Controls.Add(this.buttonMoveDown);
            this.Controls.Add(this.buttonMoveUp);
            this.Controls.Add(this.comboBoxPorts);
            this.Controls.Add(this.buttonUpdatePort);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "CrystalTable - Управление полупроводниковыми пластинами";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBoxCalibration.ResumeLayout(false);
            this.groupBoxCalibration.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonUpdatePort;
        private System.Windows.Forms.ComboBox comboBoxPorts;
        private System.IO.Ports.SerialPort MyserialPort;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveDown;
        private System.Windows.Forms.Button buttonMoveLeft;
        private System.Windows.Forms.Button buttonMoveRight;
        private System.Windows.Forms.MaskedTextBox SizeX;
        private System.Windows.Forms.MaskedTextBox SizeY;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox WaferDiameter;
        private System.Windows.Forms.Label labelTotalCrystals;
        private System.Windows.Forms.Label labelSelectedCrystal;
        private System.Windows.Forms.Button Create;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelIndex;
        private System.Windows.Forms.ComboBox loadDataComboBox;
        private System.Windows.Forms.Label label6;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button scan;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.CheckBox checkBoxFillWafer;
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

        // Пункты меню
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

        public ToolStrip toolStrip1;

        // --- Калибровка (новые элементы) ---
        private System.Windows.Forms.GroupBox groupBoxCalibration;
        private System.Windows.Forms.Button btnBuildMap;
        private System.Windows.Forms.Button btnSelectLast;
        private System.Windows.Forms.Button btnSelectFirst;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.Label lblCols;
        private System.Windows.Forms.Label lblPitchY;
        private System.Windows.Forms.Label lblPitchX;
        private System.Windows.Forms.Label lblLastRef;
        private System.Windows.Forms.Label lblFirstRef;
    }
}
