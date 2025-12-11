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
        private Panel detailedInfoPanel; // ✅ ДОБАВЛЕНО: Детальная информация
        
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
 this.Size = new Size(1300, 750); // ✅ Увеличена ширина для 3 колонок
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
  this.MinimumSize = new Size(1100, 650);
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);
         this.Font = new Font("Segoe UI", 9f);

      // ===== ГЛАВНЫЙ LAYOUT: 3 КОЛОНКИ =====
            
         TableLayoutPanel mainLayout = new TableLayoutPanel
            {
   Dock = DockStyle.Fill,
   ColumnCount = 3,
    RowCount = 1,
      Padding = new Padding(10)
        };
            // Колонка 1: Список профилей (250px)
       mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
 // Колонка 2: Параметры + График + Кнопки (гибкая, основная)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
          // Колонка 3: Детальная информация (гибкая, дополнительная)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // ===== ЛЕВАЯ ПАНЕЛЬ: Список профилей =====
            
            Panel leftPanel = CreateLeftPanel();
            mainLayout.Controls.Add(leftPanel, 0, 0);

       // ===== ЦЕНТАЛЬНАЯ ПАНЕЛЬ: Редактор + График =====
            
 Panel centerPanel = CreateCenterPanel();
            mainLayout.Controls.Add(centerPanel, 1, 0);

   // ===== ПРАВАЯ ПАНЕЛЬ: Детальная информация =====
      
        Panel rightPanel = CreateDetailedInfoPanel();
  mainLayout.Controls.Add(rightPanel, 2, 0);

            this.Controls.Add(mainLayout);
        }

 // ===== ЦЕНТРАЛЬНАЯ ПАНЕЛЬ (ВМЕСТО CreateRightPanel) =====
 
        private Panel CreateCenterPanel()
        {
  Panel panel = new Panel
       {
        Dock = DockStyle.Fill,
    Padding = new Padding(5)
       };

            // ✅ Трёхуровневый layout: 2 строки | График | Кнопки
    TableLayoutPanel verticalLayout = new TableLayoutPanel
  {
        Dock = DockStyle.Fill,
       RowCount = 3,
   ColumnCount = 1
 };
  
      // Строка 1: Параметры (160px - УМЕНЬШЕНА)
     verticalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        // Строка 2: График (остаток места - СЖАТ)
        verticalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        // Строка 3: Кнопки действий (70px - УМЕНЬШЕНА)
     verticalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

 // Параметры
      verticalLayout.Controls.Add(CreateParametersPanel(), 0, 0);

       // График
      verticalLayout.Controls.Add(CreateGraphPanel(), 0, 1);

    // Кнопки действий
          verticalLayout.Controls.Add(CreateActionsPanel(), 0, 2);

  panel.Controls.Add(verticalLayout);

    return panel;
}

     // ===== ПАНЕЛЬ ПАРАМЕТРОВ (МАКСИМАЛЬНО КОМПАКТНАЯ) =====
   
     private Panel CreateParametersPanel()
 {
   Panel panel = new Panel
    {
      Dock = DockStyle.Fill,
BackColor = Color.White,
Padding = new Padding(3), // ✅ Уменьшен отступ с 6 до 3
    AutoScroll = false
   };

    // Заголовок
 Label titleLabel = new Label
 {
          Text = "📐 Параметры профиля",
   Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
 Dock = DockStyle.Top,
    Height = 26,
    TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(2, 0, 0, 0) // ✅ Сдвиг влево
    };
 panel.Controls.Add(titleLabel);

  // ✅ МАКСИМАЛЬНО СЖАТЫЙ layout: узкие колонки
    TableLayoutPanel layout = new TableLayoutPanel
  {
 Dock = DockStyle.Fill,
    ColumnCount = 8,
     RowCount = 2,
        AutoSize = false,
 Padding = new Padding(2, 28, 2, 2) // ✅ Минимальные отступы, сдвиг вверх
       };

  // ✅ Колонки: Label очень узкие (60px), Control компактные (70px)
    for (int i = 0; i < 4; i++)
     {
layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // ✅ 70→60px (-10px)
      layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70)); // ✅ 80→70px (-10px)
      }

   int col = 0;

 // ===== СТРОКА 1 =====
 
   // Название (занимает 3 колонки = 60+70+60 = 190px)
 Label nameLabel = new Label
   {
      Text = "Название:",
  AutoSize = true,
  Margin = new Padding(0, 0, 1, 0), // ✅ Минимальный margin, только справа
    Anchor = AnchorStyles.Left,
    Font = new Font("Segoe UI", 8f) // ✅ Уменьшен шрифт с 8.5f до 8f
      };
  nameTextBox = new TextBox { Width = 130, Margin = new Padding(0) }; // ✅ 150→130px
    nameTextBox.TextChanged += (s, e) => UpdateProfileName();
   
   layout.Controls.Add(nameLabel, 0, 0);
      layout.Controls.Add(nameTextBox, 1, 0);
   layout.SetColumnSpan(nameTextBox, 3); // Занимает 3 ячейки

      col = 4;

   // Тип
  typeComboBox = new ComboBox { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList }; // ✅ 120→100px
 typeComboBox.Items.AddRange(new object[] { 
     "Трапец.", // ✅ Укорочено
       "Треуг.", // ✅ Укорочено
    "S-крив.", // ✅ Укорочено
   "Авто" 
  });
   typeComboBox.SelectedIndexChanged += (s, e) => UpdateProfileType();
   AddCompactControl(layout, 0, col, "Тип:", typeComboBox);
    col += 2;

  // Min delay
     minDelayInput = CreateNumericInput(100, 5000, 200);
    minDelayInput.ValueChanged += (s, e) => UpdateGraph();
   AddCompactControl(layout, 0, col, "Min dly:", minDelayInput);

  // ===== СТРОКА 2 =====
  
  col = 0;
  
     // Max delay
   maxDelayInput = CreateNumericInput(100, 5000, 800);
 maxDelayInput.ValueChanged += (s, e) => UpdateGraph();
   AddCompactControl(layout, 1, col, "Max dly:", maxDelayInput);
   col += 2;

  // Разгон %
  accelInput = CreateNumericInput(10, 50, 30);
         accelInput.ValueChanged += (s, e) => UpdatePercentages();
     AddCompactControl(layout, 1, col, "Разг %:", accelInput); // ✅ Убрана точка
     col += 2;

     // Крейсер %
