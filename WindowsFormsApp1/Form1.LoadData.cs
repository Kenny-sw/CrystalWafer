using System;
using System.IO;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1
    {
        // Метод для загрузки данных из файла в comboBox1
        public void LoadComboBoxData()
        {
            string filePath = "crystal_data.txt"; // Укажите путь к вашему файлу

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл с данными не найден. Убедитесь, что файл 'crystal_data.txt' существует.");
                return;
            }

            try
            {
                // Читаем файл построчно
                string[] lines = File.ReadAllLines(filePath);

                // Заполняем comboBox1
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':'); // Разделяем строку по ":"
                    if (parts.Length == 2)
                    {
                        comboBox1.Items.Add(parts[0].Trim()); // Добавляем название набора в comboBox1
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
            }
        }

        // Метод для установки значений в SizeX, SizeY и WaferDiameter на основе выбранного элемента
        public void SetFieldsFromComboBox()
        {
            string filePath = "crystal_data.txt"; // Укажите путь к вашему файлу

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл с данными не найден.");
                return;
            }

            try
            {
                // Читаем файл построчно
                string[] lines = File.ReadAllLines(filePath);

                // Ищем выбранный элемент в файле
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2 && parts[0].Trim() == comboBox1.SelectedItem.ToString())
                    {
                        string[] parameters = parts[1].Split(',');

                        if (parameters.Length == 3)
                        {
                            // Заполняем поля данными
                            SizeX.Text = parameters[0].Trim();
                            SizeY.Text = parameters[1].Trim();
                            WaferDiameter.Text = parameters[2].Trim();
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке данных: {ex.Message}");
            }
        }
    }
}
