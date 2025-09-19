using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Управляет сохранением и загрузкой настроек пластины и карт кристаллов.
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

        public void SaveWaferInfo(string sizeX, string sizeY, string diameter)
        {
            try
            {
                uint.TryParse(sizeX, out var parsedSizeX);
                uint.TryParse(sizeY, out var parsedSizeY);
                uint.TryParse(diameter, out var parsedDiameter);

                var waferInfo = BuildCurrentWaferInfo();
                waferInfo.SizeX = parsedSizeX;
                waferInfo.SizeY = parsedSizeY;
                waferInfo.WaferDiameter = parsedDiameter;

                var serializer = new Serializer();
                serializer.Serialize(waferInfo);

                MessageBox.Show("Параметры успешно сохранены!", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ExportData()
        {
            if (CrystalManager.Instance.Crystals.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "Компактный XML (*.xml)|*.xml|Подробный XML (*.xml)|*.xml|" +
                         "CSV файл (*.csv)|*.csv|JSON файл (*.json)|*.json",
                Title = "Экспортировать карту"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                var info = BuildCurrentWaferInfo();

                switch (saveDialog.FilterIndex)
                {
                    case 1:
                        exporter.ExportToCompactXml(saveDialog.FileName, info, CrystalManager.Instance.Crystals);
                        break;
                    case 2:
                        var stats = waferController.GetStatistics();
                        exporter.ExportToDetailedXml(saveDialog.FileName, info, CrystalManager.Instance.Crystals, stats);
                        break;
                    case 3:
                        exporter.ExportToCsv(saveDialog.FileName, CrystalManager.Instance.Crystals, info);
                        break;
                    case 4:
                        exporter.ExportToJson(saveDialog.FileName, info, CrystalManager.Instance.Crystals);
                        break;
                }

                MessageBox.Show("Экспорт успешно завершён!", "Экспорт",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public (WaferInfo info, List<Crystal> crystals)? ImportData()
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "XML файлы (*.xml)|*.xml|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                Title = "Импорт карты"
            };

            if (openDialog.ShowDialog() != DialogResult.OK)
            {
                return null;
            }

            try
            {
                if (openDialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    var crystals = exporter.ImportFromCsv(openDialog.FileName);
                    CrystalManager.Instance.Crystals.Clear();
                    CrystalManager.Instance.Crystals.AddRange(crystals);
                    return (null, crystals);
                }

                var result = exporter.ImportFromCompactXml(openDialog.FileName);
                CrystalManager.Instance.Crystals.Clear();
                CrystalManager.Instance.Crystals.AddRange(result.crystals);
                ApplyWaferInfo(result.info);
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public (WaferInfo info, List<Crystal> crystals)? OpenFile()
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
                Title = "Открыть карту"
            };

            if (openDialog.ShowDialog() != DialogResult.OK)
            {
                return null;
            }

            try
            {
                var result = exporter.ImportFromCompactXml(openDialog.FileName);
                CrystalManager.Instance.Crystals.Clear();
                CrystalManager.Instance.Crystals.AddRange(result.crystals);
                ApplyWaferInfo(result.info);

                MessageBox.Show("Карта успешно загружена!", "Загрузка",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public void SaveAs()
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
                Title = "Сохранить карту как",
                DefaultExt = "xml"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                var info = BuildCurrentWaferInfo();
                exporter.ExportToCompactXml(saveDialog.FileName, info, CrystalManager.Instance.Crystals);

                MessageBox.Show("Карта сохранена!", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private WaferInfo BuildCurrentWaferInfo()
        {
            var zoom = form.ZoomPanController;
            var pointer = form.GetPointerMm();

            var info = new WaferInfo
            {
                SizeX = (uint)waferController.CrystalWidthRaw,
                SizeY = (uint)waferController.CrystalHeightRaw,
                WaferDiameter = (uint)waferController.WaferDiameter,
                HasCalibration = waferController.IsCalibrationReady(),
                StepXmm = waferController.StepXmmOrDefault,
                StepYmm = waferController.StepYmmOrDefault,
                RotationAngleDeg = waferController.RotationAngleDeg,
                ZoomFactor = zoom.ZoomFactor,
                PanOffsetX = zoom.PanOffset.X,
                PanOffsetY = zoom.PanOffset.Y,
                PointerXmm = pointer.X,
                PointerYmm = pointer.Y
            };

            if (waferController.HasFirstRef)
            {
                info.FirstReferenceX = waferController.FirstRefMm.X;
                info.FirstReferenceY = waferController.FirstRefMm.Y;
            }

            if (waferController.HasLastRef)
            {
                info.LastReferenceX = waferController.LastRefMm.X;
                info.LastReferenceY = waferController.LastRefMm.Y;
            }

            return info;
        }

        private void ApplyWaferInfo(WaferInfo info)
        {
            if (info == null)
            {
                return;
            }

            waferController.CrystalWidthRaw = info.SizeX;
            waferController.CrystalHeightRaw = info.SizeY;
            waferController.WaferDiameter = info.WaferDiameter;
            waferController.SetSteps(info.StepXmm, info.StepYmm);

            if (info.HasCalibration)
            {
                waferController.SetFirstReference(info.FirstReferenceX, info.FirstReferenceY);
                waferController.SetLastReference(info.LastReferenceX, info.LastReferenceY);
            }
            else
            {
                waferController.ClearReferences();
            }

            waferController.BuildCrystalsCached();

            form.ZoomPanController.SetState(info.ZoomFactor, new PointF(info.PanOffsetX, info.PanOffsetY));
            form.SetPointerMm(info.PointerXmm, info.PointerYmm);
        }
    }
}
