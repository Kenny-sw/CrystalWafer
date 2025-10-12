using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Controllers;

namespace CrystalTable
{
    public partial class Form1
    {
        private Panel mapBuilderPanel;
        private Label mapBuilderStatusLabel;
        private NumericUpDown mapDiameterInput;
        private NumericUpDown mapWidthInput;
        private NumericUpDown mapHeightInput;
        private NumericUpDown mapStreetInput;
        private NumericUpDown mapOffsetXInput;
        private NumericUpDown mapOffsetYInput;
        private CheckBox mapMirrorXCheckBox;
        private CheckBox mapMirrorYCheckBox;
        private Button mapStartButton;
        private Button mapApplyButton;
        private Button mapCancelButton;
        private Button mapEditButton;
        private Button mapHalfStepXButton;
        private Button mapHalfStepYButton;
        private Button mapShiftLeftButton;
        private Button mapShiftRightButton;
        private Button mapShiftUpButton;
        private Button mapShiftDownButton;
        private Button mapSwapOrientationButton;
        private ToolStripButton toolStripCreateMapButton;
        private ToolStripButton toolStripEditMapButton;
        private ToolStripButton toolStripSavePngButton;
        private bool mapInputsSyncLock;

        private void InitializeMapBuilderUi()
        {
            CreateMapBuilderPanel();
            CreateMapBuilderToolbarButtons();
            SyncMapBuilderUi();
        }

        private void CreateMapBuilderPanel()
        {
            mapBuilderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 240,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(246, 250, 246)
            };

            var titleLabel = new Label
            {
                Text = "Конструктор карты",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22
            };

            mapBuilderStatusLabel = new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Top,
                Height = 18,
                ForeColor = Color.FromArgb(72, 96, 72)
            };

            var inputsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 6,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            inputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            inputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            mapDiameterInput = CreateNumericUpDown((decimal)WaferController.MinWaferDiameter, (decimal)WaferController.MaxWaferDiameter, 1m, 1);
            mapWidthInput = CreateNumericUpDown(0.1m, 200.0m, 0.1m, 3);
            mapHeightInput = CreateNumericUpDown(0.1m, 200.0m, 0.1m, 3);
            mapStreetInput = CreateNumericUpDown(0m, 10m, 0.05m, 3);
            mapOffsetXInput = CreateNumericUpDown(-200m, 200m, 0.05m, 3);
            mapOffsetYInput = CreateNumericUpDown(-200m, 200m, 0.05m, 3);

            AddLabeledControl(inputsTable, "Диаметр D, мм", mapDiameterInput, 0);
            AddLabeledControl(inputsTable, "Ширина W, мм", mapWidthInput, 1);
            AddLabeledControl(inputsTable, "Высота H, мм", mapHeightInput, 2);
            AddLabeledControl(inputsTable, "Ширина street, мм", mapStreetInput, 3);
            AddLabeledControl(inputsTable, "Смещение X, мм", mapOffsetXInput, 4);
            AddLabeledControl(inputsTable, "Смещение Y, мм", mapOffsetYInput, 5);

