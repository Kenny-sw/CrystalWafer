using System.Windows.Forms;
using System;
using System.Linq;
using CrystalTable.Logic;

namespace CrystalTable
{
    public partial class Form1
    {
        // ===== ОБРАБОТЧИКИ ВВОДА ДЛЯ ТЕКСТОВЫХ ПОЛЕЙ =====

        private void SizeX_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(SizeX.Text))
            {
                SizeX.Focus();
                SizeX.SelectionStart = 0;
            }
        }

        private void SizeY_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(SizeY.Text))
            {
                SizeY.Focus();
                SizeY.SelectionStart = 0;
            }
        }

        private void WaferDiameter_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(WaferDiameter.Text))
            {
                WaferDiameter.Focus();
                WaferDiameter.SelectionStart = 0;
            }
        }

        private void SizeX_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void SizeY_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void WaferDiameter_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        // ===== ОБРАБОТЧИКИ КНОПОК ПАНЕЛИ ИНСТРУМЕНТОВ =====

        /// <summary>
        /// Обработчик кнопки Отменить
        /// </summary>
        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (commandHistory.CanUndo())
            {
                commandHistory.Undo();
                pictureBox1.Invalidate();
                UpdateToolbarState();
            }
        }

        /// <summary>
        /// Обработчик кнопки Повторить
        /// </summary>
        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (commandHistory.CanRedo())
            {
                commandHistory.Redo();
                pictureBox1.Invalidate();
                UpdateToolbarState();
            }
        }

        /// <summary>
        /// Обработчик кнопки Экспорт
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportData();
        }

        /// <summary>
        /// Обработчик кнопки Импорт
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportData();
        }

        /// <summary>
        /// Обработчик кнопки предпросмотра маршрута
        /// </summary>
        private void btnRoutePreview_Click(object sender, EventArgs e)
        {
            ToggleRoutePreview();
            showRoutePreview = btnRoutePreview.Checked;
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Обработчик кнопки статистики
        /// </summary>
        private void btnStatistics_Click(object sender, EventArgs e)
        {
            ShowStatistics();
        }

        /// <summary>
        /// Обработчики кнопок масштабирования
        /// </summary>
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            zoomFactor = Math.Min(zoomFactor + ZoomStep * 2, MaxZoom);
            UpdateZoomCenter();
            pictureBox1.Invalidate();
            UpdateStatusBar();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            zoomFactor = Math.Max(zoomFactor - ZoomStep * 2, MinZoom);
            UpdateZoomCenter();
            pictureBox1.Invalidate();
            UpdateStatusBar();
        }

        private void btnZoomReset_Click(object sender, EventArgs e)
        {
            zoomFactor = 1.0f;
            panOffset = new System.Drawing.PointF(0, 0);
            pictureBox1.Invalidate();
            UpdateStatusBar();
        }

        // ===== ОБРАБОТЧИКИ ПУНКТОВ МЕНЮ =====

        // Меню Файл
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Создать новую пластину? Все несохраненные данные будут потеряны.",
                "Новая пластина", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Сброс всех параметров
                SizeX.Text = "";
                SizeY.Text = "";
                WaferDiameter.Text = "";
                selectedCrystals.Clear();
                commandHistory.Clear();
                zoomFactor = 1.0f;
                panOffset = new System.Drawing.PointF(0, 0);
                CrystalManager.Instance.Crystals.Clear();
                pictureBox1.Invalidate();
                UpdateStatusBar();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*";
            openDialog.Title = "Открыть файл данных";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var exporter = new DataExporter();
                    var (info, crystals) = exporter.ImportFromCompactXml(openDialog.FileName);

                    // Применяем загруженные данные
                    SizeX.Text = info.SizeX.ToString();
                    SizeY.Text = info.SizeY.ToString();
                    WaferDiameter.Text = info.WaferDiameter.ToString();

                    CrystalManager.Instance.Crystals.Clear();
                    CrystalManager.Instance.Crystals.AddRange(crystals);

                    pictureBox1.Invalidate();
                    UpdateStatusBar();

                    MessageBox.Show("Данные успешно загружены!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveButton_Click(sender, e);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*";
            saveDialog.Title = "Сохранить как";
            saveDialog.DefaultExt = "xml";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                SaveToFile(saveDialog.FileName);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportData();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportData();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // Меню Правка
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUndo_Click(sender, e);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnRedo_Click(sender, e);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Выделяем все кристаллы
            selectedCrystals.Clear();
            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                selectedCrystals.Add(crystal.Index);
            }
            UpdateSelectionLabel();
            pictureBox1.Invalidate();
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedCrystals.Clear();
            UpdateSelectionLabel();
            pictureBox1.Invalidate();
        }

        // Меню Вид
        private void showRouteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showRoutePreview = showRouteToolStripMenuItem.Checked;
            btnRoutePreview.Checked = showRoutePreview;
            ToggleRoutePreview();
            pictureBox1.Invalidate();
        }

        private void showStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowStatistics();
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnZoomIn_Click(sender, e);
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnZoomOut_Click(sender, e);
        }

        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnZoomReset_Click(sender, e);
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

        /// <summary>
        /// Обновляет состояние кнопок панели инструментов
        /// </summary>
        private void UpdateToolbarState()
        {
            btnUndo.Enabled = commandHistory.CanUndo();
            btnRedo.Enabled = commandHistory.CanRedo();
            undoToolStripMenuItem.Enabled = commandHistory.CanUndo();
            redoToolStripMenuItem.Enabled = commandHistory.CanRedo();

            // Обновляем подсказки с описанием операций
            if (commandHistory.CanUndo())
            {
                string undoText = commandHistory.GetUndoDescription();
                btnUndo.ToolTipText = $"Отменить: {undoText}";
                undoToolStripMenuItem.Text = $"Отменить {undoText}";
            }
            else
            {
                btnUndo.ToolTipText = "Отменить (Ctrl+Z)";
                undoToolStripMenuItem.Text = "Отменить";
            }

            if (commandHistory.CanRedo())
            {
                string redoText = commandHistory.GetRedoDescription();
                btnRedo.ToolTipText = $"Повторить: {redoText}";
                redoToolStripMenuItem.Text = $"Повторить {redoText}";
            }
            else
            {
                btnRedo.ToolTipText = "Повторить (Ctrl+Y)";
                redoToolStripMenuItem.Text = "Повторить";
            }
        }

        /// <summary>
        /// Центрирует масштабирование
        /// </summary>
        private void UpdateZoomCenter()
        {
            // Корректируем смещение при изменении масштаба от центра
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;

            float oldZoom = zoomFactor;
            float zoomRatio = zoomFactor / oldZoom;

            panOffset.X = centerX - (centerX - panOffset.X) * zoomRatio;
            panOffset.Y = centerY - (centerY - panOffset.Y) * zoomRatio;
        }

        /// <summary>
        /// Экспортирует данные
        /// </summary>
        private void ExportData()
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
                    var exporter = new DataExporter();
                    var info = new Data.WaferInfo
                    {
                        SizeX = uint.Parse(SizeX.Text),
                        SizeY = uint.Parse(SizeY.Text),
                        WaferDiameter = uint.Parse(WaferDiameter.Text)
                    };

                    switch (saveDialog.FilterIndex)
                    {
                        case 1: // Компактный XML
                            exporter.ExportToCompactXml(saveDialog.FileName, info,
                                CrystalManager.Instance.Crystals);
                            break;

                        case 2: // Детальный XML
                            var stats = new WaferStatistics(CrystalManager.Instance.Crystals, waferDiameter);
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
        /// Импортирует данные
        /// </summary>
        private void ImportData()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML файлы (*.xml)|*.xml|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
            openDialog.Title = "Импорт данных";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var exporter = new DataExporter();

                    if (openDialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        var crystals = exporter.ImportFromCsv(openDialog.FileName);
                        CrystalManager.Instance.Crystals.Clear();
                        CrystalManager.Instance.Crystals.AddRange(crystals);
                    }
                    else
                    {
                        var (info, crystals) = exporter.ImportFromCompactXml(openDialog.FileName);

                        SizeX.Text = info.SizeX.ToString();
                        SizeY.Text = info.SizeY.ToString();
                        WaferDiameter.Text = info.WaferDiameter.ToString();

                        CrystalManager.Instance.Crystals.Clear();
                        CrystalManager.Instance.Crystals.AddRange(crystals);
                    }

                    pictureBox1.Invalidate();
                    UpdateStatusBar();

                    MessageBox.Show("Данные успешно импортированы!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Показывает окно статистики
        /// </summary>
        private void ShowStatistics()
        {
            if (CrystalManager.Instance.Crystals.Count == 0)
            {
                MessageBox.Show("Нет данных для отображения статистики!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var stats = new WaferStatistics(CrystalManager.Instance.Crystals, waferDiameter);
            var report = stats.GenerateFullReport(crystalWidthRaw / 1000f, crystalHeightRaw / 1000f);

            // Создаем форму для отображения статистики
            var statsForm = new StatisticsForm(report, stats, selectedCrystals);
            statsForm.ShowDialog();
        }

        /// <summary>
        /// Сохраняет данные в файл
        /// </summary>
        private void SaveToFile(string fileName)
        {
            try
            {
                var info = new Data.WaferInfo
                {
                    SizeX = uint.Parse(SizeX.Text),
                    SizeY = uint.Parse(SizeY.Text),
                    WaferDiameter = uint.Parse(WaferDiameter.Text)
                };

                var exporter = new DataExporter();
                exporter.ExportToCompactXml(fileName, info, CrystalManager.Instance.Crystals);

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
}