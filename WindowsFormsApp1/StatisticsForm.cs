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
                    if (kvp.Value is float || kvp.Value is double)
                    {
                        double numValue = Convert.ToDouble(kvp.Value);
                        value = numValue.ToString("F2");

                        if (kvp.Key.Contains("Процент"))
                            value += " %";
                        else if (kvp.Key.Contains("мм"))
                            value += " мм";
                    }

                    item.SubItems.Add(value);

                    // Подсветка важных параметров
                    if (kvp.Key.Contains("Общее количество") || kvp.Key.Contains("Процент заполнения"))
                    {
                        item.Font = new Font(listView.Font, FontStyle.Bold);
                        item.BackColor = Color.LightYellow;
                    }

                    listView.Items.Add(item);
                }
                else if (kvp.Value is ValueTuple<float, float> tuple)
                {
                    ListViewItem item = new ListViewItem(kvp.Key);
                    item.SubItems.Add($"X: {tuple.Item1:F3}, Y: {tuple.Item2:F3}");
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

            // Верхняя часть - распределение по квадрантам
            GroupBox quadrantGroup = new GroupBox();
            quadrantGroup.Text = "Распределение по квадрантам";
            quadrantGroup.Dock = DockStyle.Fill;

            ListView quadrantList = new ListView();
            quadrantList.View = View.Details;
            quadrantList.FullRowSelect = true;
            quadrantList.GridLines = true;
            quadrantList.Dock = DockStyle.Fill;

            quadrantList.Columns.Add("Квадрант", 150);
            quadrantList.Columns.Add("Количество", 100);
            quadrantList.Columns.Add("Процент", 100);

            if (statistics.ContainsKey("Распределение по квадрантам"))
            {
                var quadrantDist = statistics["Распределение по квадрантам"] as Dictionary<string, int>;
                if (quadrantDist != null)
                {
                    int total = quadrantDist.Values.Sum();
                    foreach (var kvp in quadrantDist)
                    {
                        ListViewItem item = new ListViewItem(kvp.Key);
                        item.SubItems.Add(kvp.Value.ToString());
                        item.SubItems.Add($"{(kvp.Value * 100.0 / total):F1}%");
                        quadrantList.Items.Add(item);
                    }
                }
            }

            quadrantGroup.Controls.Add(quadrantList);
            splitContainer.Panel1.Controls.Add(quadrantGroup);

            // Нижняя часть - распределение край/центр
            GroupBox edgeGroup = new GroupBox();
            edgeGroup.Text = "Распределение край/центр";
            edgeGroup.Dock = DockStyle.Fill;

            ListView edgeList = new ListView();
            edgeList.View = View.Details;
            edgeList.FullRowSelect = true;
            edgeList.GridLines = true;
            edgeList.Dock = DockStyle.Fill;

            edgeList.Columns.Add("Область", 150);
            edgeList.Columns.Add("Количество", 100);
            edgeList.Columns.Add("Процент", 100);

            if (statistics.ContainsKey("Распределение край/центр"))
            {
                var edgeDist = statistics["Распределение край/центр"] as Dictionary<string, int>;
                if (edgeDist != null)
                {
                    int total = edgeDist.Values.Sum();
                    foreach (var kvp in edgeDist)
                    {
                        ListViewItem item = new ListViewItem(kvp.Key);
                        item.SubItems.Add(kvp.Value.ToString());
                        item.SubItems.Add($"{(kvp.Value * 100.0 / total):F1}%");

                        if (kvp.Key == "Край")
                            item.BackColor = Color.LightCoral;
                        else
                            item.BackColor = Color.LightGreen;

                        edgeList.Items.Add(item);
                    }
                }
            }

            edgeGroup.Controls.Add(edgeList);
            splitContainer.Panel2.Controls.Add(edgeGroup);

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