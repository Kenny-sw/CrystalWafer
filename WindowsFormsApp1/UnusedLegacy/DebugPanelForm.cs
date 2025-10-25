using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrystalTable.Controllers;
using CrystalTable.Logic;

namespace CrystalTable
{
    internal sealed class DebugPanelForm : Form
    {
        private const int MaxLogEntries = 200;

        private readonly Form1 owner;
        private readonly SerialPortController serialPortController;
        private readonly ListBox logListBox;
        private readonly Label coordinatesValueLabel;
        private readonly Label sensorStatusValueLabel;
        private readonly Label connectionStatusValueLabel;
        private readonly TextBox commandTextBox;
        private readonly TextBox dataTextBox;
        private readonly Button sendButton;
        private readonly Timer updateTimer;

        public DebugPanelForm(Form1 owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            serialPortController = owner.SerialPortController;

            Text = "Панель наладки";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 460);
            MinimizeBox = true;
            MaximizeBox = true;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var statusPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(6, 6, 6, 0)
            };

            statusPanel.Controls.Add(new Label { Text = "Координаты:", AutoSize = true, Margin = new Padding(0, 3, 4, 3) });
            coordinatesValueLabel = new Label { AutoSize = true, Margin = new Padding(0, 3, 12, 3) };
            statusPanel.Controls.Add(coordinatesValueLabel);

            statusPanel.Controls.Add(new Label { Text = "Датчик:", AutoSize = true, Margin = new Padding(0, 3, 4, 3) });
            sensorStatusValueLabel = new Label { AutoSize = true, Margin = new Padding(0, 3, 12, 3) };
            statusPanel.Controls.Add(sensorStatusValueLabel);

            statusPanel.Controls.Add(new Label { Text = "COM:", AutoSize = true, Margin = new Padding(0, 3, 4, 3) });
            connectionStatusValueLabel = new Label { AutoSize = true, Margin = new Padding(0, 3, 12, 3) };
            statusPanel.Controls.Add(connectionStatusValueLabel);

            var hintLabel = new Label
            {
                Text = "Запускайте команды только при необходимости сервисной наладки.",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 2, 6, 4)
            };

            logListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                HorizontalScrollbar = true
            };

            var commandPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(6)
            };

            commandPanel.Controls.Add(new Label { Text = "Команда (hex):", AutoSize = true, Margin = new Padding(0, 6, 4, 3) });

            commandTextBox = new TextBox
            {
                Width = 60,
                Text = "01"
            };
            commandPanel.Controls.Add(commandTextBox);

            commandPanel.Controls.Add(new Label { Text = "Данные (uint):", AutoSize = true, Margin = new Padding(8, 6, 4, 3) });

            dataTextBox = new TextBox
            {
                Width = 100,
                Text = "0"
            };
            commandPanel.Controls.Add(dataTextBox);

            sendButton = new Button
            {
                Text = "Отправить",
                AutoSize = true,
                Margin = new Padding(12, 3, 0, 3)
            };
            sendButton.Click += async (s, e) => await SendManualCommandAsync();
            commandPanel.Controls.Add(sendButton);

            layout.Controls.Add(statusPanel, 0, 0);
            layout.Controls.Add(hintLabel, 0, 1);
            layout.Controls.Add(logListBox, 0, 2);
            layout.Controls.Add(commandPanel, 0, 3);

            Controls.Add(layout);

            AppLogger.LogMessagePublished += OnLogMessagePublished;

            updateTimer = new Timer { Interval = 500 };
            updateTimer.Tick += (s, e) => UpdateCoordinateDisplay();
            updateTimer.Start();

            UpdateCoordinateDisplay();
        }

        public void InitializeState(bool? sensorOn, bool isConnected, string portName)
        {
            SetSensorState(sensorOn);
            SetConnectionState(isConnected, portName);
            UpdateCoordinateDisplay();
        }

        public void SetSensorState(bool? isOn)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool?>(SetSensorState), isOn);
                return;
            }

            sensorStatusValueLabel.Text = isOn.HasValue
                ? (isOn.Value ? "ВКЛ" : "ВЫКЛ")
                : "—";
        }

        public void SetConnectionState(bool isConnected, string portName)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool, string>(SetConnectionState), isConnected, portName);
                return;
            }

            connectionStatusValueLabel.Text = isConnected
                ? (string.IsNullOrWhiteSpace(portName) ? "Подключено" : $"Подключено ({portName})")
                : "Отключено";
            sendButton.Enabled = isConnected && serialPortController != null;
        }

        public void AppendLogLine(string line)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLogLine), line);
                return;
            }

            logListBox.BeginUpdate();
            logListBox.Items.Insert(0, line);
            while (logListBox.Items.Count > MaxLogEntries)
            {
                logListBox.Items.RemoveAt(logListBox.Items.Count - 1);
            }
            logListBox.EndUpdate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            AppLogger.LogMessagePublished -= OnLogMessagePublished;
            updateTimer?.Stop();
            updateTimer?.Dispose();
        }

        private void OnLogMessagePublished(object sender, AppLogEventArgs e)
        {
            if (e?.Entry == null)
            {
                return;
            }

            string formatted = e.Entry.Exception != null
                ? $"{e.Entry.Timestamp:HH:mm:ss.fff} [{e.Entry.Level}] {e.Entry.Message} :: {e.Entry.Exception.Message}"
                : $"{e.Entry.Timestamp:HH:mm:ss.fff} [{e.Entry.Level}] {e.Entry.Message}";

            AppendLogLine(formatted);
        }

        private void UpdateCoordinateDisplay()
        {
            if (IsDisposed) return;

            PointF pointer;
            try
            {
                pointer = owner.GetPointerMm();
            }
            catch
            {
                pointer = PointF.Empty;
            }

            string text = string.Format(CultureSettings.NumericCulture, "{0:F3} мм, {1:F3} мм", pointer.X, pointer.Y);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => coordinatesValueLabel.Text = text));
            }
            else
            {
                coordinatesValueLabel.Text = text;
            }
        }

        private async Task SendManualCommandAsync()
        {
            if (serialPortController == null)
            {
                MessageBox.Show("Подключение к контроллеру отсутствует.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryParseCommand(out byte command, out uint data))
            {
                return;
            }

            sendButton.Enabled = false;
            try
            {
                AppendLogLine($"[{DateTime.Now:HH:mm:ss.fff}] [UI] -> cmd=0x{command:X2}, data={data}");
                bool ok = await serialPortController.SendCommandAsync(command, data).ConfigureAwait(true);
                AppendLogLine($"[{DateTime.Now:HH:mm:ss.fff}] [UI] <- {(ok ? "OK" : "ERR")}");
            }
            catch (Exception ex)
            {
                AppendLogLine($"[{DateTime.Now:HH:mm:ss.fff}] [UI] ERROR: {ex.Message}");
                MessageBox.Show($"Не удалось отправить команду: {ex.Message}", "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sendButton.Enabled = true;
            }
        }

        private bool TryParseCommand(out byte command, out uint data)
        {
            command = 0;
            data = 0;

            string commandText = commandTextBox.Text.Trim();
            if (commandText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                commandText = commandText.Substring(2);
            }

            if (!byte.TryParse(commandText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out command))
            {
                MessageBox.Show("Команда должна быть в диапазоне 00–FF.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string dataText = dataTextBox.Text.Trim();
            if (!uint.TryParse(dataText, NumberStyles.Integer, CultureInfo.InvariantCulture, out data))
            {
                MessageBox.Show("Данные должны быть целым числом (0–4294967295).", "COM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}

