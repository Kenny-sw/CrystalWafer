using System;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Dialogs;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для управления workflow создания и редактирования карт
    /// </summary>
    public class WorkflowController
    {
        private readonly WaferTemplateManager templateManager;
        private WorkflowState currentState;
        private WaferBatch currentBatch;
        private WaferTemplate currentTemplate;
        private WaferTemplate draftTemplate;
        private int currentWaferNumber;

        public event EventHandler<WorkflowState> StateChanged;

        public WorkflowState CurrentState => currentState;
        public WaferBatch CurrentBatch => currentBatch;
        public WaferTemplate CurrentTemplate => currentTemplate;
        public bool IsDraftMode => currentState == WorkflowState.DraftEditing || currentState == WorkflowState.MapEditing;
        public bool HasActiveMap => currentTemplate != null && currentState != WorkflowState.Initial;

        public WorkflowController()
        {
            templateManager = new WaferTemplateManager();
            currentState = WorkflowState.Initial;
        }

        #region State Management

        private void ChangeState(WorkflowState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                StateChanged?.Invoke(this, currentState);
            }
        }

        #endregion

        #region Create New Map

        public bool StartNewMap(Form parentForm)
        {
            var dialog = new CreateMapDialog(templateManager, 
                templateManager.GenerateUniqueBatchName());

            if (dialog.ShowDialog(parentForm) != DialogResult.OK)
            {
                return false;
            }

            return CreateMapFromDialog(dialog);
        }

        public bool StartNewMapWithTemplate(Form parentForm, WaferTemplate prefilledTemplate)
        {
            if (prefilledTemplate == null)
            {
                return StartNewMap(parentForm);
            }

            // Создаем новую партию с автоматическим именем
            currentBatch = new WaferBatch
            {
                BatchName = templateManager.GenerateUniqueBatchName(),
                Notes = $"Создано из параметров: {prefilledTemplate.CrystalWidthMm}x{prefilledTemplate.CrystalHeightMm}мм",
                WaferCount = 0
            };

            // Используем предзаполненный шаблон как черновик
            draftTemplate = prefilledTemplate.Clone();
            draftTemplate.TemplateName = currentBatch.BatchName;
            currentBatch.TemplateName = draftTemplate.TemplateName;

            ChangeState(WorkflowState.DraftEditing);
            return true;
        }

        private bool CreateMapFromDialog(CreateMapDialog dialog)
        {
            if (string.IsNullOrWhiteSpace(dialog.BatchName))
            {
                MessageBox.Show("Введите название партии.", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Создаем новую партию
            currentBatch = new WaferBatch
            {
                BatchName = dialog.BatchName,
                Notes = dialog.Notes,
                WaferCount = 0
            };

            // Загружаем или создаем шаблон
            if (!dialog.IsNewTemplate)
            {
                var template = templateManager.LoadTemplateByName(dialog.SelectedTemplate);
                if (template != null)
                {
                    draftTemplate = template.Clone();
                    draftTemplate.TemplateName = dialog.BatchName;
                    currentBatch.TemplateName = dialog.SelectedTemplate;
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить шаблон.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                // Создаем новый шаблон с параметрами по умолчанию
                draftTemplate = new WaferTemplate
                {
                    TemplateName = dialog.BatchName,
                    DiameterMm = 200f,
                    CrystalWidthMm = 10f,
                    CrystalHeightMm = 10f,
                    StreetMm = 0f,
                    OffsetXMm = 0f,
                    OffsetYMm = 0f,
                    MirrorX = false,
                    MirrorY = false,
                    EdgeExclusionMm = 0f
                };
            }

            ChangeState(WorkflowState.DraftEditing);
            return true;
        }

        #endregion

        #region Draft Management

        public WaferTemplate GetDraftTemplate()
        {
            return draftTemplate;
        }

        public void UpdateDraft(WaferTemplate updatedTemplate)
        {
            if (IsDraftMode)
            {
                draftTemplate = updatedTemplate;
            }
        }

        public bool SaveDraft()
        {
            if (!IsDraftMode || draftTemplate == null)
            {
                return false;
            }

            try
            {
                // Сохраняем шаблон
                currentTemplate = draftTemplate;
                templateManager.SaveTemplate(currentTemplate);

                // Обновляем партию
                if (currentBatch != null)
                {
                    currentBatch.TemplateName = currentTemplate.TemplateName;
                    templateManager.SaveBatch(currentBatch);
                }

                draftTemplate = null;
                ChangeState(WorkflowState.MapReady);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void CancelDraft()
        {
            draftTemplate = null;

            if (currentTemplate != null)
            {
                // Возврат к готовой карте
                ChangeState(WorkflowState.MapReady);
            }
            else
            {
                // Возврат в начальное состояние
                currentBatch = null;
                ChangeState(WorkflowState.Initial);
            }
        }

        #endregion

        #region Edit Existing Map

        public bool StartEditMap()
        {
            if (currentTemplate == null || currentState != WorkflowState.MapReady)
            {
                return false;
            }

            var result = MessageBox.Show(
                "Вы уверены, что хотите редактировать карту?\nВсе текущие данные пластин будут сохранены, но карта будет изменена.",
                "Редактирование карты",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return false;
            }

            draftTemplate = currentTemplate.Clone();
            ChangeState(WorkflowState.MapEditing);
            return true;
        }

        #endregion

        #region Wafer Processing

        public bool StartNewWafer()
        {
            if (currentState != WorkflowState.MapReady || currentBatch == null)
            {
                return false;
            }

            currentWaferNumber = currentBatch.WaferCount + 1;
            ChangeState(WorkflowState.WaferProcessing);
            return true;
        }

        public bool FinishWafer()
        {
            if (currentState != WorkflowState.WaferProcessing || currentBatch == null)
            {
                return false;
            }

            try
            {
                currentBatch.WaferCount++;
                templateManager.SaveBatch(currentBatch);
                ChangeState(WorkflowState.MapReady);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении пластины: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public string GetCurrentWaferName()
        {
            if (currentBatch == null)
            {
                return string.Empty;
            }

            return templateManager.GenerateUniqueWaferName(currentBatch.BatchName, currentWaferNumber);
        }

        #endregion

        #region Batch Management

        public bool ArchiveBatch()
        {
            if (currentBatch == null)
            {
                return false;
            }

            var result = MessageBox.Show(
                $"Архивировать партию '{currentBatch.BatchName}'?\nОбработано пластин: {currentBatch.WaferCount}",
                "Архивирование партии",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return false;
            }

            try
            {
                templateManager.ArchiveBatch(currentBatch.BatchName);
                Reset();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при архивировании: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Reset()
        {
            currentBatch = null;
            currentTemplate = null;
            draftTemplate = null;
            currentWaferNumber = 0;
            ChangeState(WorkflowState.Initial);
        }

        #endregion

        #region Template Access

        public System.Collections.Generic.List<WaferTemplate> GetAllTemplates()
        {
            return templateManager.LoadAllTemplates();
        }

        public WaferTemplate LoadTemplate(string templateName)
        {
            return templateManager.LoadTemplateByName(templateName);
        }

        #endregion
    }
}