            var mirrorPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };

            mapMirrorXCheckBox = new CheckBox { Text = "Отразить X" };
            mapMirrorYCheckBox = new CheckBox { Text = "Отразить Y" };
            mirrorPanel.Controls.Add(mapMirrorXCheckBox);
            mirrorPanel.Controls.Add(mapMirrorYCheckBox);

            var shiftPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };

            mapShiftLeftButton = CreateActionButton("Влево", MapShiftLeft_Click);
            mapShiftRightButton = CreateActionButton("Вправо", MapShiftRight_Click);
            mapShiftUpButton = CreateActionButton("Вверх", MapShiftUp_Click);
            mapShiftDownButton = CreateActionButton("Вниз", MapShiftDown_Click);
            mapHalfStepXButton = CreateActionButton("X/2", MapHalfStepX_Click);
            mapHalfStepYButton = CreateActionButton("Y/2", MapHalfStepY_Click);
            mapSwapOrientationButton = CreateActionButton("W-H", MapSwapOrientation_Click);

            shiftPanel.Controls.Add(mapShiftLeftButton);
            shiftPanel.Controls.Add(mapShiftRightButton);
            shiftPanel.Controls.Add(mapShiftUpButton);
            shiftPanel.Controls.Add(mapShiftDownButton);
            shiftPanel.Controls.Add(mapHalfStepXButton);
            shiftPanel.Controls.Add(mapHalfStepYButton);
            shiftPanel.Controls.Add(mapSwapOrientationButton);

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };

            mapStartButton = CreatePrimaryButton("Создать карту", MapStartButton_Click);
            mapApplyButton = CreatePrimaryButton("Применить черновик", MapApplyButton_Click);
            mapCancelButton = CreateSecondaryButton("Отмена", MapCancelButton_Click);
            mapEditButton = CreateSecondaryButton("Редактировать карту", MapEditButton_Click);

            buttonsPanel.Controls.Add(mapStartButton);
            buttonsPanel.Controls.Add(mapApplyButton);
            buttonsPanel.Controls.Add(mapCancelButton);
            buttonsPanel.Controls.Add(mapEditButton);

            mapBuilderPanel.Controls.Add(buttonsPanel);
            mapBuilderPanel.Controls.Add(shiftPanel);
            mapBuilderPanel.Controls.Add(mirrorPanel);
            mapBuilderPanel.Controls.Add(inputsTable);
            mapBuilderPanel.Controls.Add(mapBuilderStatusLabel);
            mapBuilderPanel.Controls.Add(titleLabel);

            mapDiameterInput.ValueChanged += MapDiameterInput_ValueChanged;
            mapWidthInput.ValueChanged += MapWidthInput_ValueChanged;
            mapHeightInput.ValueChanged += MapHeightInput_ValueChanged;
            mapStreetInput.ValueChanged += MapStreetInput_ValueChanged;
            mapOffsetXInput.ValueChanged += MapOffsetXInput_ValueChanged;
            mapOffsetYInput.ValueChanged += MapOffsetYInput_ValueChanged;
            mapMirrorXCheckBox.CheckedChanged += MapMirrorCheckBox_CheckedChanged;
            mapMirrorYCheckBox.CheckedChanged += MapMirrorCheckBox_CheckedChanged;

            rightPanel.Controls.Add(mapBuilderPanel);
            rightPanel.Controls.SetChildIndex(mapBuilderPanel, 0);
        }

        private void CreateMapBuilderToolbarButtons()
        {
            toolStripCreateMapButton = new ToolStripButton
            {
                Text = "Создать карту",
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            toolStripCreateMapButton.Click += MapStartButton_Click;

            toolStripEditMapButton = new ToolStripButton
            {
                Text = "Редактировать карту",
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            toolStripEditMapButton.Click += MapEditButton_Click;

            toolStripSavePngButton = new ToolStripButton
            {
                Text = "Экспорт PNG",
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            toolStripSavePngButton.Click += MapSavePng_Click;

            toolStrip1.Items.Add(new ToolStripSeparator());
            toolStrip1.Items.Add(toolStripCreateMapButton);
            toolStrip1.Items.Add(toolStripEditMapButton);
            toolStrip1.Items.Add(toolStripSavePngButton);
        }

        private NumericUpDown CreateNumericUpDown(decimal min, decimal max, decimal increment, int decimals)
        {
            return new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Increment = increment,
                DecimalPlaces = decimals,
                Width = 90,
                TextAlign = HorizontalAlignment.Right
            };
        }

        private void AddLabeledControl(TableLayoutPanel panel, string label, Control control, int row)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Margin = new Padding(0, 3, 6, 3)
            };
            control.Margin = new Padding(0, 3, 0, 3);
            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(control, 1, row);
        }

        private Button CreateActionButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                Width = 48,
                Height = 28,
                Margin = new Padding(3)
            };
            button.Click += handler;
            return button;
        }

        private Button CreatePrimaryButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                Width = 150,
                Height = 30,
                Margin = new Padding(3)
            };
            button.Click += handler;
            return button;
        }

        private Button CreateSecondaryButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                Width = 150,
                Height = 30,
                Margin = new Padding(3)
            };
            button.Click += handler;
            return button;
        }

        private void SyncMapBuilderUi()
        {
            mapInputsSyncLock = true;
            try
            {
                var snapshot = waferController.GetEffectiveMapSnapshot();
                if (snapshot != null)
                {
                    mapDiameterInput.Value = ClampToNumeric(snapshot.DiameterMm, mapDiameterInput);
                    mapWidthInput.Value = ClampToNumeric(snapshot.CrystalWidthMm, mapWidthInput);
                    mapHeightInput.Value = ClampToNumeric(snapshot.CrystalHeightMm, mapHeightInput);
                    mapStreetInput.Value = ClampToNumeric(snapshot.StreetMm, mapStreetInput);
                    mapOffsetXInput.Value = ClampToNumeric(snapshot.OffsetXMm, mapOffsetXInput);
                    mapOffsetYInput.Value = ClampToNumeric(snapshot.OffsetYMm, mapOffsetYInput);
                    mapMirrorXCheckBox.Checked = snapshot.MirrorX;
                    mapMirrorYCheckBox.Checked = snapshot.MirrorY;
                }
                else
                {
                    float defaultDiameter = waferController.WaferDiameter > 0 ? waferController.WaferDiameter : 200f;
                    float defaultWidth = waferController.CrystalWidthRaw > 0 ? waferController.CrystalWidthRaw / 1000f : 10f;
                    float defaultHeight = waferController.CrystalHeightRaw > 0 ? waferController.CrystalHeightRaw / 1000f : 10f;

                    mapDiameterInput.Value = ClampToNumeric(defaultDiameter, mapDiameterInput);
                    mapWidthInput.Value = ClampToNumeric(defaultWidth, mapWidthInput);
                    mapHeightInput.Value = ClampToNumeric(defaultHeight, mapHeightInput);
                    mapStreetInput.Value = ClampToNumeric(0f, mapStreetInput);
                    mapOffsetXInput.Value = ClampToNumeric(0f, mapOffsetXInput);
                    mapOffsetYInput.Value = ClampToNumeric(0f, mapOffsetYInput);
                    mapMirrorXCheckBox.Checked = false;
                    mapMirrorYCheckBox.Checked = false;
                }

                bool editing = waferController.IsMapEditing;
                bool hasActive = waferController.HasActiveMap;

                mapDiameterInput.Enabled = editing;
                mapWidthInput.Enabled = editing;
                mapHeightInput.Enabled = editing;
                mapStreetInput.Enabled = editing;
                mapOffsetXInput.Enabled = editing;
                mapOffsetYInput.Enabled = editing;
                mapMirrorXCheckBox.Enabled = editing;
                mapMirrorYCheckBox.Enabled = editing;

                mapShiftLeftButton.Enabled = editing;
                mapShiftRightButton.Enabled = editing;
                mapShiftUpButton.Enabled = editing;
                mapShiftDownButton.Enabled = editing;
                mapHalfStepXButton.Enabled = editing;
                mapHalfStepYButton.Enabled = editing;
                mapSwapOrientationButton.Enabled = editing;

                mapStartButton.Enabled = !editing;
                mapApplyButton.Enabled = editing;
                mapCancelButton.Enabled = editing;
                mapEditButton.Enabled = hasActive && !editing;

                toolStripCreateMapButton.Enabled = !editing;
                toolStripEditMapButton.Enabled = hasActive && !editing;
                toolStripSavePngButton.Enabled = hasActive;

                mapBuilderStatusLabel.Text = editing
                    ? "Редактируется черновик карты"
                    : hasActive ? "Активная карта готова" : "Карта не создана";
            }
            finally
            {
                mapInputsSyncLock = false;
            }
        }

        private decimal ClampToNumeric(float value, NumericUpDown control)
        {
            decimal dec = (decimal)value;
            if (dec < control.Minimum) dec = control.Minimum;
            if (dec > control.Maximum) dec = control.Maximum;
            return Math.Round(dec, control.DecimalPlaces);
        }

        private void MapStartButton_Click(object sender, EventArgs e)
        {
            var parameters = new WaferMapParameters
            {
                DiameterMm = (float)mapDiameterInput.Value,
                CrystalWidthMm = (float)mapWidthInput.Value,
                CrystalHeightMm = (float)mapHeightInput.Value,
                StreetMm = (float)mapStreetInput.Value,
                OffsetXMm = (float)mapOffsetXInput.Value,
                OffsetYMm = (float)mapOffsetYInput.Value,
                MirrorX = mapMirrorXCheckBox.Checked,
                MirrorY = mapMirrorYCheckBox.Checked,
                SwapOrientation = false
            };

            waferController.BeginMapCreation(parameters);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
            UpdateUI();
        }

        private void MapApplyButton_Click(object sender, EventArgs e)
        {
            waferController.ApplyDraftMap();
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
            UpdateUI();
        }

        private void MapCancelButton_Click(object sender, EventArgs e)
        {
            waferController.CancelDraftMap();
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
            UpdateUI();
        }

        private void MapEditButton_Click(object sender, EventArgs e)
        {
            if (!waferController.HasActiveMap)
            {
                return;
            }

            waferController.BeginMapEdit();
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
            UpdateUI();
        }

        private void MapSavePng_Click(object sender, EventArgs e)
        {
            if (!waferController.HasActiveMap && !waferController.IsMapEditing)
            {
                MessageBox.Show("Нет активной карты для сохранения.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Сохранить карту в PNG",
                Filter = "PNG файлы (*.png)|*.png",
                DefaultExt = "png"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            waferController.UpdateDisplayCache(pictureBox1.Width, pictureBox1.Height);
            var bitmap = waferController.GetWaferBitmap(pictureBox1.Width, pictureBox1.Height);
            if (bitmap == null)
            {
                MessageBox.Show("Не удалось сформировать изображение карты.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var clone = new Bitmap(bitmap);
            clone.Save(dialog.FileName, ImageFormat.Png);
        }

        private void MapShiftLeft_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.NudgeDraftOffsets(-waferController.StepXmmOrDefault, 0f);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapShiftRight_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.NudgeDraftOffsets(waferController.StepXmmOrDefault, 0f);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapShiftUp_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.NudgeDraftOffsets(0f, -waferController.StepYmmOrDefault);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapShiftDown_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.NudgeDraftOffsets(0f, waferController.StepYmmOrDefault);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapHalfStepX_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.NudgeDraftOffsets(waferController.StepXmmOrDefault / 2f, 0f);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapHalfStepY_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.NudgeDraftOffsets(0f, waferController.StepYmmOrDefault / 2f);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapSwapOrientation_Click(object sender, EventArgs e)
        {
            if (!waferController.IsMapEditing) return;
            waferController.ToggleDraftOrientation();
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapDiameterInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.UpdateDraftDiameter((float)mapDiameterInput.Value);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapWidthInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.UpdateDraftCrystalSize((float)mapWidthInput.Value, (float)mapHeightInput.Value);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapHeightInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.UpdateDraftCrystalSize((float)mapWidthInput.Value, (float)mapHeightInput.Value);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapStreetInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.UpdateDraftStreet((float)mapStreetInput.Value);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapOffsetXInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.UpdateDraftOffsets((float)mapOffsetXInput.Value, (float)mapOffsetYInput.Value);
            pictureBox1.Invalidate();
        }

        private void MapOffsetYInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.UpdateDraftOffsets((float)mapOffsetXInput.Value, (float)mapOffsetYInput.Value);
            pictureBox1.Invalidate();
        }

        private void MapMirrorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            waferController.SetDraftMirror(mapMirrorXCheckBox.Checked, mapMirrorYCheckBox.Checked);
            pictureBox1.Invalidate();
        }
    }
}


