using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    /// <summary>
    /// Менеджер для работы с шаблонами и партиями
    /// </summary>
    public class WaferTemplateManager
    {
        private static readonly string TemplatesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrystalTable",
            "Templates");

        private static readonly string BatchesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrystalTable",
            "Batches");

        public WaferTemplateManager()
        {
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(TemplatesDirectory);
            Directory.CreateDirectory(BatchesDirectory);
        }

        #region Template Management

        public List<WaferTemplate> LoadAllTemplates()
        {
            var templates = new List<WaferTemplate>();
            var files = Directory.GetFiles(TemplatesDirectory, "*.xml");

            foreach (var file in files)
            {
                try
                {
                    var template = LoadTemplate(file);
                    if (template != null)
                    {
                        templates.Add(template);
                    }
                }
                catch
                {
                    // Пропускаем поврежденные файлы
                }
            }

            return templates.OrderBy(t => t.TemplateName).ToList();
        }

        public WaferTemplate LoadTemplateByName(string templateName)
        {
            var fileName = GetSafeFileName(templateName) + ".xml";
            var filePath = Path.Combine(TemplatesDirectory, fileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            return LoadTemplate(filePath);
        }

        private WaferTemplate LoadTemplate(string filePath)
        {
            var serializer = new XmlSerializer(typeof(WaferTemplate));
            using (var reader = new StreamReader(filePath))
            {
                return (WaferTemplate)serializer.Deserialize(reader);
            }
        }

        public void SaveTemplate(WaferTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            template.ModifiedDate = DateTime.Now;
            var fileName = GetSafeFileName(template.TemplateName) + ".xml";
            var filePath = Path.Combine(TemplatesDirectory, fileName);

            var serializer = new XmlSerializer(typeof(WaferTemplate));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, template);
            }
        }

        public void DeleteTemplate(string templateName)
        {
            var fileName = GetSafeFileName(templateName) + ".xml";
            var filePath = Path.Combine(TemplatesDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool TemplateExists(string templateName)
        {
            var fileName = GetSafeFileName(templateName) + ".xml";
            var filePath = Path.Combine(TemplatesDirectory, fileName);
            return File.Exists(filePath);
        }

        #endregion

        #region Batch Management

        public List<WaferBatch> LoadAllBatches()
        {
            var batches = new List<WaferBatch>();
            var files = Directory.GetFiles(BatchesDirectory, "*.xml");

            foreach (var file in files)
            {
                try
                {
                    var batch = LoadBatch(file);
                    if (batch != null)
                    {
                        batches.Add(batch);
                    }
                }
                catch
                {
                    // Пропускаем поврежденные файлы
                }
            }

            return batches.OrderByDescending(b => b.CreatedDate).ToList();
        }

        public WaferBatch LoadBatchByName(string batchName)
        {
            var fileName = GetSafeFileName(batchName) + ".xml";
            var filePath = Path.Combine(BatchesDirectory, fileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            return LoadBatch(filePath);
        }

        private WaferBatch LoadBatch(string filePath)
        {
            var serializer = new XmlSerializer(typeof(WaferBatch));
            using (var reader = new StreamReader(filePath))
            {
                return (WaferBatch)serializer.Deserialize(reader);
            }
        }

        public void SaveBatch(WaferBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            batch.ModifiedDate = DateTime.Now;
            var fileName = GetSafeFileName(batch.BatchName) + ".xml";
            var filePath = Path.Combine(BatchesDirectory, fileName);

            var serializer = new XmlSerializer(typeof(WaferBatch));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, batch);
            }
        }

        public void ArchiveBatch(string batchName)
        {
            var batch = LoadBatchByName(batchName);
            if (batch != null)
            {
                batch.IsArchived = true;
                SaveBatch(batch);
            }
        }

        #endregion

        #region Helper Methods

        private string GetSafeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public string GenerateUniqueBatchName(string baseName = "Партия")
        {
            var batches = LoadAllBatches();
            int counter = 1;

            while (batches.Any(b => b.BatchName == $"{baseName}_{counter:D3}"))
            {
                counter++;
            }

            return $"{baseName}_{counter:D3}";
        }

        public string GenerateUniqueWaferName(string batchName, int waferNumber)
        {
            return $"{batchName}_Пластина_{waferNumber:D3}";
        }

        #endregion
    }
}
