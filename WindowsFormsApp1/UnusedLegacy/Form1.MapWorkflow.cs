using System;
using System.Drawing;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Models;

namespace CrystalTable
{
    public partial class Form1
    {
        private GroupBox mapWorkflowGroup;

        private void InitializeMapWorkflowUi()
        {
            mapWorkflowGroup = new GroupBox
            {
                Name = "mapWorkflowGroup",
                Text = "Рабочий процесс карты",
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false
            };

            createMapButton = CreateWorkflowButton("Создать карту", CreateMapButton_Click);
            editMapButton = CreateWorkflowButton("Редактировать карту", EditMapButton_Click);
            startWaferButton = CreateWorkflowButton("Новая пластина", StartWaferButton_Click);
            completeWaferButton = CreateWorkflowButton("Завершить пластину", CompleteWaferButton_Click);
            archivePartyButton = CreateWorkflowButton("Архивировать партию", ArchivePartyButton_Click);

            buttonsPanel.Controls.Add(createMapButton);
            buttonsPanel.Controls.Add(editMapButton);
            buttonsPanel.Controls.Add(startWaferButton);
            buttonsPanel.Controls.Add(completeWaferButton);
            buttonsPanel.Controls.Add(archivePartyButton);

            mapWorkflowGroup.Controls.Add(buttonsPanel);

            rightPanel.Controls.Add(mapWorkflowGroup);
            rightPanel.Controls.SetChildIndex(mapWorkflowGroup, 0);

            UpdateWorkflowUi();
        }

