// Класс для инициализации компонентов интерфейса Windows Forms
namespace WindowsFormsApp1
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
namespace WindowsFormsApp1
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.scan = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.buttonMoveUp.Location = new System.Drawing.Point(1173, 286);
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
            this.buttonMoveDown.Location = new System.Drawing.Point(1173, 322);
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
            this.buttonMoveLeft.Location = new System.Drawing.Point(1065, 321);
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
            this.buttonMoveRight.Location = new System.Drawing.Point(1281, 321);
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
            this.SizeX.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SizeX_MouseClick);
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
            this.SizeY.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SizeY_MouseClick);
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
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(41, 82);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(963, 575);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
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
            this.WaferDiameter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.WaferDiameter_MouseClick);
            // 
            // labelTotalCrystals
            // 
            this.labelTotalCrystals.AutoSize = true;
            this.labelTotalCrystals.Location = new System.Drawing.Point(227, 22);
            this.labelTotalCrystals.Name = "labelTotalCrystals";
            this.labelTotalCrystals.Size = new System.Drawing.Size(210, 16);
            this.labelTotalCrystals.TabIndex = 13;
            this.labelTotalCrystals.Text = "Общее количество кристаллов";
            // 
            // labelSelectedCrystal
            // 
            this.labelSelectedCrystal.AutoSize = true;
            this.labelSelectedCrystal.Location = new System.Drawing.Point(561, 22);
            this.labelSelectedCrystal.Name = "labelSelectedCrystal";
            this.labelSelectedCrystal.Size = new System.Drawing.Size(185, 16);
            this.labelSelectedCrystal.TabIndex = 16;
            this.labelSelectedCrystal.Text = "Текущий индекс кристалла";
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
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1092, 685);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 16);
            this.label3.TabIndex = 18;
            this.label3.Text = "label3";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1095, 718);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 16);
            this.label4.TabIndex = 19;
            this.label4.Text = "label4";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelIndex
            // 
            this.labelIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIndex.AutoSize = true;
            this.labelIndex.Location = new System.Drawing.Point(1070, 751);
            this.labelIndex.Name = "labelIndex";
            this.labelIndex.Size = new System.Drawing.Size(69, 16);
            this.labelIndex.TabIndex = 20;
            this.labelIndex.Text = "labelIndex";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(1275, 478);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(133, 24);
            this.comboBox1.TabIndex = 21;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1165, 486);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 16);
            this.label6.TabIndex = 22;
            this.label6.Text = "Тип изделия";
            // 
            // scan
            // 
            this.scan.Location = new System.Drawing.Point(1255, 685);
            this.scan.Name = "scan";
            this.scan.Size = new System.Drawing.Size(126, 57);
            this.scan.TabIndex = 23;
            this.scan.Text = "Сканирование";
            this.scan.UseVisualStyleBackColor = true;
            this.scan.Click += new System.EventHandler(this.scan_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1426, 795);
            this.Controls.Add(this.scan);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBox1);
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
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label6;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button scan;
    }
}
