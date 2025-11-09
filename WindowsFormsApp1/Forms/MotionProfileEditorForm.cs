using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Controllers;
using CrystalTable.Logic;

namespace CrystalTable.Forms
{
 /// <summary>
    /// Редактор профилей движения с визуальным интерфейсом
    /// </summary>
    public partial class MotionProfileEditorForm : Form
    {
        private readonly Form1 mainForm;
      private MotionProfileManager manager;
        private MotionProfile currentProfile;
     private bool isUpdating;

        // UI Controls
        private ListBox profilesList;
        private ProfileGraphControl graphControl;
        private Label statsLabel;
        
        private TextBox nameTextBox;
        private ComboBox typeComboBox;
        private NumericUpDown minDelayInput;
        private NumericUpDown maxDelayInput;
    private NumericUpDown accelInput;
        private NumericUpDown cruiseInput;
        private NumericUpDown decelInput;
        private NumericUpDown autoThresholdInput;
   
        private Button applyButton;
   private Button testXButton;
        private Button testYButton;
        private Button saveButton;
 private Button deleteButton;
 private Button exportButton;
        private Button importButton;
    private Button closeButton;
    
        private Label testResultLabel;

     public MotionProfileEditorForm(Form1 form)
        {
 mainForm = form ?? throw new ArgumentNullException(nameof(form));
            manager = MotionProfileManager.Instance;
      
  InitializeComponent();
            LoadProfiles();
        UpdateUI();
        }

        private void InitializeComponent()
        {
            // ===== ОСНОВНЫЕ НАСТРОЙКИ ФОРМЫ =====
        
            this.Text = "⚙️ Редактор профилей движения";
     this.Size = new Size(1000, 700);
 this.StartPosition = FormStartPosition.CenterParent;
 this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 600);
    this.MaximizeBox = true;
   this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.Font = new Font("Segoe UI", 9f);

    // ===== ГЛАВНЫЙ LAYOUT =====
    
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
  Dock = DockStyle.Fill,
         ColumnCount = 2,
          RowCount = 1,
Padding = new Padding(10)
          };
     mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ===== ЛЕВАЯ ПАНЕЛЬ: Список профилей =====
        
        Panel leftPanel = CreateLeftPanel();
    mainLayout.Controls.Add(leftPanel, 0, 0);

            // ===== ПРАВАЯ ПАНЕЛЬ: Редактор + График =====
     
            Panel rightPanel = CreateRightPanel();
            mainLayout.Controls.Add(rightPanel, 1, 0);