cruiseInput = CreateNumericInput(0, 80, 40);
     cruiseInput.ValueChanged += (s, e) => UpdatePercentages();
   AddCompactControl(layout, 1, col, "Крейс %:", cruiseInput);
     col += 2;

  // Торможение %
  decelInput = CreateNumericInput(10, 50, 30);
 decelInput.ValueChanged += (s, e) => UpdatePercentages();
   AddCompactControl(layout, 1, col, "Торм %:", decelInput); // ✅ Укорочено "Тормож" → "Торм"
     col += 2;

       // ✅ ИСПРАВЛЕНО: Auto threshold добавлен в интерфейс (для типа Auto)
   autoThresholdInput = CreateNumericInput(1, 1000, 20);
   autoThresholdInput.ValueChanged += (s, e) => UpdateGraph();
   // Будет отображаться/скрываться в зависимости от типа профиля

 panel.Controls.Add(layout);

    return panel;
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

  Button newButton = CreateButton("➕ Создать новый", (s, e) => CreateNewProfile(), 230);
            saveButton = CreateButton("💾 Сохранить", (s, e) => SaveCurrentProfile(), 230);
   deleteButton = CreateButton("🗑️ Удалить", (s, e) => DeleteCurrentProfile(), 230);
        Button resetButton = CreateButton("🔄 Сброс к умолчаниям", (s, e) => ResetToDefaults(), 230);

       buttonsPanel.Controls.Add(newButton);
      buttonsPanel.Controls.Add(saveButton);
 buttonsPanel.Controls.Add(deleteButton);
       buttonsPanel.Controls.Add(resetButton);

      // ✅ ИСПРАВЛЕНО: Правильный порядок для Dock - сначала Bottom, потом Fill, потом Top
      // При Dock порядок Controls.Add обратный: последний Top будет сверху
      panel.Controls.Add(buttonsPanel);   // Bottom - добавляется первым
      panel.Controls.Add(profilesList);   // Fill - занимает оставшееся место
      panel.Controls.Add(titleLabel);     // Top - добавляется последним, будет сверху

       return panel;
        }

  // ===== ПАНЕЛЬ ГРАФИКА (МАКСИМАЛЬНО СЖАТАЯ) =====
        
  private Panel CreateGraphPanel()
   {
      Panel panel = new Panel
     {
    Dock = DockStyle.Fill,
  BackColor = Color.White,
  Padding = new Padding(6, 5, 6, 5) // ✅ Ещё меньше отступы
   };

 // Заголовок
 Label titleLabel = new Label
    {
   Text = "📈 Визуализация профиля",
    Font = new Font("Segoe UI", 9f, FontStyle.Bold), // ✅ Ещё меньше шрифт
Dock = DockStyle.Top,
    Height = 22, // ✅ Ещё меньше высота
       TextAlign = ContentAlignment.MiddleLeft
     };

  // График
         graphControl = new ProfileGraphControl
      {
  Dock = DockStyle.Fill,
     BackColor = Color.FromArgb(250, 250, 255),
     MinimumSize = new Size(200, 130) // ✅ СИЛЬНО сжат: 162 → 130px (-32px, -20%)
  };

    // Краткая статистика под графиком
   statsLabel = new Label
{
  Dock = DockStyle.Bottom,
     Height = 28, // ✅ Уменьшена высота с 30 до 28
     Font = new Font("Consolas", 8.5f), // ✅ УВЕЛИЧЕН шрифт с 7.5f до 8.5f
TextAlign = ContentAlignment.TopLeft,
   Padding = new Padding(3, 2, 3, 2), // ✅ Меньший отступ
  BackColor = Color.FromArgb(245, 245, 250)
  };

 panel.Controls.Add(graphControl);
panel.Controls.Add(statsLabel);
  panel.Controls.Add(titleLabel);

       return panel;
   }

 // ✅ ИСПРАВЛЕНО: Правая панель детальной информации - правильный порядок
    private Panel CreateDetailedInfoPanel()
{
   detailedInfoPanel = new Panel
   {
 Dock = DockStyle.Fill,
        BackColor = Color.White,
  Padding = new Padding(8, 5, 8, 5),
  AutoScroll = true,
     AutoScrollMinSize = new Size(0, 0)
      };

  // Заголовок
  Label titleLabel = new Label
         {
       Text = "📊 Детальная информация",
    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
    Dock = DockStyle.Top,
       Height = 24,
 TextAlign = ContentAlignment.MiddleLeft,
    Margin = new Padding(0, 0, 0, 5)
  };

     // Контент - TableLayoutPanel для структурированной информации
 TableLayoutPanel infoLayout = new TableLayoutPanel
     {
    Dock = DockStyle.Top,
    ColumnCount = 2,
        AutoSize = true,
   Padding = new Padding(3)
  };

   // Колонки: Label и Value
      infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
   infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

          // Будет заполняться в UpdateDetailedInfo()

  // ✅ КРИТИЧЕСКИ ВАЖНО: Правильный порядок добавления
 // При Dock=Top порядок Controls.Add определяет порядок отображения!
 detailedInfoPanel.Controls.Add(infoLayout);  // ← Сначала layout (он будет ниже)
       detailedInfoPanel.Controls.Add(titleLabel);   // ← Потом заголовок (он будет выше)

 return detailedInfoPanel;
        }

    // ✅ НОВЫЙ: Обновление детальной информации (2-колоночный формат)
        private void UpdateDetailedInfo()
        {
       if (currentProfile == null || detailedInfoPanel == null) return;

// Находим layout
       var infoLayout = detailedInfoPanel.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
            if (infoLayout == null) return;

     infoLayout.Controls.Clear();
            infoLayout.RowStyles.Clear();

         // Статистика для разных дистанций
            var stats100 = currentProfile.CalculateStats(100);
            var stats1000 = currentProfile.CalculateStats(1000);
            var stats10000 = currentProfile.CalculateStats(10000);

            int row = 0;

   // === ОБЩАЯ ИНФОРМАЦИЯ ===
         AddInfoGroupTitle(infoLayout, row++, "🎯 Общая информация");
       AddInfoRow2Col(infoLayout, row++, "Тип профиля:", currentProfile.Type.ToString());
  AddInfoRow2Col(infoLayout, row++, "Валидность:", currentProfile.IsValid() ? "✅ OK" : "❌ Ошибки");
       AddInfoRow2Col(infoLayout, row++, "По умолчанию:", currentProfile.IsDefault ? "Да" : "Нет");
   if (!string.IsNullOrEmpty(currentProfile.Description))
          {
        AddInfoRow2Col(infoLayout, row++, "Описание:", currentProfile.Description);
            }

            // === ПАРАМЕТРЫ СКОРОСТИ ===
       AddInfoGroupTitle(infoLayout, row++, "⚡ Параметры скорости");
     AddInfoRow2Col(infoLayout, row++, "Min delay:", $"{currentProfile.MinDelayUs} мкс");
            AddInfoRow2Col(infoLayout, row++, "Max delay:", $"{currentProfile.MaxDelayUs} мкс");
    AddInfoRow2Col(infoLayout, row++, "Max скорость:", $"{currentProfile.MaxSpeedStepsPerSec:F0} шаг/с");
      AddInfoRow2Col(infoLayout, row++, "Min скорость:", $"{currentProfile.MinSpeedStepsPerSec:F0} шаг/с");
            AddInfoRow2Col(infoLayout, row++, "Диапазон:", $"×{(currentProfile.MaxSpeedStepsPerSec / Math.Max(currentProfile.MinSpeedStepsPerSec, 1)):F1}");
        AddInfoRow2Col(infoLayout, row++, "Ускорение:", $"{currentProfile.MaxAccelerationStepsPerSec2:F0} шаг/с²");

            // === РАСПРЕДЕЛЕНИЕ ФАЗ ===
          AddInfoGroupTitle(infoLayout, row++, "📊 Распределение фаз");
    AddInfoRow2Col(infoLayout, row++, "ACCEL:", $"{currentProfile.AccelPercent}%");
            AddInfoRow2Col(infoLayout, row++, "CRUISE:", $"{currentProfile.CruisePercent}%");
            AddInfoRow2Col(infoLayout, row++, "DECEL:", $"{currentProfile.DecelPercent}%");
       AddInfoRow2Col(infoLayout, row++, "Сумма:", $"{currentProfile.AccelPercent + currentProfile.CruisePercent + currentProfile.DecelPercent}%");
          AddInfoRow2Col(infoLayout, row++, "Auto порог:", $"{currentProfile.AutoThreshold} шаг.");

        // === СТАТИСТИКА ДЛЯ 100 ШАГОВ ===
    AddInfoGroupTitle(infoLayout, row++, "📏 Для 100 шагов");
            AddInfoRow2Col(infoLayout, row++, "Время:", $"{stats100.TotalTimeSec:F3} сек");
        AddInfoRow2Col(infoLayout, row++, "Макс. скор.:", $"{stats100.MaxSpeedStepsPerSec:F0} ш/с");
         AddInfoRow2Col(infoLayout, row++, "ACCEL:", $"{stats100.AccelSteps} шаг");
            AddInfoRow2Col(infoLayout, row++, "RUN:", $"{stats100.CruiseSteps} шаг");
            AddInfoRow2Col(infoLayout, row++, "DECEL:", $"{stats100.DecelSteps} шаг");
       AddInfoRow2Col(infoLayout, row++, "Всего:", $"{stats100.TotalSteps} шаг");

  // === СТАТИСТИКА ДЛЯ 1000 ШАГОВ ===
    AddInfoGroupTitle(infoLayout, row++, "📐 Для 1000 шагов");
            AddInfoRow2Col(infoLayout, row++, "Время:", $"{stats1000.TotalTimeSec:F3} сек");
            AddInfoRow2Col(infoLayout, row++, "Макс. скор.:", $"{stats1000.MaxSpeedStepsPerSec:F0} ш/с");
     AddInfoRow2Col(infoLayout, row++, "ACCEL:", $"{stats1000.AccelSteps} шаг");
            AddInfoRow2Col(infoLayout, row++, "RUN:", $"{stats1000.CruiseSteps} шаг");
  AddInfoRow2Col(infoLayout, row++, "DECEL:", $"{stats1000.DecelSteps} шаг");
AddInfoRow2Col(infoLayout, row++, "Всего:", $"{stats1000.TotalSteps} шаг");

            // === СТАТИСТИКА ДЛЯ 10000 ШАГОВ ===
     AddInfoGroupTitle(infoLayout, row++, "📊 Для 10000 шагов");
   AddInfoRow2Col(infoLayout, row++, "Время:", $"{stats10000.TotalTimeSec:F3} сек");
            AddInfoRow2Col(infoLayout, row++, "Макс. скор.:", $"{stats10000.MaxSpeedStepsPerSec:F0} ш/с");
            AddInfoRow2Col(infoLayout, row++, "ACCEL:", $"{stats10000.AccelSteps} шаг");
     AddInfoRow2Col(infoLayout, row++, "RUN:", $"{stats10000.CruiseSteps} шаг");
  AddInfoRow2Col(infoLayout, row++, "DECEL:", $"{stats10000.DecelSteps} шаг");
            AddInfoRow2Col(infoLayout, row++, "Всего:", $"{stats10000.TotalSteps} шаг");

            // === СРАВНИТЕЛЬНАЯ ОЦЕНКА ===
            AddInfoGroupTitle(infoLayout, row++, "⚖️ Сравнительная оценка");
   AddInfoRow2Col(infoLayout, row++, "Эффективность:", GetEfficiencyRating(currentProfile));
 AddInfoRow2Col(infoLayout, row++, "Плавность:", GetSmoothnessRating(currentProfile));
 AddInfoRow2Col(infoLayout, row++, "Применимость:", GetApplicabilityRating(currentProfile));
 AddInfoRow2Col(infoLayout, row++, "Рекомендация:", GetRecommendation(currentProfile));
        }

        // ✅ Вспомогательные методы для 2-колоночного формата
        private void AddInfoGroupTitle(TableLayoutPanel layout, int row, string title)
        {
layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
   
          Label label = new Label
    {
  Text = title,
    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), // ✅ Уменьшен шрифт с 9f до 8.5f
    ForeColor = Color.FromArgb(50, 50, 150),
    AutoSize = true,
    Margin = new Padding(3, 8, 3, 2), // ✅ Уменьшены отступы
    BackColor = Color.FromArgb(240, 240, 255),
     Padding = new Padding(4, 2, 4, 2) // ✅ Уменьшен padding
     };
    layout.SetColumnSpan(label, 2);
      layout.Controls.Add(label, 0, row);
        }

        private void AddInfoRow2Col(TableLayoutPanel layout, int row, string label, string value)
 {
  layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lblControl = new Label
         {
         Text = label,
   AutoSize = true,
  Margin = new Padding(3, 2, 2, 2), // ✅ Уменьшены отступы с 5,3,2,3 до 3,2,2,2
     Font = new Font("Segoe UI", 8f), // ✅ Уменьшен шрифт с 8.5f до 8f
    ForeColor = Color.Gray
     };
    Label valControl = new Label
       {
          Text = value,
   AutoSize = true,
      Margin = new Padding(2, 2, 3, 2), // ✅ Уменьшены отступы
    Font = new Font("Segoe UI", 8f, FontStyle.Bold) // ✅ Уменьшен шрифт
     };
  layout.Controls.Add(lblControl, 0, row);
   layout.Controls.Add(valControl, 1, row);
      }

        // ✅ Компактное добавление контролов
   private void AddCompactControl(TableLayoutPanel layout, int row, int col, string labelText, Control control)
        {
  if (layout.RowStyles.Count <= row)
{
    layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
  }
     
 Label label = new Label
         {
      Text = labelText,
    AutoSize = true,
   Margin = new Padding(0, 0, 1, 0), // ✅ Минимальный margin, только справа
   Anchor = AnchorStyles.Left,
      Font = new Font("Segoe UI", 8f) // ✅ Уменьшен шрифт с 8.5f до 8f
  };
  
     control.Margin = new Padding(0); // ✅ Полностью убран margin
  control.Anchor = AnchorStyles.Left;

     layout.Controls.Add(label, col, row);
  layout.Controls.Add(control, col + 1, row);
        }

        // ✅ Оценочные методы
        private string GetEfficiencyRating(MotionProfile profile)
        {
 var stats = profile.CalculateStats(1000);
    float efficiency = stats.CruiseSteps / (float)Math.Max(stats.TotalSteps, 1);
  
      if (efficiency > 0.5f) return "⭐⭐⭐ Высокая";
  if (efficiency > 0.3f) return "⭐⭐ Средняя";
  return "⭐ Низкая";
        }

        private string GetSmoothnessRating(MotionProfile profile)
     {
     float accelRatio = profile.AccelPercent / 100f;
          
 if (accelRatio >= 0.3f && accelRatio <= 0.4f) return "⭐⭐⭐ Отличная";
  if (accelRatio >= 0.2f && accelRatio <= 0.5f) return "⭐⭐ Хорошая";
    return "⭐ Приемлемая";
 }

  private string GetApplicabilityRating(MotionProfile profile)
  {
       switch (profile.Type)
  {
    case ProfileType.Trapezoid:
 return "🎯 Универсальный";
       case ProfileType.Triangle:
return "⚡ Короткие дистанции";
    case ProfileType.SCurve:
     return "🎨 Высокоточные операции";
         case ProfileType.Auto:
 return "🤖 Адаптивный";
default:
   return "—";
 }
   }

   private string GetRecommendation(MotionProfile profile)
        {
      var stats = profile.CalculateStats(1000);
     
     if (stats.TotalTimeSec < 0.5f) return "✅ Быстрый режим";
        if (stats.TotalTimeSec < 1.5f) return "✅ Сбалансированный";
      return "⚠️ Медленный режим";
 }

   // ✅ НОВЫЙ: Панель кнопок действий
  private Panel CreateActionsPanel()
   {
        Panel panel = new Panel
 {
        Dock = DockStyle.Fill,
     BackColor = Color.White,
    Padding = new Padding(8, 5, 8, 5)
   };

   FlowLayoutPanel actionsPanel = new FlowLayoutPanel
 {
    Dock = DockStyle.Fill,
FlowDirection = FlowDirection.LeftToRight,
     WrapContents = true,
     Padding = new Padding(3)
      };

       applyButton = CreateButton("💾 Применить и отправить в Arduino", (s, e) => ApplyProfile(), 260);
 testXButton = CreateButton("🎯 Тест X", (s, e) => TestProfile(Axis.X), 100);
     testYButton = CreateButton("🎯 Тест Y", (s, e) => TestProfile(Axis.Y), 100);
 exportButton = CreateButton("📤 Экспорт", (s, e) => ExportProfile(), 100);
    importButton = CreateButton("📂 Импорт", (s, e) => ImportProfile(), 100);
       closeButton = CreateButton("❌ Закрыть", (s, e) => this.Close(), 100);

       applyButton.BackColor = Color.FromArgb(76, 175, 80);
    applyButton.ForeColor = Color.White;
applyButton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

    // Результат теста
      testResultLabel = new Label
    {
    AutoSize = true,
          Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
  TextAlign = ContentAlignment.MiddleLeft,
  Margin = new Padding(8, 6, 5, 3)
   };

   actionsPanel.Controls.Add(applyButton);
   actionsPanel.Controls.Add(testXButton);
    actionsPanel.Controls.Add(testYButton);
   actionsPanel.Controls.Add(exportButton);
       actionsPanel.Controls.Add(importButton);
     actionsPanel.Controls.Add(closeButton);
       actionsPanel.Controls.Add(testResultLabel);

  panel.Controls.Add(actionsPanel);

        return panel;
}

 private NumericUpDown CreateNumericInput(decimal min, decimal max, decimal value)
  {
   return new NumericUpDown
     {
                Minimum = min,
    Maximum = max,
Value = value,
      Width = 70, // ✅ Уменьшена ширина с 80 до 70 (-10px)
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
   Height = 28,
      Margin = new Padding(2),
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
            else if (profilesList.Items.Count > 0)
      {
        // Если нет активного профиля, выбираем первый
     profilesList.SelectedIndex = 0;
            }
      
          // ✅ ИСПРАВЛЕНО: Принудительное обновление графика после загрузки
      if (profilesList.SelectedItem is MotionProfile selectedProfile)
            {
            LoadProfile(selectedProfile);
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
  
                // ✅ ИСПРАВЛЕНО: Безопасная установка AutoThreshold
  if (autoThresholdInput != null)
  {
         autoThresholdInput.Value = profile.AutoThreshold;
  }
     }
   finally
       {
     isUpdating = false;
        }
 
      // ✅ ИСПРАВЛЕНО: UpdateGraph вызывается ПОСЛЕ сброса isUpdating
   UpdateGraph();
        UpdateDetailedInfo(); // ✅ Обновляем детальную информацию
 UpdateUI();
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

        // ✅ ДОБАВЛЕНО: Показываем AutoThreshold только для типа Auto
        // (поле всегда существует, но функционал активен только для Auto)

     UpdateGraph();
        UpdateDetailedInfo(); // ✅ Обновляем информацию
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
      UpdateDetailedInfo(); // ✅ Обновляем информацию
     }

        private void UpdateGraph()
    {
    if (isUpdating)
   {
           AppLogger.Debug("UpdateGraph пропущен: isUpdating=true");
   return;
            }
     
   if (currentProfile == null)
{
  AppLogger.Debug("UpdateGraph пропущен: currentProfile=null");
          return;
    }

    // Обновляем параметры профиля
   currentProfile.MinDelayUs = (ushort)minDelayInput.Value;
     currentProfile.MaxDelayUs = (ushort)maxDelayInput.Value;
      currentProfile.AccelPercent = (byte)accelInput.Value;
currentProfile.CruisePercent = (byte)cruiseInput.Value;
         currentProfile.DecelPercent = (byte)decelInput.Value;
  
            // ✅ ИСПРАВЛЕНО: Безопасное обновление AutoThreshold
 if (autoThresholdInput != null)
            {
          currentProfile.AutoThreshold = (uint)autoThresholdInput.Value;
 }

   // Обновляем график
         AppLogger.Debug($"UpdateGraph: Установка профиля '{currentProfile.Name}' в graphControl");
   graphControl.Profile = currentProfile;
 graphControl.Invalidate();

   // Обновляем краткую статистику
         var stats = currentProfile.CalculateStats(1000); // Для 1000 шагов
     statsLabel.Text = 
       $"📊 Краткая статистика (1000 шагов): Время={stats.TotalTimeSec:F2}с | " +
   $"Макс.скор.={stats.MaxSpeedStepsPerSec:F0}ш/с | Ускор.={stats.MaxAcceleration:F0}ш/с²";
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
            // Сначала применяем профиль и ЖДЁМ подтверждения
            bool profileApplied = await manager.ApplyProfileToArduino(mainForm.SerialPortController, currentProfile);
            
            if (!profileApplied)
            {
                testResultLabel.Text = $"❌ Не удалось применить профиль";
                testResultLabel.ForeColor = Color.Red;
                return;
            }
            
            // Небольшая задержка для гарантии применения профиля на Arduino
            await System.Threading.Tasks.Task.Delay(100);

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
