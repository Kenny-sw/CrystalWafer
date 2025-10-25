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
        private CheckBox checkBoxFillWafer;  // ← Добавлено из Parameters
        private ComboBox loadDataComboBox;    // ← Добавлено из Parameters
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
                Height = 380,  // ← УВЕЛИЧЕНО с 320 до 380 для полного отображения
                Padding = new Padding(10),
                BackColor = Color.FromArgb(246, 250, 246),
                AutoScroll = true  // ← Добавлено для прокрутки при необходимости
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

            // ✅ НОВЫЙ БЛОК: Тип изделия и режим схемы
            var presetPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };

            var lblPreset = new Label
            {
                Text = "Тип изделия:",
                AutoSize = true,
                Margin = new Padding(0, 5, 6, 3),
                Width = 90
            };

            loadDataComboBox = new ComboBox
            {
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 3, 0, 3)
            };
            loadDataComboBox.SelectedIndexChanged += loadDataComboBox_SelectedIndexChanged;

            checkBoxFillWafer = new CheckBox
            {
                Text = "Режим схемы",
                AutoSize = true,
                Margin = new Padding(10, 5, 0, 3)
            };
            checkBoxFillWafer.CheckedChanged += checkBoxFillWafer_CheckedChanged;

            presetPanel.Controls.Add(lblPreset);
            presetPanel.Controls.Add(loadDataComboBox);
            presetPanel.Controls.Add(checkBoxFillWafer);

            var inputsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 6,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            inputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            inputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            mapDiameterInput = CreateNumericUpDown((decimal)WaferController.MinWaferDiameter, (decimal)WaferController.MaxWaferDiameter, 1m, 0);
            mapWidthInput = CreateNumericUpDown(1m, 50000m, 1m, 0);  // ← В мкм!
            mapHeightInput = CreateNumericUpDown(1m, 50000m, 1m, 0); // ← В мкм!
            mapStreetInput = CreateNumericUpDown(0m, 10m, 0.05m, 3);
            mapOffsetXInput = CreateNumericUpDown(-200m, 200m, 0.05m, 3);
            mapOffsetYInput = CreateNumericUpDown(-200m, 200m, 0.05m, 3);

            AddLabeledControl(inputsTable, "Диаметр D, мм", mapDiameterInput, 0);
            AddLabeledControl(inputsTable, "Ширина W, мкм", mapWidthInput, 1);  // ← мкм!
            AddLabeledControl(inputsTable, "Высота H, мкм", mapHeightInput, 2); // ← мкм!
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
            mapApplyButton = CreatePrimaryButton("Применить", MapApplyButton_Click);
            mapCancelButton = CreateSecondaryButton("Отмена", MapCancelButton_Click);
            mapEditButton = CreateSecondaryButton("Редактировать", MapEditButton_Click);

            buttonsPanel.Controls.Add(mapStartButton);
            buttonsPanel.Controls.Add(mapApplyButton);
            buttonsPanel.Controls.Add(mapCancelButton);
            buttonsPanel.Controls.Add(mapEditButton);

            mapBuilderPanel.Controls.Add(buttonsPanel);
            mapBuilderPanel.Controls.Add(shiftPanel);
            mapBuilderPanel.Controls.Add(mirrorPanel);
            mapBuilderPanel.Controls.Add(inputsTable);
            mapBuilderPanel.Controls.Add(presetPanel);  // ← Добавлено
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
                    mapWidthInput.Value = ClampToNumeric(snapshot.CrystalWidthMm * 1000f, mapWidthInput);  // ← мм → мкм
                    mapHeightInput.Value = ClampToNumeric(snapshot.CrystalHeightMm * 1000f, mapHeightInput); // ← мм → мкм
                    mapStreetInput.Value = ClampToNumeric(snapshot.StreetMm, mapStreetInput);
                    mapOffsetXInput.Value = ClampToNumeric(snapshot.OffsetXMm, mapOffsetXInput);
                    mapOffsetYInput.Value = ClampToNumeric(snapshot.OffsetYMm, mapOffsetYInput);
                    mapMirrorXCheckBox.Checked = snapshot.MirrorX;
                    mapMirrorYCheckBox.Checked = snapshot.MirrorY;
                }
                else
                {
                    float defaultDiameter = waferController.WaferDiameter > 0 ? waferController.WaferDiameter : 200f;
                    float defaultWidthMicrons = waferController.CrystalWidthRaw > 0 ? waferController.CrystalWidthRaw : 100f;  // ← мкм
                    float defaultHeightMicrons = waferController.CrystalHeightRaw > 0 ? waferController.CrystalHeightRaw : 100f; // ← мкм

                    mapDiameterInput.Value = ClampToNumeric(defaultDiameter, mapDiameterInput);
                    mapWidthInput.Value = ClampToNumeric(defaultWidthMicrons, mapWidthInput);
                    mapHeightInput.Value = ClampToNumeric(defaultHeightMicrons, mapHeightInput);
                    mapStreetInput.Value = ClampToNumeric(0f, mapStreetInput);
                    mapOffsetXInput.Value = ClampToNumeric(0f, mapOffsetXInput);
                    mapOffsetYInput.Value = ClampToNumeric(0f, mapOffsetYInput);
                    mapMirrorXCheckBox.Checked = false;
                    mapMirrorYCheckBox.Checked = false;
                }

                bool editing = waferController.IsMapEditing;
                bool hasActive = waferController.HasActiveMap;

                // ✅ Режим редактирования - можно менять все
                mapDiameterInput.Enabled = editing || !hasActive;
                mapWidthInput.Enabled = editing || !hasActive;
                mapHeightInput.Enabled = editing || !hasActive;
                mapStreetInput.Enabled = editing || !hasActive;
                mapOffsetXInput.Enabled = editing || !hasActive;
                mapOffsetYInput.Enabled = editing || !hasActive;
                mapMirrorXCheckBox.Enabled = editing || !hasActive;
                mapMirrorYCheckBox.Enabled = editing || !hasActive;
                loadDataComboBox.Enabled = !editing && !hasActive;  // ← Только до создания
                checkBoxFillWafer.Enabled = true;  // ← Всегда доступно

                mapShiftLeftButton.Enabled = editing;
                mapShiftRightButton.Enabled = editing;
                mapShiftUpButton.Enabled = editing;
                mapShiftDownButton.Enabled = editing;
                mapHalfStepXButton.Enabled = editing;
                mapHalfStepYButton.Enabled = editing;
                mapSwapOrientationButton.Enabled = editing;

                mapStartButton.Enabled = !editing && !hasActive;
                mapApplyButton.Enabled = editing;
                mapCancelButton.Enabled = editing;
                mapEditButton.Enabled = !editing;  // ← ИЗМЕНЕНО: всегда доступна когда НЕ редактируем

                toolStripCreateMapButton.Enabled = !editing && !hasActive;
                toolStripEditMapButton.Enabled = !editing;  // ← ИЗМЕНЕНО: всегда доступна когда НЕ редактируем
                toolStripSavePngButton.Enabled = hasActive;

                // ✅ Обновленные статусы
                if (editing)
                {
                    mapBuilderStatusLabel.Text = "Редактируется черновик карты";
                    mapEditButton.Text = "Отменить";  // Меняем текст кнопки
                }
                else if (hasActive)
                {
                    mapBuilderStatusLabel.Text = "Активная карта готова";
                    mapEditButton.Text = "Редактировать";
                }
                else
                {
                    mapBuilderStatusLabel.Text = "Настройте параметры и нажмите 'Создать карту'";
                    mapEditButton.Text = "Настроить";  // ← НОВЫЙ текст для режима "до создания"
                }
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
            // ✅ ОБЪЕДИНЕННЫЙ ФУНКЦИОНАЛ: проверка + создание + синхронизация

            float diameterMm = (float)mapDiameterInput.Value;
            float widthMicrons = (float)mapWidthInput.Value;
            float heightMicrons = (float)mapHeightInput.Value;

            // Валидация
            if (diameterMm < WaferController.MinWaferDiameter || diameterMm > WaferController.MaxWaferDiameter)
            {
                MessageBox.Show($"Диаметр пластины должен быть от {WaferController.MinWaferDiameter} до {WaferController.MaxWaferDiameter} мм.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (widthMicrons < 1f || heightMicrons < 1f)
            {
                MessageBox.Show("Размеры кристалла должны быть больше 0.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var parameters = new WaferMapParameters
            {
                DiameterMm = diameterMm,
                CrystalWidthMm = widthMicrons / 1000f,  // ← мкм → мм
                CrystalHeightMm = heightMicrons / 1000f, // ← мкм → мм
                StreetMm = (float)mapStreetInput.Value,
                OffsetXMm = (float)mapOffsetXInput.Value,
                OffsetYMm = (float)mapOffsetYInput.Value,
                MirrorX = mapMirrorXCheckBox.Checked,
                MirrorY = mapMirrorYCheckBox.Checked,
                SwapOrientation = false
            };

            // ✅ КРИТИЧНО: Синхронизировать состояние WaferController
            waferController.CrystalWidthRaw = (uint)widthMicrons;
            waferController.CrystalHeightRaw = (uint)heightMicrons;
            waferController.WaferDiameter = diameterMm;
            waferController.SizeXtemp = waferController.CrystalWidthRaw;
            waferController.SizeYtemp = waferController.CrystalHeightRaw;
            waferController.WaferDiameterTemp = waferController.WaferDiameter;

            // Создание карты
            waferController.BeginMapCreation(parameters);
            
            // Центрирование указателя
            CenterPointer();
            zoomPanController.Reset();
            
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
            UpdateUI();
            
            // Логирование (опционально)
            System.Diagnostics.Debug.WriteLine($"Карта создана: {diameterMm}мм, {widthMicrons}×{heightMicrons} мкм, кристаллов: {Logic.CrystalManager.Instance.Crystals.Count}");
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
            // ✅ УЛУЧШЕННАЯ ЛОГИКА: работает в трех режимах
            
            if (waferController.IsMapEditing)
            {
                // Режим 1: Отменить редактирование (как Cancel)
                waferController.CancelDraftMap();
            }
            else if (waferController.HasActiveMap)
            {
                // Режим 2: Редактировать существующую карту
                waferController.BeginMapEdit();
            }
            else
            {
                // Режим 3: Войти в режим настройки ДО создания карты (как "Создать карту" но без создания)
                // Просто переводим в режим редактирования параметров
                var parameters = new WaferMapParameters
                {
                    DiameterMm = (float)mapDiameterInput.Value,
                    CrystalWidthMm = (float)mapWidthInput.Value / 1000f,
                    CrystalHeightMm = (float)mapHeightInput.Value / 1000f,
                    StreetMm = (float)mapStreetInput.Value,
                    OffsetXMm = (float)mapOffsetXInput.Value,
                    OffsetYMm = (float)mapOffsetYInput.Value,
                    MirrorX = mapMirrorXCheckBox.Checked,
                    MirrorY = mapMirrorYCheckBox.Checked,
                    SwapOrientation = false
                };
                
                waferController.BeginMapCreation(parameters);
            }
            
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
            float widthMm = (float)mapWidthInput.Value / 1000f;  // ← мкм → мм
            float heightMm = (float)mapHeightInput.Value / 1000f; // ← мкм → мм
            waferController.UpdateDraftCrystalSize(widthMm, heightMm);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private void MapHeightInput_ValueChanged(object sender, EventArgs e)
        {
            if (mapInputsSyncLock || !waferController.IsMapEditing) return;
            float widthMm = (float)mapWidthInput.Value / 1000f;  // ← мкм → мм
            float heightMm = (float)mapHeightInput.Value / 1000f; // ← мкм → мм
            waferController.UpdateDraftCrystalSize(widthMm, heightMm);
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

        // Обработчики loadDataComboBox_SelectedIndexChanged и checkBoxFillWafer_CheckedChanged
        // находятся в Form1.cs и Form1.LoadData.cs
    }
}


