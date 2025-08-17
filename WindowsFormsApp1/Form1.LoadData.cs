using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using CrystalTable.Logic;

namespace CrystalTable
{
    public partial class Form1
    {
        /// <summary>
        /// Загрузка данных из файла в comboBox
        /// </summary>
        public void LoadComboBoxData()
        {
            string filePath = Path.Combine(Application.StartupPath, "crystal_data.txt");

            if (!File.Exists(filePath))
            {
                CreateSampleDataFile(filePath);
            }

            try
            {
                loadDataComboBox.Items.Clear();
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        loadDataComboBox.Items.Add(parts[0].Trim());
                    }
                }

                if (loadDataComboBox.Items.Count > 0)
                {
                    loadDataComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Создание файла с примерами данных
        /// </summary>
        private void CreateSampleDataFile(string filePath)
        {
            try
            {
                string[] sampleData = new string[]
                {
                    "# Формат: Название : SizeX, SizeY, WaferDiameter",
                    "# SizeX и SizeY в микрометрах (целые), WaferDiameter в миллиметрах (вещественное)",
                    "",
                    "Стандартный 100мм : 5000, 5000, 100",
                    "Стандартный 150мм : 5000, 5000, 150",
                    "Стандартный 200мм : 5000, 5000, 200",
                    "Стандартный 300мм : 5000, 5000, 300",
                    "Мелкий чип 100мм : 2000, 2000, 100",
                    "Мелкий чип 150мм : 2000, 2000, 150",
                    "Крупный чип 200мм : 10000, 10000, 200",
                    "Прямоугольный чип : 8000, 4000, 150",
                    "Память DRAM : 6000, 3000, 300",
                    "Процессор : 15000, 15000, 300",
                    "Датчик CMOS : 7000, 5000, 200",
                    "Power MOSFET : 12000, 12000, 150"
                };

                File.WriteAllLines(filePath, sampleData);

                MessageBox.Show("Создан файл с примерами конфигураций кристаллов.",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла примеров: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Установка значений из выбранного элемента comboBox
        /// </summary>
        public void SetFieldsFromComboBox()
        {
            if (loadDataComboBox.SelectedItem == null)
                return;

            string filePath = Path.Combine(Application.StartupPath, "crystal_data.txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл с данными не найден.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(':');
                    if (parts.Length == 2 && parts[0].Trim() == loadDataComboBox.SelectedItem.ToString())
                    {
                        string[] parameters = parts[1].Split(',');

                        if (parameters.Length == 3)
                        {
                            // Парсим как: SizeX/SizeY — uint (мкм), Diameter — float (мм)
                            if (!uint.TryParse(parameters[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out uint newSizeXum) ||
                                !uint.TryParse(parameters[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out uint newSizeYum) ||
                                !float.TryParse(parameters[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float newDiameterMm))
                            {
                                MessageBox.Show("Некорректный формат записи. Ожидается: SizeX(uint), SizeY(uint), WaferDiameter(float).",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Сохраняем старые (для истории), как float — так и было
                            float oldSizeX = waferController.CrystalWidthRaw;   // uint -> float (неявно)
                            float oldSizeY = waferController.CrystalHeightRaw;  // uint -> float
                            float oldDiameter = waferController.WaferDiameter;  // float

                            // Устанавливаем в поля UI
                            SizeX.Text = newSizeXum.ToString(CultureInfo.InvariantCulture);
                            SizeY.Text = newSizeYum.ToString(CultureInfo.InvariantCulture);
                            WaferDiameter.Text = newDiameterMm.ToString(CultureInfo.InvariantCulture);

                            // Обновляем контроллер (явные приведения где нужно)
                            waferController.CrystalWidthRaw = newSizeXum;
                            waferController.CrystalHeightRaw = newSizeYum;
                            waferController.WaferDiameter = newDiameterMm;

                            // Добавляем в историю, если значения изменились
                            if (oldSizeX != newSizeXum || oldSizeY != newSizeYum || Math.Abs(oldDiameter - newDiameterMm) > 1e-6f)
                            {
                                commandHistory.ExecuteCommand(
                                    new ChangeWaferParametersCommand(
                                        oldSizeX, oldSizeY, oldDiameter,
                                        newSizeXum, newSizeYum, newDiameterMm,
                                        (x, y, d) =>
                                        {
                                            // x,y здесь float — приводим к uint для контроллера
                                            SizeX.Text = ((uint)x).ToString(CultureInfo.InvariantCulture);
                                            SizeY.Text = ((uint)y).ToString(CultureInfo.InvariantCulture);
                                            WaferDiameter.Text = d.ToString(CultureInfo.InvariantCulture);
                                            waferController.CrystalWidthRaw = (uint)x;
                                            waferController.CrystalHeightRaw = (uint)y;
                                            waferController.WaferDiameter = d;
                                        },
                                        () => UpdateUI()
                                    )
                                );
                            }

                            UpdateUI();
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загрузка конфигурации по умолчанию
        /// </summary>
        public void LoadDefaultConfiguration()
        {
            try
            {
                LoadComboBoxData();

                string lastConfigPath = Path.Combine(Application.StartupPath, "last_config.txt");
                if (File.Exists(lastConfigPath))
                {
                    string lastConfig = File.ReadAllText(lastConfigPath).Trim();

                    for (int i = 0; i < loadDataComboBox.Items.Count; i++)
                    {
                        if (loadDataComboBox.Items[i].ToString() == lastConfig)
                        {
                            loadDataComboBox.SelectedIndex = i;
                            SetFieldsFromComboBox();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки конфигурации: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохранение последней использованной конфигурации
        /// </summary>
        public void SaveLastConfiguration()
        {
            try
            {
                if (loadDataComboBox.SelectedItem != null)
                {
                    string lastConfigPath = Path.Combine(Application.StartupPath, "last_config.txt");
                    File.WriteAllText(lastConfigPath, loadDataComboBox.SelectedItem.ToString());
                }
            }
            catch
            {
                // Игнорируем ошибки при сохранении
            }
        }
    }
}
