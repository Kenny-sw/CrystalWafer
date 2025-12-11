using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CrystalTable.Logic;

namespace CrystalTable.Forms
{
    public class LogViewerForm : Form
    {
        private RichTextBox logTextBox;
        private Button btnRefresh;
        private Button btnClear;
        private Button btnCopy;
        private Button btnClose;
        private CheckBox chkAutoScroll;
        private ComboBox cmbLogLevel;
        private Label lblStatus;

        private AppLogLevel filterLevel = AppLogLevel.Trace;

        public LogViewerForm()
        {
            InitializeComponents();
            LoadLogs();
            
            // Подписываемся на события логирования
            AppLogger.LogMessagePublished += OnLogMessagePublished;
        }

        private void InitializeComponents()
        {
            this.Text = "Логи приложения";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);

            // TextBox для логов
            logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.None,
                WordWrap = false
            };

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10, 5, 10, 10)
            };

            btnRefresh = new Button
            {
                Text = "Обновить",
                Width = 100,
                Height = 35,
                Location = new Point(10, 5)
            };
            btnRefresh.Click += (s, e) => LoadLogs();

            btnClear = new Button
            {
                Text = "Очистить",
                Width = 100,
                Height = 35,
                Location = new Point(120, 5)
            };
            btnClear.Click += (s, e) => logTextBox.Clear();

            btnCopy = new Button
            {
                Text = "Копировать",
                Width = 100,
                Height = 35,
                Location = new Point(230, 5)
            };
            btnCopy.Click += (s, e) => 
            {
                if (!string.IsNullOrEmpty(logTextBox.Text))
                {
                    Clipboard.SetText(logTextBox.Text);
                    lblStatus.Text = "Логи скопированы в буфер обмена";
                }
            };

            chkAutoScroll = new CheckBox
            {
                Text = "Авто-прокрутка",
                Checked = true,
                Location = new Point(340, 10),
                Width = 120
            };

            // Добавляем фильтр по уровню логирования
            var lblFilter = new Label
            {
                Text = "Уровень:",
                Location = new Point(470, 10),
                Width = 60,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbLogLevel = new ComboBox
            {
                Location = new Point(530, 7),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLogLevel.Items.AddRange(new object[] { "Trace", "Debug", "Info", "Warning", "Error" });
            cmbLogLevel.SelectedIndex = 0;
            cmbLogLevel.SelectedIndexChanged += (s, e) => 
            {
                filterLevel = (AppLogLevel)cmbLogLevel.SelectedIndex;
                LoadLogs();
            };

            btnClose = new Button
            {
                Text = "Закрыть",
                Width = 100,
                Height = 35,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => Close();

            lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 5)
            };

            buttonPanel.Controls.Add(btnRefresh);
            buttonPanel.Controls.Add(btnClear);
            buttonPanel.Controls.Add(btnCopy);
            buttonPanel.Controls.Add(chkAutoScroll);
            buttonPanel.Controls.Add(lblFilter);
            buttonPanel.Controls.Add(cmbLogLevel);
            buttonPanel.Controls.Add(btnClose);

            // Вычисляем позицию кнопки Close
            this.Resize += (s, e) => 
            {
                btnClose.Location = new Point(buttonPanel.Width - btnClose.Width - 10, 5);
            };
            btnClose.Location = new Point(buttonPanel.Width - btnClose.Width - 10, 5);

            this.Controls.Add(logTextBox);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(lblStatus);

            this.FormClosing += (s, e) => 
            {
                AppLogger.LogMessagePublished -= OnLogMessagePublished;
            };
        }

        private void LoadLogs()
        {
            try
            {
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                var todayLog = Path.Combine(logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");

                if (File.Exists(todayLog))
                {
                    var content = File.ReadAllText(todayLog);
                    logTextBox.Text = content;
                    
                    if (chkAutoScroll.Checked)
                    {
                        logTextBox.SelectionStart = logTextBox.Text.Length;
                        logTextBox.ScrollToCaret();
                    }

                    lblStatus.Text = $"Загружено: {todayLog}";
                }
                else
                {
                    logTextBox.Text = "Лог-файл не найден.";
                    lblStatus.Text = "Файл не найден";
                }
            }
            catch (Exception ex)
            {
                logTextBox.Text = $"Ошибка загрузки логов: {ex.Message}";
                lblStatus.Text = "Ошибка загрузки";
            }
        }

        private void OnLogMessagePublished(object sender, AppLogEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, AppLogEventArgs>(OnLogMessagePublished), sender, e);
                return;
            }

            try
            {
                var entry = e.Entry;
                
                // Фильтрация по уровню
                if (entry.Level < filterLevel)
                    return;
                
                var timestamp = entry.Timestamp.ToString("HH:mm:ss.fff");
                var line = $"{timestamp} [{entry.Level}] {entry.Message}";

                // Цветовая подсветка
                var color = GetColorForLevel(entry.Level);
                
                logTextBox.SelectionStart = logTextBox.TextLength;
                logTextBox.SelectionLength = 0;
                logTextBox.SelectionColor = color;
                logTextBox.AppendText(line + Environment.NewLine);
                logTextBox.SelectionColor = logTextBox.ForeColor;

                if (entry.Exception != null)
                {
                    logTextBox.SelectionColor = Color.Red;
                    logTextBox.AppendText(entry.Exception.ToString() + Environment.NewLine);
                    logTextBox.SelectionColor = logTextBox.ForeColor;
                }

                if (chkAutoScroll.Checked)
                {
                    logTextBox.SelectionStart = logTextBox.Text.Length;
                    logTextBox.ScrollToCaret();
                }
            }
            catch
            {
                // Ignore errors in log viewer
            }
        }

        private Color GetColorForLevel(AppLogLevel level)
        {
            switch (level)
            {
                case AppLogLevel.Trace:
                    return Color.Gray;
                case AppLogLevel.Debug:
                    return Color.LightGray;
                case AppLogLevel.Info:
                    return Color.LightGreen;
                case AppLogLevel.Warning:
                    return Color.Orange;
                case AppLogLevel.Error:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }
    }
}
