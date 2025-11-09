using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrystalTable.Logic;

namespace CrystalTable
{
    /// <summary>
    /// Форма для отображения статистики пластины
    /// </summary>
    public partial class StatisticsForm : Form
    {
        private Dictionary<string, object> statistics;
        private WaferStatistics waferStats;
        private HashSet<int> selectedCrystals;

        /// <summary>
        /// Конструктор формы статистики
        /// </summary>
        /// <param name="stats">Словарь со статистикой</param>
        /// <param name="waferStats">Объект статистики пластины</param>
        /// <param name="selectedCrystals">Выбранные кристаллы</param>
        public StatisticsForm(Dictionary<string, object> stats,
       WaferStatistics waferStats, HashSet<int> selectedCrystals)
  {
            this.statistics = stats;
            this.waferStats = waferStats;
            this.selectedCrystals = selectedCrystals;

         InitializeComponent();
     PopulateStatistics();
}

        /// <summary>
        /// Инициализация компонентов формы
     /// </summary>
        private void InitializeComponent()
 {
         // Настройки формы
        this.Text = "Статистика пластины";
    this.Size = new Size(600, 500);
      this.StartPosition = FormStartPosition.CenterParent;
   this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем TabControl для разных видов статистики
            TabControl tabControl = new TabControl();
     tabControl.Dock = DockStyle.Fill;

            // Вкладка общей статистики
     TabPage generalTab = new TabPage("Общая статистика");
            generalTab.Controls.Add(CreateGeneralStatisticsPanel());
          tabControl.TabPages.Add(generalTab);

            // Вкладка распределения
            TabPage distributionTab = new TabPage("Распределение");
       distributionTab.Controls.Add(CreateDistributionPanel());
       tabControl.TabPages.Add(distributionTab);

       // Вкладка выделенных кристаллов
            if (selectedCrystals != null && selectedCrystals.Count > 0)
            {
        TabPage selectionTab = new TabPage("Выделенные кристаллы");
     selectionTab.Controls.Add(CreateSelectionPanel());
            tabControl.TabPages.Add(selectionTab);
   }

            // Панель с кнопками
            Panel buttonPanel = new Panel();
   buttonPanel.Height = 50;
   buttonPanel.Dock = DockStyle.Bottom;

      Button btnExport = new Button();
            btnExport.Text = "Экспорт в файл";
     btnExport.Size = new Size(120, 30);
            btnExport.Location = new Point(10, 10);
     btnExport.Click += BtnExport_Click;
            buttonPanel.Controls.Add(btnExport);

          Button btnCopy = new Button();
 btnCopy.Text = "Копировать";
 btnCopy.Size = new Size(120, 30);
         btnCopy.Location = new Point(140, 10);
         btnCopy.Click += BtnCopy_Click;
   buttonPanel.Controls.Add(btnCopy);

       Button btnClose = new Button();
btnClose.Text = "Закрыть";
            btnClose.Size = new Size(120, 30);
     btnClose.Location = new Point(460, 10);
            btnClose.Click += (s, e) => this.Close();
    buttonPanel.Controls.Add(btnClose);

     // Добавляем контролы на форму
       this.Controls.Add(tabControl);
 this.Controls.Add(buttonPanel);
        }

   /// <summary>
        /// Создает панель общей статистики
  /// </summary>
        private Panel CreateGeneralStatisticsPanel()
    {
            Panel panel = new Panel();
panel.Dock = DockStyle.Fill;
       panel.AutoScroll = true;

       ListView listView = new ListView();
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Dock = DockStyle.Fill;

     // Колонки
            listView.Columns.Add("Параметр", 250);
            listView.Columns.Add("Значение", 300);

       // Добавляем статистику
       foreach (var kvp in statistics)
            {
    if (kvp.Value is string || kvp.Value is int || kvp.Value is float || kvp.Value is double)
      {
        ListViewItem item = new ListViewItem(kvp.Key);

      string value = kvp.Value.ToString();

      // Подсветка важных параметров
       if (kvp.Key.Contains("Общее количество") || 
     kvp.Key.Contains("Процент заполнения") ||
   kvp.Key.Contains("времени сканирования"))
     {
    item.Font = new Font(listView.Font, FontStyle.Bold);
             item.BackColor = Color.LightYellow;
      }

           item.SubItems.Add(value);
     listView.Items.Add(item);
   }
         }

            panel.Controls.Add(listView);
         return panel;
    }