        this.Controls.Add(mainLayout);
        }

        // ===== ЛЕВАЯ ПАНЕЛЬ =====
        
 private Panel CreateLeftPanel()
        {
            Panel panel = new Panel
        {
            Dock = DockStyle.Fill,
      BackColor = Color.White,
         Padding = new Padding(5)
      };

       // Заголовок
            Label titleLabel = new Label
   {
    Text = "📋 Библиотека профилей",
           Dock = DockStyle.Top,
       Height = 30,
   Font = new Font("Segoe UI", 10f, FontStyle.Bold),
      TextAlign = ContentAlignment.MiddleLeft
          };

          // Список профилей
        profilesList = new ListBox
     {
  Dock = DockStyle.Fill,
             Font = new Font("Segoe UI", 9f),
       IntegralHeight = false,
      SelectionMode = SelectionMode.One
   };
            profilesList.SelectedIndexChanged += ProfilesList_SelectedIndexChanged;

   // Кнопки управления
 FlowLayoutPanel buttonsPanel = new FlowLayoutPanel
  {
           Dock = DockStyle.Bottom,
     Height = 120,
    FlowDirection = FlowDirection.TopDown,
   WrapContents = false,
        Padding = new Padding(5)
            };

 Button newButton = CreateButton("➕ Создать новый", (s, e) => CreateNewProfile());
            saveButton = CreateButton("💾 Сохранить", (s, e) => SaveCurrentProfile());
       deleteButton = CreateButton("🗑️ Удалить", (s, e) => DeleteCurrentProfile());
            
     Button resetButton = CreateButton("🔄 Сброс к умолчаниям", (s, e) => ResetToDefaults());

  buttonsPanel.Controls.Add(newButton);
        buttonsPanel.Controls.Add(saveButton);
        buttonsPanel.Controls.Add(deleteButton);
            buttonsPanel.Controls.Add(resetButton);

          panel.Controls.Add(profilesList);
   panel.Controls.Add(buttonsPanel);
            panel.Controls.Add(titleLabel);

    return panel;
        }

    // ===== ПРАВАЯ ПАНЕЛЬ =====
        
        private Panel CreateRightPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
          Padding = new Padding(5)
    };

     // Разделение на верх и низ
    SplitContainer splitContainer = new SplitContainer
         {
       Dock = DockStyle.Fill,
    Orientation = Orientation.Horizontal,
              SplitterDistance = 350,
          FixedPanel = FixedPanel.Panel1
  };

    // Верх: Параметры
  splitContainer.Panel1.Controls.Add(CreateParametersPanel());

            // Низ: График + Статистика
            splitContainer.Panel2.Controls.Add(CreateGraphPanel());

            panel.Controls.Add(splitContainer);

        return panel;
        }

        // ===== ПАНЕЛЬ ПАРАМЕТРОВ =====
      
        private Panel CreateParametersPanel()
        {
      Panel panel = new Panel
     {
         Dock = DockStyle.Fill,
     BackColor = Color.White,
           Padding = new Padding(10),
     AutoScroll = true
    };

       TableLayoutPanel layout = new TableLayoutPanel
            {
  Dock = DockStyle.Fill,
         ColumnCount = 2,
        AutoSize = true
            };
  layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

   int row = 0;

        // Заголовок
          Label titleLabel = new Label
            {
        Text = "📐 Параметры профиля",
     Font = new Font("Segoe UI", 11f, FontStyle.Bold),
         Dock = DockStyle.Top,
      Height = 35,
                TextAlign = ContentAlignment.MiddleLeft
            };
 panel.Controls.Add(titleLabel);

 // Название
            AddLabeledControl(layout, row++, "Название:", nameTextBox = new TextBox { Width = 250 });
       nameTextBox.TextChanged += (s, e) => UpdateProfileName();

    // Тип профиля
       typeComboBox = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            typeComboBox.Items.AddRange(new object[] { 
         "Трапециевидный", 
        "Треугольный", 
        "S-образный", 
  "Автоматический" 
 });
  typeComboBox.SelectedIndexChanged += (s, e) => UpdateProfileType();
            AddLabeledControl(layout, row++, "Тип:", typeComboBox);

            // Разделитель
    AddSeparator(layout, row++);

       // Скорости
            AddGroupTitle(layout, row++, "⚡ Скорости (мкс между шагами)");
    
      minDelayInput = CreateNumericInput(100, 5000, 200);
            minDelayInput.ValueChanged += (s, e) => UpdateGraph();
    AddLabeledControl(layout, row++, "Мин. пауза (крейсер):", minDelayInput);

   maxDelayInput = CreateNumericInput(100, 5000, 800);
            maxDelayInput.ValueChanged += (s, e) => UpdateGraph();
            AddLabeledControl(layout, row++, "Макс. пауза (старт):", maxDelayInput);

            // Разделитель
    AddSeparator(layout, row++);

     // Распределение фаз
     AddGroupTitle(layout, row++, "📊 Распределение фаз (%)");
          
 accelInput = CreateNumericInput(10, 50, 30);
            accelInput.ValueChanged += (s, e) => UpdatePercentages();
      AddLabeledControl(layout, row++, "Разгон (ACCEL):", accelInput);

cruiseInput = CreateNumericInput(0, 80, 40);
     cruiseInput.ValueChanged += (s, e) => UpdatePercentages();
     AddLabeledControl(layout, row++, "Крейсер (RUN):", cruiseInput);

 decelInput = CreateNumericInput(10, 50, 30);
  decelInput.ValueChanged += (s, e) => UpdatePercentages();
      AddLabeledControl(layout, row++, "Торможение (DECEL):", decelInput);

            // Порог Auto
        autoThresholdInput = CreateNumericInput(1, 1000, 20);
    AddLabeledControl(layout, row++, "Порог Auto (шагов):", autoThresholdInput);

     // Разделитель
          AddSeparator(layout, row++);

            // Кнопки действий
            FlowLayoutPanel actionsPanel = new FlowLayoutPanel
  {
       Dock = DockStyle.Bottom,
       Height = 80,
    FlowDirection = FlowDirection.LeftToRight,
      WrapContents = true,
    Padding = new Padding(5)
};

      applyButton = CreateButton("💾 Применить и отправить в Arduino", (s, e) => ApplyProfile(), 240);
            testXButton = CreateButton("🎯 Тест X", (s, e) => TestProfile(Axis.X), 100);
        testYButton = CreateButton("🎯 Тест Y", (s, e) => TestProfile(Axis.Y), 100);
      exportButton = CreateButton("📤 Экспорт", (s, e) => ExportProfile(), 100);
    importButton = CreateButton("📂 Импорт", (s, e) => ImportProfile(), 100);
closeButton = CreateButton("❌ Закрыть", (s, e) => this.Close(), 100);

      applyButton.BackColor = Color.FromArgb(76, 175, 80);
            applyButton.ForeColor = Color.White;
            applyButton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

         actionsPanel.Controls.Add(applyButton);
   actionsPanel.Controls.Add(testXButton);
          actionsPanel.Controls.Add(testYButton);
         actionsPanel.Controls.Add(exportButton);
actionsPanel.Controls.Add(importButton);
     actionsPanel.Controls.Add(closeButton);

       panel.Controls.Add(actionsPanel);
        panel.Controls.Add(layout);

          return panel;
        }

        // ===== ПАНЕЛЬ ГРАФИКА =====
        
        private Panel CreateGraphPanel()
        {
Panel panel = new Panel
            {
  Dock = DockStyle.Fill,
           BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Заголовок
            Label titleLabel = new Label
            {
              Text = "📈 Визуализация профиля",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
Dock = DockStyle.Top,
 Height = 30,
       TextAlign = ContentAlignment.MiddleLeft
        };

        // График
       graphControl = new ProfileGraphControl
          {
     Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 255)
 };

          // Статистика
        statsLabel = new Label
     {
          Dock = DockStyle.Bottom,
    Height = 60,
     Font = new Font("Consolas", 9f),
    TextAlign = ContentAlignment.TopLeft,
   Padding = new Padding(5),
                BackColor = Color.FromArgb(245, 245, 250)
    };

        // Результат теста
         testResultLabel = new Label
            {
     Dock = DockStyle.Bottom,
      Height = 30,
      Font = new Font("Segoe UI", 9f, FontStyle.Bold),
     TextAlign = ContentAlignment.MiddleCenter,
  BackColor = Color.Transparent
          };

            panel.Controls.Add(graphControl);
       panel.Controls.Add(statsLabel);
            panel.Controls.Add(testResultLabel);
            panel.Controls.Add(titleLabel);

            return panel;
 }

      // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ UI =====

        private void AddLabeledControl(TableLayoutPanel layout, int row, string labelText, Control control)
   {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
         
         Label label = new Label
    {
        Text = labelText,
                AutoSize = true,
             Margin = new Padding(5, 8, 5, 5),
    Anchor = AnchorStyles.Left
            };
      
     control.Margin = new Padding(5);
            control.Anchor = AnchorStyles.Left;

 layout.Controls.Add(label, 0, row);
     layout.Controls.Add(control, 1, row);
        }

     private void AddSeparator(TableLayoutPanel layout, int row)
        {
      layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
    Panel separator = new Panel
            {
         Height = 1,
    Dock = DockStyle.Top,
      BackColor = Color.LightGray
            };
      layout.SetColumnSpan(separator, 2);
            layout.Controls.Add(separator, 0, row);
        }

 private void AddGroupTitle(TableLayoutPanel layout, int row, string title)
        {
  layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
  Label label = new Label
    {
    Text = title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 150),
     AutoSize = true,
              Margin = new Padding(5, 10, 5, 5)
            };
          layout.SetColumnSpan(label, 2);
     layout.Controls.Add(label, 0, row);
        }

        private NumericUpDown CreateNumericInput(decimal min, decimal max, decimal value)
        {
        return new NumericUpDown
            {
                Minimum = min,
     Maximum = max,
        Value = value,
   Width = 100,
    DecimalPlaces = 0,
    TextAlign = HorizontalAlignment.Right
     };
        }

        private Button CreateButton(string text, EventHandler handler, int width = 230)
        {
            Button button = new Button
            {
      Text = text,
      Width = width,
                Height = 32,
      Margin = new Padding(3),
   FlatStyle = FlatStyle.System,
      UseVisualStyleBackColor = true
            };
            button.Click += handler;
            return button;
        }

        // ===== БИЗНЕС-ЛОГИКА =====

    private void LoadProfiles()
        {
            profilesList.Items.Clear();
            
     foreach (var profile in manager.Profiles)
         {
           profilesList.Items.Add(profile);
      }

  // Выбираем активный профиль
  if (manager.ActiveProfile != null)
    {
      int index = profilesList.Items.IndexOf(manager.ActiveProfile);
    if (index >= 0)
                {
      profilesList.SelectedIndex = index;
         }
          }
        }

  private void ProfilesList_SelectedIndexChanged(object sender, EventArgs e)
        {
       if (profilesList.SelectedItem is MotionProfile profile)
     {
            LoadProfile(profile);
     }
        }

        private void LoadProfile(MotionProfile profile)
     {
        if (profile == null) return;

      isUpdating = true;
      try
     {
              currentProfile = profile;

     nameTextBox.Text = profile.Name;
        typeComboBox.SelectedIndex = (int)profile.Type;
             minDelayInput.Value = profile.MinDelayUs;
      maxDelayInput.Value = profile.MaxDelayUs;
   accelInput.Value = profile.AccelPercent;
      cruiseInput.Value = profile.CruisePercent;
          decelInput.Value = profile.DecelPercent;
             autoThresholdInput.Value = profile.AutoThreshold;

    UpdateGraph();
    UpdateUI();
       }
    finally
            {
     isUpdating = false;
            }
     }

        private void UpdateProfileName()
  {
            if (isUpdating || currentProfile == null) return;
  currentProfile.Name = nameTextBox.Text;
            
  // Обновляем список
      int index = profilesList.SelectedIndex;
    profilesList.Items[index] = currentProfile;
        }

        private void UpdateProfileType()
        {
      if (isUpdating || currentProfile == null) return;
            currentProfile.Type = (ProfileType)typeComboBox.SelectedIndex;

       // Для треугольного профиля cruise = 0
    if (currentProfile.Type == ProfileType.Triangle)
      {
        cruiseInput.Value = 0;
      cruiseInput.Enabled = false;
         accelInput.Value = 50;
       decelInput.Value = 50;
    }
            else
 {
       cruiseInput.Enabled = true;
    }

         UpdateGraph();
        }

  private void UpdatePercentages()
 {
            if (isUpdating || currentProfile == null) return;

            int total = (int)(accelInput.Value + cruiseInput.Value + decelInput.Value);
            
   // Цветовая индикация
    if (total == 100)
        {
         accelInput.BackColor = Color.White;
        cruiseInput.BackColor = Color.White;
         decelInput.BackColor = Color.White;
   }
       else
            {
           accelInput.BackColor = Color.FromArgb(255, 220, 220);
  cruiseInput.BackColor = Color.FromArgb(255, 220, 220);
   decelInput.BackColor = Color.FromArgb(255, 220, 220);
       }

          UpdateGraph();
        }

        private void UpdateGraph()
        {
 if (isUpdating || currentProfile == null) return;

    // Обновляем параметры профиля
     currentProfile.MinDelayUs = (ushort)minDelayInput.Value;
            currentProfile.MaxDelayUs = (ushort)maxDelayInput.Value;
            currentProfile.AccelPercent = (byte)accelInput.Value;
            currentProfile.CruisePercent = (byte)cruiseInput.Value;
   currentProfile.DecelPercent = (byte)decelInput.Value;
 currentProfile.AutoThreshold = (uint)autoThresholdInput.Value;

          // Обновляем график
         graphControl.Profile = currentProfile;
  graphControl.Invalidate();

            // Обновляем статистику
            var stats = currentProfile.CalculateStats(1000); // Для 1000 шагов
  statsLabel.Text = 
          $"📊 Статистика (для 1000 шагов):\n" +
          $"  • Время: {stats.TotalTimeSec:F2} сек\n" +
    $"  • Макс. скорость: {stats.MaxSpeedStepsPerSec:F0} шаг/с\n" +
     $"  • Ускорение: {stats.MaxAcceleration:F0} шаг/с²";
        }

 private void UpdateUI()
        {
          bool hasProfile = currentProfile != null;
  bool isValid = hasProfile && currentProfile.IsValid();

     saveButton.Enabled = hasProfile;
    deleteButton.Enabled = hasProfile && !currentProfile.IsDefault;
   applyButton.Enabled = isValid;
      testXButton.Enabled = isValid;
    testYButton.Enabled = isValid;
          exportButton.Enabled = hasProfile;

          // Валидация
   if (hasProfile && !isValid)
   {
     var errors = currentProfile.Validate();
        testResultLabel.Text = "❌ Ошибки: " + string.Join(", ", errors);
    testResultLabel.ForeColor = Color.Red;
    }
            else
   {
   testResultLabel.Text = "";
   }
        }

    // ===== ДЕЙСТВИЯ =====

   private void CreateNewProfile()
      {
         var newProfile = new MotionProfile
        {
                Name = "Новый профиль " + DateTime.Now.ToString("HH:mm:ss"),
   Description = "Пользовательский профиль"
   };

        try
            {
          manager.AddProfile(newProfile);
     LoadProfiles();
    
         int index = profilesList.Items.IndexOf(newProfile);
        if (index >= 0)
           {
         profilesList.SelectedIndex = index;
            }

           MessageBox.Show("Профиль создан!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
       }
   catch (Exception ex)
        {
    MessageBox.Show($"Ошибка создания профиля:\n{ex.Message}", "Ошибка", 
          MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
     }

        private void SaveCurrentProfile()
        {
    if (currentProfile == null) return;

            try
       {
    manager.UpdateProfile(currentProfile);
     MessageBox.Show($"Профиль '{currentProfile.Name}' сохранён!", "Успех", 
        MessageBoxButtons.OK, MessageBoxIcon.Information);
 }
     catch (Exception ex)
            {
            MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", 
     MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }

    private void DeleteCurrentProfile()
        {
            if (currentProfile == null) return;

 var result = MessageBox.Show(
       $"Удалить профиль '{currentProfile.Name}'?",
      "Подтверждение",
    MessageBoxButtons.YesNo,
 MessageBoxIcon.Question);

  if (result == DialogResult.Yes)
     {
          try
         {
        manager.DeleteProfile(currentProfile.Id);
           LoadProfiles();
  MessageBox.Show("Профиль удалён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
      catch (Exception ex)
           {
                  MessageBox.Show($"Ошибка удаления:\n{ex.Message}", "Ошибка", 
     MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
            }
 }

  private async void ApplyProfile()
   {
  if (currentProfile == null || !currentProfile.IsValid()) return;

            try
            {
     // Сохраняем
    manager.UpdateProfile(currentProfile);

         // Делаем активным
             manager.SetActiveProfile(currentProfile);

   // Отправляем в Arduino
      bool success = await manager.ApplyProfileToArduino(mainForm.SerialPortController, currentProfile);

             if (success)
         {
    MessageBox.Show(
             $"Профиль '{currentProfile.Name}' применён и отправлен в Arduino!",
                    "Успех",
       MessageBoxButtons.OK,
       MessageBoxIcon.Information);
         }
  else
         {
        MessageBox.Show(
         "Профиль сохранён, но не удалось отправить в Arduino.\n" +
    "Проверьте подключение COM-порта.",
        "Частичный успех",
            MessageBoxButtons.OK,
         MessageBoxIcon.Warning);
     }
        }
            catch (Exception ex)
            {
     MessageBox.Show($"Ошибка применения профиля:\n{ex.Message}", "Ошибка", 
             MessageBoxButtons.OK, MessageBoxIcon.Error);
   }
        }

        private enum Axis { X, Y }

        private async void TestProfile(Axis axis)
        {
   if (currentProfile == null || !currentProfile.IsValid()) return;

        testResultLabel.Text = $"⏳ Тестирование {axis}...";
      testResultLabel.ForeColor = Color.Orange;
            testXButton.Enabled = false;
          testYButton.Enabled = false;

       try
            {
        // Временно применяем профиль
   await manager.ApplyProfileToArduino(mainForm.SerialPortController, currentProfile);

             // Запускаем таймер
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

         // Тестовое движение 1000 мкм
 byte command = axis == Axis.X ? Protocol.Commands.MoveRight : Protocol.Commands.MoveDown;
    bool success = await mainForm.SerialPortController.SendCommandAsync(command, 1000);

       stopwatch.Stop();

        if (success)
   {
     testResultLabel.Text = $"✅ Тест {axis}: {stopwatch.ElapsedMilliseconds} мс";
  testResultLabel.ForeColor = Color.Green;
    }
                else
                {
         testResultLabel.Text = $"❌ Тест {axis}: ошибка";
    testResultLabel.ForeColor = Color.Red;
    }
        }
            catch (Exception ex)
            {
             testResultLabel.Text = $"❌ Ошибка: {ex.Message}";
  testResultLabel.ForeColor = Color.Red;
            }
 finally
   {
   testXButton.Enabled = true;
            testYButton.Enabled = true;
 }
        }

  private void ExportProfile()
      {
            if (currentProfile == null) return;

      using (SaveFileDialog dialog = new SaveFileDialog())
            {
        dialog.Filter = "Профиль движения (*.profile)|*.profile|XML (*.xml)|*.xml";
              dialog.FileName = currentProfile.Name.Replace(" ", "_") + ".profile";

                if (dialog.ShowDialog() == DialogResult.OK)
    {
      try
    {
            manager.ExportProfile(currentProfile, dialog.FileName);
   MessageBox.Show("Профиль экспортирован!", "Успех", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
           catch (Exception ex)
          {
           MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка", 
     MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
       }
        }
        }

   private void ImportProfile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
       dialog.Filter = "Профиль движения (*.profile)|*.profile|XML (*.xml)|*.xml";

   if (dialog.ShowDialog() == DialogResult.OK)
  {
    try
           {
    var imported = manager.ImportProfile(dialog.FileName);
         LoadProfiles();
                
   int index = profilesList.Items.IndexOf(imported);
     if (index >= 0)
   {
    profilesList.SelectedIndex = index;
        }

          MessageBox.Show("Профиль импортирован!", "Успех", 
     MessageBoxButtons.OK, MessageBoxIcon.Information);
       }
  catch (Exception ex)
        {
  MessageBox.Show($"Ошибка импорта:\n{ex.Message}", "Ошибка", 
    MessageBoxButtons.OK, MessageBoxIcon.Error);
     }
        }
    }
        }

   private void ResetToDefaults()
        {
 var result = MessageBox.Show(
"Сбросить все профили к умолчаниям?\n\n" +
 "Все пользовательские профили будут удалены!",
    "Подтверждение",
MessageBoxButtons.YesNo,
     MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
          manager.ResetToDefaults();
             LoadProfiles();
         MessageBox.Show("Профили сброшены к умолчаниям!", "Успех", 
             MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
     }
    }

    // ===== КОНТРОЛ ГРАФИКА =====

    public class ProfileGraphControl : Control
    {
        private MotionProfile _profile;

      public MotionProfile Profile
      {
       get => _profile;
            set
{
     _profile = value;
Invalidate();
            }
        }

        public ProfileGraphControl()
        {
      DoubleBuffered = true;
            ResizeRedraw = true;
 MinimumSize = new Size(200, 150);
    BackColor = Color.FromArgb(250, 250, 255);
        }

    protected override void OnPaint(PaintEventArgs e)
 {
 base.OnPaint(e);

     if (_profile == null || Width < 100 || Height < 100)
  {
            DrawPlaceholder(e.Graphics);
              return;
            }

   // Проверка валидности профиля
     if (!_profile.IsValid())
       {
      using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
         e.Graphics.DrawString("⚠️ Профиль невалидный", 
              new Font("Segoe UI", 10f), Brushes.Red, 
   new RectangleF(0, 0, Width, Height), sf);
           }
                return;
            }

         DrawProfile(e.Graphics);
        }

    private void DrawPlaceholder(Graphics g)
        {
       using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
   {
    g.DrawString("Выберите профиль для визуализации", 
     new Font("Segoe UI", 10f), Brushes.Gray, 
        new RectangleF(0, 0, Width, Height), sf);
       }
        }

        private void DrawProfile(Graphics g)
{
     g.SmoothingMode = SmoothingMode.AntiAlias;
  g.Clear(BackColor);

     // Отступы
     int marginLeft = 60;
 int marginRight = 30;
  int marginTop = 30;
int marginBottom = 50;

  int graphWidth = Width - marginLeft - marginRight;
    int graphHeight = Height - marginTop - marginBottom;

            if (graphWidth < 10 || graphHeight < 10) return;

    // Оси
      using (Pen axisPen = new Pen(Color.Black, 2))
       {
     g.DrawLine(axisPen, marginLeft, marginTop + graphHeight, marginLeft + graphWidth, marginTop + graphHeight); // X
         g.DrawLine(axisPen, marginLeft, marginTop, marginLeft, marginTop + graphHeight); // Y
     }

       // Статистика для 1000 шагов
 var stats = _profile.CalculateStats(1000);
      
            AppLogger.Debug($"График: TotalSteps={stats.TotalSteps}, MaxSpeed={_profile.MaxSpeedStepsPerSec:F2}");

uint accelSteps = stats.AccelSteps;
  uint cruiseSteps = stats.CruiseSteps;
 uint decelSteps = stats.DecelSteps;
          uint totalSteps = stats.TotalSteps;

   if (totalSteps == 0)
{
      using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
          {
         g.DrawString("⚠️ TotalSteps = 0", 
           new Font("Segoe UI", 10f), Brushes.Red, 
         new RectangleF(0, 0, Width, Height), sf);
            }
                return;
            }

   float xScale = (float)graphWidth / totalSteps;
  float yScale = (float)graphHeight / Math.Max(_profile.MaxSpeedStepsPerSec, 1);

            // Фоновые зоны
  DrawZone(g, marginLeft, marginTop, accelSteps * xScale, graphHeight, Color.FromArgb(50, 76, 175, 80), "ACCEL");
      DrawZone(g, marginLeft + accelSteps * xScale, marginTop, cruiseSteps * xScale, graphHeight, Color.FromArgb(50, 33, 150, 243), "RUN");
 DrawZone(g, marginLeft + (accelSteps + cruiseSteps) * xScale, marginTop, decelSteps * xScale, graphHeight, Color.FromArgb(50, 255, 152, 0), "DECEL");

      // График профиля
   using (Pen profilePen = new Pen(Color.FromArgb(200, 33, 150, 243), 3))
    {
                PointF[] points = CalculateProfilePoints(marginLeft, marginTop, graphHeight, xScale, yScale, stats);
        AppLogger.Debug($"График: Точек={points.Length}");
         
          if (points.Length > 1)
   {
    g.DrawLines(profilePen, points);
             }
            }

            // Метки осей
      using (Font font = new Font("Arial", 8f))
            {
             g.DrawString("Скорость (шаг/с)", font, Brushes.Black, marginLeft - 55, marginTop - 5, 
        new StringFormat { FormatFlags = StringFormatFlags.DirectionVertical });
       g.DrawString("Время (шаги)", font, Brushes.Black, marginLeft + graphWidth / 2 - 30, marginTop + graphHeight + 30);
    
        // Значения Y
 g.DrawString(_profile.MaxSpeedStepsPerSec.ToString("F0"), font, Brushes.Black, marginLeft - 50, marginTop);
    g.DrawString("0", font, Brushes.Black, marginLeft - 20, marginTop + graphHeight - 10);
            }
        }

        private void DrawZone(Graphics g, float x, float y, float width, float height, Color color, string label)
    {
    if (width <= 0) return;

            using (Brush brush = new SolidBrush(color))
            {
           g.FillRectangle(brush, x, y, width, height);
          }

            using (Font font = new Font("Arial", 8f, FontStyle.Bold))
     using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
            {
          g.DrawString(label, font, Brushes.Gray, x + width / 2, y + 5, sf);
            }
  }

 private PointF[] CalculateProfilePoints(int marginLeft, int marginTop, int graphHeight, float xScale, float yScale, ProfileStatistics stats)
     {
            List<PointF> points = new List<PointF>();

          uint accelSteps = stats.AccelSteps;
         uint cruiseSteps = stats.CruiseSteps;
  uint decelSteps = stats.DecelSteps;

          float minSpeed = _profile.MinSpeedStepsPerSec;
      float maxSpeed = _profile.MaxSpeedStepsPerSec;

  // Старт
  points.Add(new PointF(marginLeft, marginTop + graphHeight - minSpeed * yScale));

       // Разгон
       for (uint i = 0; i <= accelSteps; i++)
  {
     float t = accelSteps > 0 ? (float)i / accelSteps : 0;
    float speed = minSpeed + (maxSpeed - minSpeed) * t;
   float x = marginLeft + i * xScale;
      float y = marginTop + graphHeight - speed * yScale;
points.Add(new PointF(x, y));
    }

            // Крейсер
       if (cruiseSteps > 0)
       {
            float x1 = marginLeft + accelSteps * xScale;
  float x2 = marginLeft + (accelSteps + cruiseSteps) * xScale;
    float y = marginTop + graphHeight - maxSpeed * yScale;
          points.Add(new PointF(x2, y));
    }

            // Торможение
for (uint i = 0; i <= decelSteps; i++)
     {
   float t = decelSteps > 0 ? (float)i / decelSteps : 0;
      float speed = maxSpeed - (maxSpeed - minSpeed) * t;
       float x = marginLeft + (accelSteps + cruiseSteps + i) * xScale;
  float y = marginTop + graphHeight - speed * yScale;
     points.Add(new PointF(x, y));
       }

      return points.ToArray();
        }
    }
}
