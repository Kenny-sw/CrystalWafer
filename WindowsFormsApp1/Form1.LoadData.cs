using System;
using System.IO;
using System.Windows.Forms;
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
                    "# SizeX и SizeY в микрометрах, WaferDiameter в миллиметрах",
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
                            // Сохраняем старые значения
                            float oldSizeX = waferController.CrystalWidthRaw;
                            float oldSizeY = waferController.CrystalHeightRaw;
                            float oldDiameter = waferController.WaferDiameter;

                            // Устанавливаем новые значения
                            SizeX.Text = parameters[0].Trim();
                            SizeY.Text = parameters[1].Trim();
                            WaferDiameter.Text = parameters[2].Trim();

                            float newSizeX = float.Parse(parameters[0].Trim());
                            float newSizeY = float.Parse(parameters[1].Trim());
                            float newDiameter = float.Parse(parameters[2].Trim());

                            // Обновляем контроллер
                            waferController.CrystalWidthRaw = newSizeX;
                            waferController.CrystalHeightRaw = newSizeY;
                            waferController.WaferDiameter = newDiameter;

                            // Добавляем в историю, если значения изменились
                            if (oldSizeX != newSizeX || oldSizeY != newSizeY || oldDiameter != newDiameter)
                            {
                                commandHistory.ExecuteCommand(
                                    new ChangeWaferParametersCommand(
                                        oldSizeX, oldSizeY, oldDiameter,
                                        newSizeX, newSizeY, newDiameter,
                                        (x, y, d) => {
                                            SizeX.Text = x.ToString();
                                            SizeY.Text = y.ToString();
                                            WaferDiameter.Text = d.ToString();
                                            waferController.CrystalWidthRaw = x;
                                            waferController.CrystalHeightRaw = y;
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