        /// <summary>
        /// Создает панель распределения
        /// </summary>
        private Panel CreateDistributionPanel()
        {
Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;

// Разделяем панель на две части
 SplitContainer splitContainer = new SplitContainer();
       splitContainer.Dock = DockStyle.Fill;
     splitContainer.Orientation = Orientation.Horizontal;

  // Верхняя часть - распределение по рядам
            GroupBox rowGroup = new GroupBox();
   rowGroup.Text = "Распределение по рядам";
  rowGroup.Dock = DockStyle.Fill;

            ListView rowList = new ListView();
    rowList.View = View.Details;
        rowList.FullRowSelect = true;
    rowList.GridLines = true;
       rowList.Dock = DockStyle.Fill;

 rowList.Columns.Add("Ряд", 100);
     rowList.Columns.Add("Количество", 100);
            rowList.Columns.Add("Процент", 100);

            if (waferStats != null)
     {
      var rowDist = waferStats.GetRowDistribution();
                if (rowDist.Count > 0)
             {
 int total = rowDist.Values.Sum();
      foreach (var kvp in rowDist.OrderBy(x => x.Key))
  {
     ListViewItem item = new ListViewItem($"Ряд {kvp.Key}");
   item.SubItems.Add(kvp.Value.ToString());
             item.SubItems.Add($"{(kvp.Value * 100.0 / total):F1}%");
       rowList.Items.Add(item);
}
      }
        }

   rowGroup.Controls.Add(rowList);
       splitContainer.Panel1.Controls.Add(rowGroup);

  // Нижняя часть - распределение по зонам (радиальное)
            GroupBox zoneGroup = new GroupBox();
            zoneGroup.Text = "Радиальное распределение (от центра к краю)";
     zoneGroup.Dock = DockStyle.Fill;

ListView zoneList = new ListView();
   zoneList.View = View.Details;
 zoneList.FullRowSelect = true;
            zoneList.GridLines = true;
        zoneList.Dock = DockStyle.Fill;

         zoneList.Columns.Add("Зона", 150);
    zoneList.Columns.Add("Количество", 100);
    zoneList.Columns.Add("Процент", 100);

      if (waferStats != null)
     {
    var zoneDist = waferStats.GetRadialZoneDistribution(5);
                if (zoneDist.Count > 0)
        {
               int total = zoneDist.Values.Sum();
   string[] zoneNames = { "Центр", "Зона 2", "Зона 3", "Зона 4", "Край" };
        
            foreach (var kvp in zoneDist.OrderBy(x => x.Key))
       {
string zoneName = kvp.Key < zoneNames.Length ? zoneNames[kvp.Key] : $"Зона {kvp.Key + 1}";
        ListViewItem item = new ListViewItem(zoneName);
             item.SubItems.Add(kvp.Value.ToString());
  item.SubItems.Add($"{(kvp.Value * 100.0 / total):F1}%");

  // Подсветка краевой зоны
     if (kvp.Key == zoneDist.Count - 1)
       item.BackColor = Color.LightCoral;
        else if (kvp.Key == 0)
 item.BackColor = Color.LightGreen;

    zoneList.Items.Add(item);
         }
  }
 }

            zoneGroup.Controls.Add(zoneList);
       splitContainer.Panel2.Controls.Add(zoneGroup);

      panel.Controls.Add(splitContainer);
            return panel;
    }

        /// <summary>
        /// Создает панель статистики выделенных кристаллов
        /// </summary>
     private Panel CreateSelectionPanel()
    {
 Panel panel = new Panel();
panel.Dock = DockStyle.Fill;
   panel.AutoScroll = true;

     if (waferStats != null && selectedCrystals != null && selectedCrystals.Count > 0)
          {
 var selectionStats = waferStats.GetSelectionStatistics(selectedCrystals);

                ListView listView = new ListView();
         listView.View = View.Details;
      listView.FullRowSelect = true;
        listView.GridLines = true;
                listView.Dock = DockStyle.Fill;

           listView.Columns.Add("Параметр", 250);
    listView.Columns.Add("Значение", 300);

           foreach (var kvp in selectionStats)
        {
          ListViewItem item = new ListViewItem(kvp.Key);
  item.SubItems.Add(kvp.Value.ToString());

           if (kvp.Key == "Выбрано кристаллов")
        {
       item.Font = new Font(listView.Font, FontStyle.Bold);
             item.BackColor = Color.LightBlue;
         }

  listView.Items.Add(item);
      }

         panel.Controls.Add(listView);
            }
  else
{
                Label label = new Label();
         label.Text = "Нет выделенных кристаллов";
    label.Dock = DockStyle.Fill;
   label.TextAlign = ContentAlignment.MiddleCenter;
    panel.Controls.Add(label);
  }

      return panel;
 }

        /// <summary>
        /// Заполняет форму статистикой
        /// </summary>
     private void PopulateStatistics()
    {
            // Статистика заполняется при создании панелей
        }

        /// <summary>
        /// Обработчик кнопки экспорта
        /// </summary>
        private void BtnExport_Click(object sender, EventArgs e)
 {
        SaveFileDialog saveDialog = new SaveFileDialog();
    saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv";
         saveDialog.Title = "Экспорт статистики";
            saveDialog.FileName = $"Статистика_пластины_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
      try
          {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

  sb.AppendLine("=== СТАТИСТИКА ПЛАСТИНЫ ===");
         sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
     sb.AppendLine();

        // Общая статистика
     sb.AppendLine("ОБЩАЯ СТАТИСТИКА:");
            foreach (var kvp in statistics)
       {
        if (kvp.Value is string || kvp.Value is int || kvp.Value is float || kvp.Value is double)
              {
 sb.AppendLine($"{kvp.Key}: {kvp.Value}");
    }
       }

      System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(),
  System.Text.Encoding.UTF8);

          MessageBox.Show("Статистика успешно экспортирована!", "Успех",
             MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
                catch (Exception ex)
       {
 MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
       }
        }

        /// <summary>
        /// Обработчик кнопки копирования
        /// </summary>
        private void BtnCopy_Click(object sender, EventArgs e)
        {
       System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var kvp in statistics)
    {
       if (kvp.Value is string || kvp.Value is int || kvp.Value is float || kvp.Value is double)
       {
             sb.AppendLine($"{kvp.Key}: {kvp.Value}");
              }
      }

 Clipboard.SetText(sb.ToString());
          MessageBox.Show("Статистика скопирована в буфер обмена!", "Информация",
      MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
