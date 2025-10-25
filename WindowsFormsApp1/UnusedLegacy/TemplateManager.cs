using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Models;

namespace CrystalTable
{
    public class MapTemplate
    {
        public string Name { get; set; }
        public MapMetadata Metadata { get; set; }
        public WaferMapParameters Parameters { get; set; }

        public MapTemplate Clone()
        {
            return new MapTemplate
            {
                Name = Name,
                Metadata = Metadata == null ? null : new MapMetadata(Metadata.LotNumber, Metadata.WaferNumber, Metadata.Note),
                Parameters = Parameters?.Clone()
            };
        }
    }

    public class TemplateManager
    {
        private readonly Dictionary<string, MapTemplate> templates = new Dictionary<string, MapTemplate>();
        private readonly ComboBox templatesComboBox;
        private readonly string templatesPath;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        // Текущий черновик (при создании/редактировании карты)
        private MapTemplate currentDraft;
        private bool draftDirty = false;

        // Состояние текущей партии
        private bool sessionActive = false;
        private string currentLotName;
        private int nextWaferNumber = 1;

        // События для UI
        public event Action<MapTemplate> DraftUpdated;
        public event Action TemplatesChanged;
        public event Action<bool> SessionStateChanged; // arg: sessionActive
        public event Action<int> WaferNumberChanged;

        private const string TemplatesFileName = "wafer_templates.json";

        public TemplateManager(ComboBox comboBox)
        {
            templatesComboBox = comboBox;
            templatesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplatesFileName);
            LoadTemplates();
            UpdateComboBox();
        }

        public void SaveTemplate(MapTemplate template)
        {
            templates[template.Name] = template.Clone();
            UpdateComboBox();
            TemplatesChanged?.Invoke();
            SaveTemplates();
        }

        public MapTemplate GetTemplate(string name)
        {
            return templates.TryGetValue(name, out var template) ? template.Clone() : null;
        }

        public IReadOnlyList<string> GetTemplateNames()
        {
            return templates.Keys
                .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void DeleteTemplate(string name)
        {
            if (templates.Remove(name))
            {
                UpdateComboBox();
            }
        }

        private void UpdateComboBox()
        {
            templatesComboBox.Items.Clear();
            templatesComboBox.Items.AddRange(templates.Keys.OrderBy(k => k).Cast<object>().ToArray());
            TemplatesChanged?.Invoke();
        }

        public void LoadTemplates()
        {
            try
            {
                if (File.Exists(templatesPath))
                {
                    var json = File.ReadAllText(templatesPath);
                    var loadedTemplates = serializer.Deserialize<Dictionary<string, MapTemplate>>(json)
                        ?? new Dictionary<string, MapTemplate>();

                    templates.Clear();
                    foreach (var kvp in loadedTemplates)
                    {
                        templates[kvp.Key] = kvp.Value;
                    }

                    UpdateComboBox();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке шаблонов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void SaveTemplates()
        {
            try
            {
                var json = serializer.Serialize(templates);
                File.WriteAllText(templatesPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении шаблонов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // --- Draft / session management ---
        public MapTemplate CurrentDraft => currentDraft == null ? null : currentDraft.Clone();
        public bool IsDraftDirty => draftDirty;
        public bool IsSessionActive => sessionActive;
        public string CurrentLotName => currentLotName;
        public int NextWaferNumber => nextWaferNumber;

        // Создать черновик на основе шаблона или с нуля
        public void CreateDraft(string draftName, string templateName = null)
        {
            if (!string.IsNullOrEmpty(templateName) && templates.TryGetValue(templateName, out var tmpl))
            {
                currentDraft = tmpl.Clone();
                currentDraft.Name = draftName;
            }
            else
            {
                currentDraft = new MapTemplate
                {
                    Name = draftName,
                    Metadata = new MapMetadata(draftName, "", ""),
                    Parameters = new WaferMapParameters()
                };
            }
            draftDirty = false;
            DraftUpdated?.Invoke(CurrentDraft);
        }

        // Обновление параметров черновика (UI будет вызывать при изменениях)
        public void UpdateDraftParameters(WaferMapParameters parameters)
        {
            if (currentDraft == null) return;
            currentDraft.Parameters = parameters;
            draftDirty = true;
            DraftUpdated?.Invoke(CurrentDraft);
        }

        public void CancelDraft()
        {
            currentDraft = null;
            draftDirty = false;
            DraftUpdated?.Invoke(null);
        }

        // Сохраняем черновик как шаблон (новый или обновляем существующий)
        public void FinalizeDraft(bool saveAsTemplate = true)
        {
            if (currentDraft == null) return;
            if (saveAsTemplate)
            {
                SaveTemplate(currentDraft);
            }
            // открываем сессию партии
            sessionActive = true;
            currentLotName = currentDraft.Name;
            nextWaferNumber = 1;
            SessionStateChanged?.Invoke(sessionActive);
            WaferNumberChanged?.Invoke(nextWaferNumber);
            draftDirty = false;
        }

        // Редактирование существующего шаблона
        public void EditTemplate(string templateName)
        {
            if (templates.TryGetValue(templateName, out var tmpl))
            {
                currentDraft = tmpl.Clone();
                draftDirty = false;
                DraftUpdated?.Invoke(CurrentDraft);
            }
        }

        // Подтвердить сохранение изменений в редактировании (обновляем шаблон или создаём новый)
        public void SaveDraftAsTemplate(string templateName)
        {
            if (currentDraft == null) return;
            currentDraft.Name = templateName;
            SaveTemplate(currentDraft);
            draftDirty = false;
        }

        // Работа с пластинами
        public string GenerateNextWaferNumberString()
        {
            return nextWaferNumber.ToString("D3");
        }

        public int StartNewWafer()
        {
            if (!sessionActive) throw new InvalidOperationException("Session is not active");
            var number = nextWaferNumber;
            nextWaferNumber++;
            WaferNumberChanged?.Invoke(nextWaferNumber);
            return number;
        }

        public void FinishWafer(/* можно добавить параметры для сохранения данных */)
        {
            // Здесь будет логика сохранения данных пластины в хранилище
            // Для простоты — пока не реализуем физическое сохранение; вызываем событие, если нужно
        }

        public void ArchiveLot()
        {
            // Завершение партии: деактивируем сессию и сбрасываем счетчик
            sessionActive = false;
            currentLotName = null;
            nextWaferNumber = 1;
            SessionStateChanged?.Invoke(sessionActive);
            WaferNumberChanged?.Invoke(nextWaferNumber);
            // Черновик остаётся сохранённым как шаблон
        }
    }
}
