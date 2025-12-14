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
        private ComboBox cmbAutoMarkBin;  // ✅ НОВОЕ: Выбор категории для авто-пометки
        private NumericUpDown numDwellTime;
        private NumericUpDown numGoToCrystal;  // ✅ НОВОЕ: Переход к кристаллу
        private CheckBox chkSkipInspected;
        private CheckBox chkAutoMark;  // ✅ Переименовано
        private CheckBox chkShowRoutePreview;  // ✅ НОВОЕ: Превью маршрута
        private Button btnScanStart;
        private Button btnScanPause;
        private Button btnScanStop;
        private Button btnScanNext;
        private Button btnScanPrev;
        private Button btnGoToCrystal;  // ✅ НОВОЕ: Кнопка перехода
        private Label lblScanStatus;
        private Label lblScanProgress;
        private Label lblElapsedTime;  // ✅ НОВОЕ: Прошедшее время
        private ProgressBar progressBarScan;
        
        // ✅ Прогресс-бар в StatusStrip
        private ToolStripProgressBar statusProgressBar;
        private ToolStripStatusLabel statusProgressLabel;
        
        // ✅ НОВОЕ: Флаг превью маршрута автообхода
        private bool showScanRoutePreview = false;

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

                AppLogger.Debug($"InitializeScanUI: TabControl найден, вкладок до создания: {rightTabControl.TabPages.Count}");

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
                
                // ✅ НОВОЕ: Загружаем сохранённые настройки
                LoadScanSettings();
                
                UpdateScanUI();
                
                AppLogger.Info($"InitializeScanUI: вкладка Автообход создана. Всего вкладок: {rightTabControl.TabPages.Count}");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка InitializeScanUI", ex);
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
                Padding = new Padding(8),
                Name = "tabPageScan"
            };

            // Просто добавляем в конец - это надежнее чем Insert
            rightTabControl.TabPages.Add(tabPageScan);
            
            AppLogger.Debug($"Вкладка Автообход добавлена. Contains={rightTabControl.TabPages.Contains(tabPageScan)}, Count={rightTabControl.TabPages.Count}");

            // Проверяем добавление
            if (!rightTabControl.TabPages.Contains(tabPageScan))
            {
                AppLogger.Error("ОШИБКА: Вкладка Автообход НЕ добавлена в TabControl!");
                // Пробуем альтернативный способ
                rightTabControl.Controls.Add(tabPageScan);
                AppLogger.Debug($"Альтернативное добавление через Controls. Contains={rightTabControl.TabPages.Contains(tabPageScan)}");
            }
            else
            {
                AppLogger.Debug($"Вкладка Автообход успешно в TabControl, индекс: {rightTabControl.TabPages.IndexOf(tabPageScan)}");
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

            // ===== Заголовок =====
            var titleLabel = new Label
            {
                Text = "🔄 Автообход кристаллов",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 28
            };

            // ===== GroupBox: Настройки обхода =====
            var settingsGroup = new GroupBox
            {
                Text = "Настройки обхода",
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(5)
            };

            var settingsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));

            // Строка 1: Паттерн и Старт
            var lblPattern = new Label { Text = "Паттерн:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
            cmbScanPattern = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(2) };
            cmbScanPattern.Items.AddRange(new object[] { "Змейка", "Построчно", "Выделенные" });
            cmbScanPattern.SelectedIndex = 0;
            cmbScanPattern.SelectedIndexChanged += (s, e) => UpdateRoutePreview();

            var lblCorner = new Label { Text = "Старт:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
            cmbStartCorner = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(2) };
            cmbStartCorner.Items.AddRange(new object[] { "↖ Левый верх", "↗ Правый верх", "↙ Левый низ", "↘ Правый низ" });
            cmbStartCorner.SelectedIndex = 0;
            cmbStartCorner.SelectedIndexChanged += (s, e) => UpdateRoutePreview();

            // Строка 2: Пауза и Авто-пометка
            var lblDwell = new Label { Text = "Пауза (мс):", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
            numDwellTime = new NumericUpDown { Minimum = 0, Maximum = 30000, Value = 500, Increment = 100, Dock = DockStyle.Fill, Margin = new Padding(2) };

            chkAutoMark = new CheckBox { Text = "Авто-пометка:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 4, 0, 0) };
            chkAutoMark.CheckedChanged += (s, e) => cmbAutoMarkBin.Enabled = chkAutoMark.Checked;

            cmbAutoMarkBin = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(2), Enabled = false };
            cmbAutoMarkBin.Items.AddRange(new object[] { "✅ Годен", "❌ Брак", "❓ Проверить", "🔧 Доработка", "📐 Край" });
            cmbAutoMarkBin.SelectedIndex = 0;

            // Строка 3: Опции
            chkSkipInspected = new CheckBox { Text = "Пропускать проверенные", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 4, 0, 0) };
            chkSkipInspected.CheckedChanged += (s, e) => UpdateRoutePreview();

            chkShowRoutePreview = new CheckBox { Text = "Показать маршрут", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 4, 0, 0) };
            chkShowRoutePreview.CheckedChanged += ChkShowRoutePreview_CheckedChanged;

            settingsTable.Controls.Add(lblPattern, 0, 0);
            settingsTable.Controls.Add(cmbScanPattern, 1, 0);
            settingsTable.Controls.Add(lblCorner, 2, 0);
            settingsTable.Controls.Add(cmbStartCorner, 3, 0);
            settingsTable.Controls.Add(lblDwell, 0, 1);
            settingsTable.Controls.Add(numDwellTime, 1, 1);
            settingsTable.Controls.Add(chkAutoMark, 2, 1);
            settingsTable.Controls.Add(cmbAutoMarkBin, 3, 1);
            settingsTable.Controls.Add(chkSkipInspected, 0, 2);
            settingsTable.SetColumnSpan(chkSkipInspected, 2);
            settingsTable.Controls.Add(chkShowRoutePreview, 2, 2);
            settingsTable.SetColumnSpan(chkShowRoutePreview, 2);

            settingsGroup.Controls.Add(settingsTable);

            // ===== GroupBox: Управление =====
            var controlGroup = new GroupBox
            {
                Text = "Управление",
                Dock = DockStyle.Top,
                Height = 80,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(5)
            };

            var controlTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2
            };
            for (int i = 0; i < 6; i++)
                controlTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66f));

            // Строка 1: Основные кнопки
            btnScanStart = CreateScanButton("▶ Старт", Color.FromArgb(46, 204, 113), BtnScanStart_Click);
            btnScanPause = CreateScanButton("⏸ Пауза", Color.FromArgb(241, 196, 15), BtnScanPause_Click);
            btnScanStop = CreateScanButton("⏹ Стоп", Color.FromArgb(231, 76, 60), BtnScanStop_Click);
            btnScanPrev = CreateScanButton("◀ Пред", Color.FromArgb(108, 117, 125), BtnScanPrev_Click);
            btnScanNext = CreateScanButton("След ▶", Color.FromArgb(108, 117, 125), BtnScanNext_Click);

            // Строка 2: Переход к кристаллу
            var lblGoTo = new Label { Text = "Перейти к #:", AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 8, 5, 0) };
            numGoToCrystal = new NumericUpDown { Minimum = 0, Maximum = 99999, Value = 0, Dock = DockStyle.Fill, Margin = new Padding(2) };
            btnGoToCrystal = CreateScanButton("→ Перейти", Color.FromArgb(52, 152, 219), BtnGoToCrystal_Click);

            controlTable.Controls.Add(btnScanStart, 0, 0);
            controlTable.Controls.Add(btnScanPause, 1, 0);
            controlTable.Controls.Add(btnScanStop, 2, 0);
            controlTable.Controls.Add(btnScanPrev, 3, 0);
            controlTable.Controls.Add(btnScanNext, 4, 0);
            controlTable.Controls.Add(lblGoTo, 0, 1);
            controlTable.SetColumnSpan(lblGoTo, 2);
            controlTable.Controls.Add(numGoToCrystal, 2, 1);
            controlTable.Controls.Add(btnGoToCrystal, 3, 1);
            controlTable.SetColumnSpan(btnGoToCrystal, 2);

            controlGroup.Controls.Add(controlTable);

            // ===== GroupBox: Прогресс =====
            var progressGroup = new GroupBox
            {
                Text = "Прогресс",
                Dock = DockStyle.Top,
                Height = 90,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(5)
            };

            var progressTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            progressTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            progressTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            lblScanStatus = new Label
            {
                Text = "⏳ Готов к обходу",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 9f)
            };

            lblScanProgress = new Label
            {
                Text = "0 / 0",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            progressBarScan = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Height = 20,
                Style = ProgressBarStyle.Continuous,
                Margin = new Padding(0, 2, 0, 2)
            };

            lblElapsedTime = new Label
            {
                Text = "⏱ 00:00 / ~00:00",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("Segoe UI", 8f)
            };

            progressTable.Controls.Add(lblScanStatus, 0, 0);
            progressTable.Controls.Add(lblScanProgress, 1, 0);
            progressTable.Controls.Add(progressBarScan, 0, 1);
            progressTable.SetColumnSpan(progressBarScan, 2);
            progressTable.Controls.Add(lblElapsedTime, 0, 2);
            progressTable.SetColumnSpan(lblElapsedTime, 2);

            progressGroup.Controls.Add(progressTable);

            // ===== Подсказка по горячим клавишам =====
            var hintLabel = new Label
            {
                Text = "⌨ Горячие клавиши: Space=Пауза/Продолжить | Esc=Стоп | ←/→=Пред/След",
                AutoSize = true,
                Dock = DockStyle.Top,
                ForeColor = Color.FromArgb(140, 140, 140),
                Font = new Font("Segoe UI", 8f),
                Padding = new Padding(0, 5, 0, 0)
            };

            // Собираем панель (в обратном порядке для Dock.Top)
            scanPanel.Controls.Add(hintLabel);
            scanPanel.Controls.Add(progressGroup);
            scanPanel.Controls.Add(controlGroup);
            scanPanel.Controls.Add(settingsGroup);
            scanPanel.Controls.Add(titleLabel);

            // Добавляем на вкладку "Автообход"
            tabPageScan.Controls.Add(scanPanel);
        }

        private Button CreateScanButton(string text, Color backColor, EventHandler handler)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand
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
            btnGoToCrystal.Enabled = hasMap;

            // Настройки (только когда не активен)
            bool canEdit = state == ScanState.Idle || state == ScanState.Completed;
            cmbScanPattern.Enabled = canEdit;
            cmbStartCorner.Enabled = canEdit;
            numDwellTime.Enabled = canEdit;
            chkSkipInspected.Enabled = canEdit;
            chkAutoMark.Enabled = canEdit;
            cmbAutoMarkBin.Enabled = canEdit && chkAutoMark.Checked;
            chkShowRoutePreview.Enabled = canEdit;
            
            // Обновляем максимум для перехода к кристаллу
            numGoToCrystal.Maximum = Math.Max(0, CrystalManager.Instance.Crystals.Count - 1);

            // Текст кнопки старт
            btnScanStart.Text = state == ScanState.Paused ? "▶ Продолжить" : "▶ Старт";
            btnScanStart.BackColor = state == ScanState.Paused 
                ? Color.FromArgb(52, 152, 219)  // Синий для продолжения
                : Color.FromArgb(46, 204, 113); // Зелёный для старта

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
            scanController.Settings.AutoMark = chkAutoMark.Checked;
            scanController.Settings.AutoMarkBin = GetSelectedBinCategory();
        }

        /// <summary>
        /// Получить выбранную категорию для авто-пометки
        /// </summary>
        private BinCategory GetSelectedBinCategory()
        {
            switch (cmbAutoMarkBin.SelectedIndex)
            {
                case 0: return BinCategory.Good;
                case 1: return BinCategory.Defective;
                case 2: return BinCategory.NeedsReview;
                case 3: return BinCategory.Rework;
                case 4: return BinCategory.Edge;
                default: return BinCategory.Good;
            }
        }

        /// <summary>
        /// ✅ НОВОЕ: Обработчик превью маршрута
        /// </summary>
        private void ChkShowRoutePreview_CheckedChanged(object sender, EventArgs e)
        {
            showScanRoutePreview = chkShowRoutePreview.Checked;
            UpdateRoutePreview();
        }

        /// <summary>
        /// ✅ НОВОЕ: Обновить превью маршрута на карте
        /// </summary>
        private void UpdateRoutePreview()
        {
            if (!showScanRoutePreview || scanController == null)
            {
                // Скрыть превью - устанавливаем пустой маршрут
                routePreview.SetRoute(null);
                UpdateUI();
                return;
            }

            // Применяем текущие настройки для построения превью
            ApplyScanSettings();
            
            // Строим маршрут
            HashSet<int> selected = null;
            if (scanController.Settings.Pattern == ScanPattern.SelectedOnly)
            {
                selected = mouseController.SelectedCrystals;
            }
            
            var route = scanController.BuildRoute(selected);
            
            // Устанавливаем маршрут в RoutePreview
            if (route.Count > 0)
            {
                routePreview.SetRoute(route);
            }
            else
            {
                routePreview.SetRoute(null);
            }
            
            UpdateUI();
        }

        /// <summary>
        /// ✅ НОВОЕ: Сохранение настроек обхода
        /// </summary>
        private void SaveScanSettings()
        {
            try
            {
                Properties.Settings.Default.ScanPattern = cmbScanPattern.SelectedIndex;
                Properties.Settings.Default.ScanStartCorner = cmbStartCorner.SelectedIndex;
                Properties.Settings.Default.ScanDwellTime = (int)numDwellTime.Value;
                Properties.Settings.Default.ScanSkipInspected = chkSkipInspected.Checked;
                Properties.Settings.Default.ScanAutoMark = chkAutoMark.Checked;
                Properties.Settings.Default.ScanAutoMarkBin = cmbAutoMarkBin.SelectedIndex;
                Properties.Settings.Default.Save();
                AppLogger.Debug("Настройки автообхода сохранены");
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Не удалось сохранить настройки автообхода: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ НОВОЕ: Загрузка настроек обхода
        /// </summary>
        private void LoadScanSettings()
        {
            try
            {
                if (Properties.Settings.Default.ScanPattern >= 0 && Properties.Settings.Default.ScanPattern < cmbScanPattern.Items.Count)
                    cmbScanPattern.SelectedIndex = Properties.Settings.Default.ScanPattern;
                
                if (Properties.Settings.Default.ScanStartCorner >= 0 && Properties.Settings.Default.ScanStartCorner < cmbStartCorner.Items.Count)
                    cmbStartCorner.SelectedIndex = Properties.Settings.Default.ScanStartCorner;
                
                numDwellTime.Value = Math.Max(numDwellTime.Minimum, Math.Min(numDwellTime.Maximum, Properties.Settings.Default.ScanDwellTime));
                chkSkipInspected.Checked = Properties.Settings.Default.ScanSkipInspected;
                chkAutoMark.Checked = Properties.Settings.Default.ScanAutoMark;
                
                if (Properties.Settings.Default.ScanAutoMarkBin >= 0 && Properties.Settings.Default.ScanAutoMarkBin < cmbAutoMarkBin.Items.Count)
                    cmbAutoMarkBin.SelectedIndex = Properties.Settings.Default.ScanAutoMarkBin;
                
                AppLogger.Debug("Настройки автообхода загружены");
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Не удалось загрузить настройки автообхода: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ НОВОЕ: Обработка горячих клавиш автообхода
        /// </summary>
        private bool HandleScanHotKeys(KeyEventArgs e)
        {
            if (scanController == null) return false;

            var state = scanController.State;

            // Space - Пауза/Продолжить
            if (e.KeyCode == Keys.Space && !e.Control && !e.Alt)
            {
                if (state == ScanState.Running)
                {
                    scanController.Pause();
                    return true;
                }
                else if (state == ScanState.Paused)
                {
                    scanController.Resume();
                    return true;
                }
            }

            // Escape - Стоп
            if (e.KeyCode == Keys.Escape && (state == ScanState.Running || state == ScanState.Paused))
            {
                scanController.Stop();
                return true;
            }

            // Стрелки для навигации (только в режиме паузы или после завершения)
            if (state == ScanState.Paused || state == ScanState.Completed)
            {
                if (e.KeyCode == Keys.Left && scanController.CurrentIndex > 0)
                {
                    scanController.GoToPrevious();
                    return true;
                }
                if (e.KeyCode == Keys.Right && scanController.CurrentIndex < scanController.TotalCount - 1)
                {
                    scanController.GoToNext();
                    return true;
                }
            }

            return false;
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
                SaveScanSettings();  // ✅ Сохраняем настройки при старте
                
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

                // Скрываем превью маршрута при запуске
                if (showScanRoutePreview)
                {
                    chkShowRoutePreview.Checked = false;
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

        /// <summary>
        /// ✅ НОВОЕ: Переход к конкретному кристаллу
        /// </summary>
        private void BtnGoToCrystal_Click(object sender, EventArgs e)
        {
            int targetIndex = (int)numGoToCrystal.Value;
            var crystals = CrystalManager.Instance.Crystals;
            
            if (crystals == null || crystals.Count == 0)
            {
                MessageBox.Show("Нет кристаллов на карте.", "Автообход", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ищем кристалл по индексу
            var crystal = crystals.Find(c => c.Index == targetIndex);
            if (crystal == null)
            {
                MessageBox.Show($"Кристалл #{targetIndex} не найден.", "Автообход", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Выделяем кристалл
            mouseController.ClearSelection();
            mouseController.SelectedCrystals.Add(crystal.Index);
            
            // Центрируем вид
            zoomPanController.CenterOnPoint(crystal.RealX, crystal.RealY);
            
            // Обновляем статус
            lblScanStatus.Text = $"📍 Кристалл #{crystal.Index} ({crystal.RealX:F2}, {crystal.RealY:F2}) мм";
            
            UpdateUI();
            AppLogger.Info($"Переход к кристаллу #{crystal.Index}");
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
            
            // Форматируем время
            string elapsed = $"{e.Elapsed.Minutes:D2}:{e.Elapsed.Seconds:D2}";
            string remaining = e.Remaining.HasValue 
                ? $"~{e.Remaining.Value.Minutes:D2}:{e.Remaining.Value.Seconds:D2}" 
                : "...";
            
            lblElapsedTime.Text = $"⏱ {elapsed} / {remaining}";
            statusProgressLabel.Text = $"Обход: {e.Current}/{e.Total} ({remaining})";

            // Обновляем статус
            if (e.CurrentCrystal != null)
            {
                string binInfo = e.CurrentCrystal.Bin != BinCategory.NotInspected 
                    ? $" [{BinMapSettings.GetBinName(e.CurrentCrystal.Bin)}]" 
                    : "";
                lblScanStatus.Text = $"🔍 #{e.CurrentCrystal.Index} ({e.CurrentCrystal.RealX:F2}, {e.CurrentCrystal.RealY:F2}) мм{binInfo}";
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

            lblScanStatus.Text = "✅ Обход завершён!";
            lblScanStatus.ForeColor = Color.FromArgb(46, 204, 113);
            statusProgressLabel.Text = "";
            statusProgressBar.Visible = false;
            statusProgressLabel.Visible = false;

            UpdateScanUI();
            miniMapControl?.InvalidateCache();
            UpdateUI();

            // Подсчёт статистики
            int goodCount = 0, defectCount = 0, reviewCount = 0;
            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                switch (crystal.Bin)
                {
                    case BinCategory.Good: goodCount++; break;
                    case BinCategory.Defective: defectCount++; break;
                    case BinCategory.NeedsReview: reviewCount++; break;
                }
            }

            MessageBox.Show(
                $"Автообход завершён!\n\n" +
                $"Обработано кристаллов: {scanController.TotalCount}\n\n" +
                $"Статистика Bin Map:\n" +
                $"  ✅ Годен: {goodCount}\n" +
                $"  ❌ Брак: {defectCount}\n" +
                $"  ❓ Проверить: {reviewCount}",
                "Готово",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            // Восстанавливаем цвет статуса
            lblScanStatus.ForeColor = Color.FromArgb(100, 100, 100);
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
                    lblScanStatus.Text = "⏳ Готов к обходу";
                    lblScanStatus.ForeColor = Color.FromArgb(100, 100, 100);
                    lblElapsedTime.Text = "⏱ 00:00 / ~00:00";
                    break;
                case ScanState.Running:
                    lblScanStatus.Text = "▶ Выполняется обход...";
                    lblScanStatus.ForeColor = Color.FromArgb(52, 152, 219);
                    break;
                case ScanState.Paused:
                    lblScanStatus.Text = "⏸ Пауза (Space - продолжить)";
                    lblScanStatus.ForeColor = Color.FromArgb(241, 196, 15);
                    break;
                case ScanState.Completed:
                    lblScanStatus.Text = "✅ Обход завершён";
                    lblScanStatus.ForeColor = Color.FromArgb(46, 204, 113);
                    break;
            }

            UpdateScanUI();
        }
    }
}
