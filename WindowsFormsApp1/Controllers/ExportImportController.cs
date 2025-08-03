using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для экспорта и импорта данных
    /// </summary>
    public class ExportImportController
    {
        private readonly Form1 form;
        private readonly WaferController waferController;
        private readonly DataExporter exporter = new DataExporter();

        public ExportImportController(Form1 form, WaferController waferController)
        {
            this.form = form;
            this.waferController = waferController;
        }

        /// <summary>
        /// Сохранить информацию о пластине
        /// </summary>
        public void SaveWaferInfo(string sizeX, string sizeY, string diameter)
        {
            try
            {
                uint.TryParse(sizeX, out var x);
                uint.TryParse(sizeY, out var y);
                uint.TryParse(diameter, out var d);

                var waferInfo = new WaferInfo
                {
                    SizeX = x,
                    SizeY = y,
                    WaferDiameter = d,
                };

                var serializer = new Serializer();
                serializer.Serialize(waferInfo);

                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Экспортировать данные
        /// </summary>
        public void ExportData()
        {
            if (CrystalManager.Instance.Crystals.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Компактный XML (*.xml)|*.xml|Детальный XML (*.xml)|*.xml|" +
                               "CSV файл (*.csv)|*.csv|JSON файл (*.json)|*.json";
            saveDialog.Title = "Экспорт данных";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var info = CreateWaferInfo();

                    switch (saveDialog.FilterIndex)
                    {
                        case 1: // Компактный XML
                            exporter.ExportToCompactXml(saveDialog.FileName, info,
                                CrystalManager.Instance.Crystals);
                            break;

                        case 2: // Детальный XML
                            var stats = waferController.GetStatistics();
                            exporter.ExportToDetailedXml(saveDialog.FileName, info,
                                CrystalManager.Instance.Crystals, stats);
                            break;

                        case 3: // CSV
                            exporter.ExportToCsv(saveDialog.FileName,
                                CrystalManager.Instance.Crystals, info);
                            break;

                        case 4: // JSON
                            exporter.ExportToJson(saveDialog.FileName, info,
                                CrystalManager.Instance.Crystals);
                            break;
                    }

                    MessageBox.Show("Данные успешно экспортированы!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Импортировать данные
        /// </summary>
        public (WaferInfo info, List<Crystal> crystals)? ImportData()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML файлы (*.xml)|*.xml|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
            openDialog.Title = "Импорт данных";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (openDialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        var crystals = exporter.ImportFromCsv(openDialog.FileName);
                        CrystalManager.Instance.Crystals.Clear();
                        CrystalManager.Instance.Crystals.AddRange(crystals);
                        return (null, crystals);
                    }
                    else
                    {
                        var result = exporter.ImportFromCompactXml(openDialog.FileName);
                        CrystalManager.Instance.Crystals.Clear();
                        CrystalManager.Instance.Crystals.AddRange(result.crystals);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return null;
        }

        /// <summary>
        /// Открыть файл
        /// </summary>
        public (WaferInfo info, List<Crystal> crystals)? OpenFile()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*";
            openDialog.Title = "Открыть файл данных";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = exporter.ImportFromCompactXml(openDialog.FileName);
                    CrystalManager.Instance.Crystals.Clear();
                    CrystalManager.Instance.Crystals.AddRange(result.crystals);

                    MessageBox.Show("Данные успешно загружены!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return null;
        }

        /// <summary>
        /// Сохранить как
        /// </summary>
        public void SaveAs()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*";
            saveDialog.Title = "Сохранить как";
            saveDialog.DefaultExt = "xml";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var info = CreateWaferInfo();
                    exporter.ExportToCompactXml(saveDialog.FileName, info,
                        CrystalManager.Instance.Crystals);

                    MessageBox.Show("Данные успешно сохранены!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private WaferInfo CreateWaferInfo()
        {
            return new WaferInfo
            {
                SizeX = (uint)waferController.CrystalWidthRaw,
                SizeY = (uint)waferController.CrystalHeightRaw,
                WaferDiameter = (uint)waferController.WaferDiameter
            };
        }
    }
}