        private Button CreateWorkflowButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                Width = 270,
                Height = 30,
                Margin = new Padding(0, 0, 0, 6),
                UseVisualStyleBackColor = true
            };
            button.Click += handler;
            return button;
        }

        private void SetWorkflowStage(MapWorkflowStage stage)
        {
            workflowStage = stage;
            UpdateWorkflowUi();
        }

        private void UpdateWorkflowUi()
        {
            bool hasDraft = waferController?.IsMapEditing ?? false;
            bool hasActiveMap = waferController?.HasActiveMap ?? false;
            bool hasWafer = currentWaferMetadata != null;

            if (createMapButton != null)
            {
                createMapButton.Visible = workflowStage == MapWorkflowStage.Idle;
            }

            if (editMapButton != null)
            {
                editMapButton.Visible = workflowStage == MapWorkflowStage.Ready && hasActiveMap && !hasDraft;
                editMapButton.Enabled = hasActiveMap && !hasDraft;
            }

            if (startWaferButton != null)
            {
                startWaferButton.Visible = workflowStage == MapWorkflowStage.Ready && !hasWafer;
                startWaferButton.Enabled = hasActiveMap && !hasWafer;
            }

            if (completeWaferButton != null)
            {
                completeWaferButton.Visible = workflowStage == MapWorkflowStage.WaferActive;
                completeWaferButton.Enabled = hasWafer;
            }

            if (archivePartyButton != null)
            {
                archivePartyButton.Visible = workflowStage == MapWorkflowStage.Ready && hasActiveMap;
                archivePartyButton.Enabled = hasActiveMap;
            }

            if (mapBuilderPanel != null)
            {
                mapBuilderPanel.Visible = hasDraft;
            }

            if (mapStartButton != null)
            {
                mapStartButton.Visible = false;
            }

            if (mapEditButton != null)
            {
                mapEditButton.Visible = false;
            }

            if (mapApplyButton != null)
            {
                mapApplyButton.Enabled = hasDraft && draftDirty;
            }

            if (mapCancelButton != null)
            {
                mapCancelButton.Enabled = hasDraft;
            }

            if (mapBuilderStatusLabel != null)
            {
                if (hasDraft)
                {
                    mapBuilderStatusLabel.Text = draftDirty ? "Черновик изменён" : "Черновик без изменений";
                }
                else if (hasActiveMap)
                {
                    mapBuilderStatusLabel.Text = $"Текущая карта: {currentMapMetadata?.LotNumber ?? "-"}";
                }
                else
                {
                    mapBuilderStatusLabel.Text = "Карта не создана";
                }
            }
        }

        private void BeginMapCreationWithParameters(WaferMapParameters parameters)
        {
            var effectiveParameters = parameters?.Clone() ?? BuildParametersFromInputs();
            waferController.BeginMapCreation(effectiveParameters);
            draftDirty = false;
            SetWorkflowStage(MapWorkflowStage.Draft);
            pictureBox1.Invalidate();
            SyncMapBuilderUi();
        }

        private WaferMapParameters BuildParametersFromInputs()
        {
            if (mapDiameterInput == null)
            {
                float diameter = waferController?.WaferDiameterTemp > 0 ? waferController.WaferDiameterTemp : 200f;
                float width = waferController?.SizeXtemp > 0 ? waferController.SizeXtemp / 1000f : 10f;
                float height = waferController?.SizeYtemp > 0 ? waferController.SizeYtemp / 1000f : 10f;
                return new WaferMapParameters
                {
                    DiameterMm = diameter,
                    CrystalWidthMm = width,
                    CrystalHeightMm = height
                };
            }

            return new WaferMapParameters
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
        }

        private void MarkDraftDirty()
        {
            if (!(waferController?.IsMapEditing ?? false))
            {
                return;
            }

            if (draftDirty)
            {
                return;
            }

            draftDirty = true;
            UpdateWorkflowUi();
        }

        private void PersistTemplate(MapMetadata metadata, WaferMapParameters parameters, string templateName)
        {
            if (templateManager == null || metadata == null || parameters == null)
            {
                return;
            }

            var name = !string.IsNullOrWhiteSpace(templateName)
                ? templateName.Trim()
                : metadata.LotNumber?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var template = new MapTemplate
            {
                Name = name,
                Metadata = new MapMetadata(metadata.LotNumber, metadata.WaferNumber, metadata.Note),
                Parameters = parameters.Clone()
            };

            templateManager.SaveTemplate(template);
            SelectTemplateInCombo(name);
        }

        private static bool MetadataEquals(MapMetadata first, MapMetadata second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            return string.Equals(first.LotNumber, second.LotNumber, StringComparison.OrdinalIgnoreCase)
                && string.Equals(first.WaferNumber, second.WaferNumber, StringComparison.OrdinalIgnoreCase)
                && string.Equals(first.Note, second.Note, StringComparison.OrdinalIgnoreCase);
        }

        private void SelectTemplateInCombo(string templateName)
        {
            if (loadDataComboBox == null || string.IsNullOrWhiteSpace(templateName))
            {
                return;
            }

            for (int i = 0; i < loadDataComboBox.Items.Count; i++)
            {
                if (string.Equals(loadDataComboBox.Items[i]?.ToString(), templateName, StringComparison.OrdinalIgnoreCase))
                {
                    loadDataComboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private void CreateMapButton_Click(object sender, EventArgs e)
        {
            using var dialog = new MapMetadataDialog(templateManager, null, loadDataComboBox?.SelectedItem?.ToString());
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            draftMapMetadata = dialog.Metadata;
            draftTemplateName = string.IsNullOrWhiteSpace(currentTemplateName)
                ? draftMapMetadata?.LotNumber?.Trim()
                : currentTemplateName?.Trim();

            var template = dialog.SelectedTemplate;
            BeginMapCreationWithParameters(template?.Parameters);
            draftDirty = true;
            UpdateWorkflowUi();
        }

        private void EditMapButton_Click(object sender, EventArgs e)
        {
            if (!waferController.HasActiveMap)
            {
                MessageBox.Show(
                    this,
                    "Нет активной карты для редактирования.",
                    "Редактирование карты",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    this,
                    "Перейти в режим редактирования текущей карты? Черновик будет создан на основе сохранённой карты.",
                    "Редактирование карты",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            using var dialog = new MapMetadataDialog(templateManager, currentMapMetadata, currentTemplateName);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            draftMapMetadata = dialog.Metadata;
            draftTemplateName = draftMapMetadata?.LotNumber?.Trim();

            var snapshot = waferController.GetActiveMapSnapshot();
            BeginMapCreationWithParameters(snapshot);
            draftDirty = !MetadataEquals(currentMapMetadata, draftMapMetadata);
            UpdateWorkflowUi();
        }

        private void StartWaferButton_Click(object sender, EventArgs e)
        {
            if (workflowStage != MapWorkflowStage.Ready || currentMapMetadata == null)
            {
                return;
            }

            string waferNumber = nextWaferSequence.ToString("D3");
            using var dialog = new WaferRunDialog(currentMapMetadata, waferNumber);

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            currentWaferMetadata = dialog.Metadata;
            nextWaferSequence++;
            SetWorkflowStage(MapWorkflowStage.WaferActive);
            UpdateUI();
        }

        private void CompleteWaferButton_Click(object sender, EventArgs e)
        {
            if (currentWaferMetadata == null)
            {
                return;
            }

            currentWaferMetadata = null;
            SetWorkflowStage(MapWorkflowStage.Ready);
            UpdateUI();
        }

        private void ArchivePartyButton_Click(object sender, EventArgs e)
        {
            if (currentMapMetadata == null)
            {
                return;
            }

            if (MessageBox.Show(
                    this,
                    "Архивировать партию? Черновик и активная пластина будут очищены.",
                    "Архивирование партии",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            currentMapMetadata = null;
            currentTemplateName = null;
            currentWaferMetadata = null;
            draftMapMetadata = null;
            draftTemplateName = null;
            draftDirty = false;
            nextWaferSequence = 1;

            waferController.CreateNewWafer();
            mouseController.ClearSelection();
            commandHistory.Clear();

            SetWorkflowStage(MapWorkflowStage.Idle);
            UpdateUI();
        }
    }
}
