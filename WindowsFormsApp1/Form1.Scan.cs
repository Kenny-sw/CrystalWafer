using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CrystalTable.Controllers;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable
{
    public partial class Form1
    {
        // ✅ Контроллер автообхода
        private ScanController scanController;
        
        // ✅ Вкладка автообхода
        private TabPage tabPageScan;
        
        // ✅ UI элементы панели автообхода
        private Panel scanPanel;
        private ComboBox cmbScanPattern;
        private ComboBox cmbStartCorner;
        private NumericUpDown numDwellTime;
        private CheckBox chkSkipInspected;
        private CheckBox chkAutoMarkGood;
        private Button btnScanStart;
        private Button btnScanPause;
        private Button btnScanStop;
        private Button btnScanNext;
        private Button btnScanPrev;
        private Label lblScanStatus;
        private Label lblScanProgress;
        private ProgressBar progressBarScan;
        
        // ✅ Прогресс-бар в StatusStrip
        private ToolStripProgressBar statusProgressBar;
        private ToolStripStatusLabel statusProgressLabel;

        /// <summary>
        /// Инициализация UI автообхода
        /// </summary>
        private void InitializeScanUI()
        {
            try
            {
                // Проверяем наличие TabControl
                if (rightTabControl == null)
                {
                    AppLogger.Warning("InitializeScanUI: rightTabControl не найден!");
                    return;
                }

                // Создаём контроллер
                scanController = new ScanController(this);
                scanController.ProgressChanged += ScanController_ProgressChanged;
                scanController.CrystalReached += ScanController_CrystalReached;
                scanController.ScanCompleted += ScanController_ScanCompleted;
                scanController.StateChanged += ScanController_StateChanged;

                // Создаём отдельную вкладку для автообхода
                CreateScanTab();
                
                // Добавляем прогресс-бар в StatusStrip
                CreateStatusProgressBar();
                
                UpdateScanUI();
                
                AppLogger.Info($"InitializeScanUI: вкладка Автообход создана. Всего вкладок: {rightTabControl.TabPages.Count}");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка InitializeScanUI", ex);
                System.Diagnostics.Debug.WriteLine($"InitializeScanUI ERROR: {ex}");
            }
        }

        /// <summary>
        /// Создание вкладки автообхода
        /// </summary>
        private void CreateScanTab()
        {
            // Создаём новую вкладку
            tabPageScan = new TabPage
            {
                Text = "Автообход",
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(8)
            };

            // Вставляем после вкладки "Карта" или в конец
            if (tabPageMap != null && rightTabControl.TabPages.Contains(tabPageMap))
            {
                int insertIndex = rightTabControl.TabPages.IndexOf(tabPageMap) + 1;
                rightTabControl.TabPages.Insert(insertIndex, tabPageScan);
            }
            else
            {
                // Если tabPageMap не найден - добавляем в конец
                rightTabControl.TabPages.Add(tabPageScan);
            }

            // Создаём панель на этой вкладке
            CreateScanPanel();
        }

        /// <summary>
        /// Создание панели управления автообходом
        /// </summary>
        private void CreateScanPanel()
        {
            scanPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            // Заголовок
            var titleLabel = new Label
            {
                Text = "🔄 Автообход кристаллов",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 30
            };

            // Панель настроек
            var settingsPanel = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 3,
                Dock = DockStyle.Top,
                Height = 90,
                AutoSize = false
            };
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            // Паттерн обхода
            var lblPattern = new Label { Text = "Паттерн:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            cmbScanPattern = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120
            };
            cmbScanPattern.Items.AddRange(new object[] { "Змейка", "Построчно", "Выделенные" });
            cmbScanPattern.SelectedIndex = 0;

            // Угол старта
            var lblCorner = new Label { Text = "Старт:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            cmbStartCorner = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120
            };
            cmbStartCorner.Items.AddRange(new object[] { "Левый верх", "Правый верх", "Левый низ", "Правый низ" });
            cmbStartCorner.SelectedIndex = 0;

            // Время паузы
            var lblDwell = new Label { Text = "Пауза (мс):", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            numDwellTime = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10000,
                Value = 500,
                Increment = 100,
                Width = 80
            };

            // Чекбоксы
            chkSkipInspected = new CheckBox
            {
                Text = "Пропускать проверенные",
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 0)
            };

            chkAutoMarkGood = new CheckBox
            {
                Text = "Авто-пометка 'Годен'",
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 0)
            };

            settingsPanel.Controls.Add(lblPattern, 0, 0);
            settingsPanel.Controls.Add(cmbScanPattern, 1, 0);
            settingsPanel.Controls.Add(lblCorner, 2, 0);
            settingsPanel.Controls.Add(cmbStartCorner, 3, 0);
            settingsPanel.Controls.Add(lblDwell, 0, 1);
            settingsPanel.Controls.Add(numDwellTime, 1, 1);
            settingsPanel.Controls.Add(chkSkipInspected, 2, 1);
            settingsPanel.Controls.Add(chkAutoMarkGood, 3, 1);

            // Панель кнопок управления
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 5)
            };

            btnScanStart = CreateScanButton("▶ Старт", Color.FromArgb(46, 204, 113), BtnScanStart_Click);
            btnScanPause = CreateScanButton("⏸ Пауза", Color.FromArgb(241, 196, 15), BtnScanPause_Click);
            btnScanStop = CreateScanButton("⏹ Стоп", Color.FromArgb(231, 76, 60), BtnScanStop_Click);
            btnScanPrev = CreateScanButton("◀ Пред", Color.FromArgb(149, 165, 166), BtnScanPrev_Click);
            btnScanNext = CreateScanButton("След ▶", Color.FromArgb(149, 165, 166), BtnScanNext_Click);

            buttonsPanel.Controls.Add(btnScanStart);
            buttonsPanel.Controls.Add(btnScanPause);
            buttonsPanel.Controls.Add(btnScanStop);
            buttonsPanel.Controls.Add(btnScanPrev);
            buttonsPanel.Controls.Add(btnScanNext);

            // Прогресс
            var progressPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            lblScanStatus = new Label
            {
                Text = "Готов к обходу",
                AutoSize = true,
                Location = new Point(0, 5),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            lblScanProgress = new Label
            {
                Text = "0 / 0",
                AutoSize = true,
                Location = new Point(200, 5),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            progressBarScan = new ProgressBar
            {
                Location = new Point(0, 28),
                Width = 350,
                Height = 18,
                Style = ProgressBarStyle.Continuous
            };

            progressPanel.Controls.Add(lblScanStatus);
            progressPanel.Controls.Add(lblScanProgress);
            progressPanel.Controls.Add(progressBarScan);

            // Собираем панель (в обратном порядке для Dock)
            scanPanel.Controls.Add(progressPanel);
            scanPanel.Controls.Add(buttonsPanel);
            scanPanel.Controls.Add(settingsPanel);
            scanPanel.Controls.Add(titleLabel);

            // Добавляем на вкладку "Автообход"
            tabPageScan.Controls.Add(scanPanel);
        }

        private Button CreateScanButton(string text, Color backColor, EventHandler handler)
        {
            var btn = new Button
            {
                Text = text,
                Width = 70,
                Height = 28,
                Margin = new Padding(0, 0, 5, 0),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += handler;
            return btn;
        }

        /// <summary>
        /// Создание прогресс-бара в StatusStrip
        /// </summary>
        private void CreateStatusProgressBar()
        {
            statusProgressLabel = new ToolStripStatusLabel
            {
                Text = "",
                Visible = false
            };

            statusProgressBar = new ToolStripProgressBar
            {
                Width = 100,
                Visible = false,
                Style = ProgressBarStyle.Continuous
            };

            statusStrip1.Items.Add(new ToolStripSeparator());
            statusStrip1.Items.Add(statusProgressLabel);
            statusStrip1.Items.Add(statusProgressBar);
        }

        /// <summary>
        /// Обновить состояние UI автообхода
        /// </summary>
        private void UpdateScanUI()
        {
            if (scanController == null) return;

            var state = scanController.State;
            bool hasMap = CrystalManager.Instance.Crystals.Count > 0;

            // Кнопки
            btnScanStart.Enabled = hasMap && (state == ScanState.Idle || state == ScanState.Paused || state == ScanState.Completed);
            btnScanPause.Enabled = state == ScanState.Running;
            btnScanStop.Enabled = state == ScanState.Running || state == ScanState.Paused;
            btnScanPrev.Enabled = state != ScanState.Idle && scanController.CurrentIndex > 0;
            btnScanNext.Enabled = state != ScanState.Idle && scanController.CurrentIndex < scanController.TotalCount - 1;

            // Настройки (только когда не активен)
            bool canEdit = state == ScanState.Idle || state == ScanState.Completed;
            cmbScanPattern.Enabled = canEdit;
            cmbStartCorner.Enabled = canEdit;
            numDwellTime.Enabled = canEdit;
            chkSkipInspected.Enabled = canEdit;
            chkAutoMarkGood.Enabled = canEdit;

            // Текст кнопки старт
            btnScanStart.Text = state == ScanState.Paused ? "▶ Продолжить" : "▶ Старт";

            // Прогресс-бар в статусе
            bool showProgress = state == ScanState.Running || state == ScanState.Paused;
            statusProgressBar.Visible = showProgress;
            statusProgressLabel.Visible = showProgress;
        }

        /// <summary>
        /// Применить настройки из UI в контроллер
        /// </summary>
        private void ApplyScanSettings()
        {
            scanController.Settings.Pattern = (ScanPattern)cmbScanPattern.SelectedIndex;
            scanController.Settings.StartFrom = (StartCorner)cmbStartCorner.SelectedIndex;
            scanController.Settings.DwellTimeMs = (int)numDwellTime.Value;
            scanController.Settings.SkipInspected = chkSkipInspected.Checked;
            scanController.Settings.AutoMarkGood = chkAutoMarkGood.Checked;
        }

        // ===== Обработчики кнопок =====

        private async void BtnScanStart_Click(object sender, EventArgs e)
        {
            if (scanController.State == ScanState.Paused)
            {
                scanController.Resume();
            }
            else
            {
                ApplyScanSettings();
                
                // Если выбран режим "Выделенные" - используем выделение
                HashSet<int> selected = null;
                if (scanController.Settings.Pattern == ScanPattern.SelectedOnly)
                {
                    selected = mouseController.SelectedCrystals;
                    if (selected.Count == 0)
                    {
                        MessageBox.Show("Выделите кристаллы для обхода.", "Автообход", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                await scanController.StartAsync(selected);
            }
        }

        private void BtnScanPause_Click(object sender, EventArgs e)
        {
            scanController.Pause();
        }

        private void BtnScanStop_Click(object sender, EventArgs e)
        {
            scanController.Stop();
        }

        private void BtnScanPrev_Click(object sender, EventArgs e)
        {
            scanController.GoToPrevious();
        }

        private void BtnScanNext_Click(object sender, EventArgs e)
        {
            scanController.GoToNext();
        }

        // ===== Обработчики событий ScanController =====

        private void ScanController_ProgressChanged(object sender, ScanProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, ScanProgressEventArgs>(ScanController_ProgressChanged), sender, e);
                return;
            }

            // Обновляем прогресс на панели
            lblScanProgress.Text = $"{e.Current} / {e.Total}";
            progressBarScan.Maximum = e.Total;
            progressBarScan.Value = Math.Min(e.Current, e.Total);

            // Обновляем прогресс в статусе
            statusProgressBar.Maximum = e.Total;
            statusProgressBar.Value = Math.Min(e.Current, e.Total);
            
            string remaining = e.Remaining.HasValue 
                ? $"~{e.Remaining.Value.Minutes:D2}:{e.Remaining.Value.Seconds:D2}" 
                : "...";
            statusProgressLabel.Text = $"Обход: {e.Current}/{e.Total} ({remaining})";

            // Обновляем статус
            if (e.CurrentCrystal != null)
            {
                lblScanStatus.Text = $"Кристалл #{e.CurrentCrystal.Index} ({e.CurrentCrystal.RealX:F2}, {e.CurrentCrystal.RealY:F2}) мм";
            }
        }

        private void ScanController_CrystalReached(object sender, Crystal crystal)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, Crystal>(ScanController_CrystalReached), sender, crystal);
                return;
            }

            // Выделяем текущий кристалл
            mouseController.ClearSelection();
            mouseController.SelectedCrystals.Add(crystal.Index);
            
            // Центрируем вид на кристалле
            zoomPanController.CenterOnPoint(crystal.RealX, crystal.RealY);
            
            UpdateUI();
        }

        private void ScanController_ScanCompleted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, EventArgs>(ScanController_ScanCompleted), sender, e);
                return;
            }

            lblScanStatus.Text = "Обход завершён!";
            statusProgressLabel.Text = "";
            statusProgressBar.Visible = false;
            statusProgressLabel.Visible = false;

            UpdateScanUI();
            miniMapControl?.InvalidateCache();
            UpdateUI();

            MessageBox.Show(
                $"Автообход завершён!\n\nОбработано кристаллов: {scanController.TotalCount}",
                "Готово",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ScanController_StateChanged(object sender, ScanState newState)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, ScanState>(ScanController_StateChanged), sender, newState);
                return;
            }

            switch (newState)
            {
                case ScanState.Idle:
                    lblScanStatus.Text = "Готов к обходу";
                    break;
                case ScanState.Running:
                    lblScanStatus.Text = "Выполняется обход...";
                    break;
                case ScanState.Paused:
                    lblScanStatus.Text = "Пауза";
                    break;
                case ScanState.Completed:
                    lblScanStatus.Text = "Обход завершён";
                    break;
            }

            UpdateScanUI();
        }
    }
}
