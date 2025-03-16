// Главный файл формы Form1, в котором содержатся основные параметры и обработка событий Resize и инициализация компонента

using CrystalTable.Data;
using CrystalTable.Logic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Основные параметры формы
        private float waferDiameter = 100f;     // Диаметр пластины в миллиметрах
        private float scaleFactor = 4.0f;       // Коэффициент масштабирования для отображения
        private const float MinWaferDiameter = 50f;  // Минимально допустимый диаметр пластины
        private const float MaxWaferDiameter = 300f; // Максимально допустимый диаметр пластины
        private int selectedCrystalIndex = -1;   // Индекс выбранного кристалла (-1 = не выбран)
        private int nextCrystalIndex = 1;        // Счетчик для нумерации кристаллов
        private float crystalWidthRaw;           // Ширина кристалла в микрометрах
        private float crystalHeightRaw;          // Высота кристалла в микрометрах


        public Form1()
        {
            InitializeComponent(); // Инициализация компонентов формы
            pictureBox1.Paint += pictureBox1_Paint; // Привязываем событие Paint к обработчику pictureBox1_Paint для отрисовки
            this.Resize += new EventHandler(Form1_Resize); // Привязываем событие Resize для обновления визуализации при изменении размеров формы
        }


        // Обработчик события изменения размеров формы
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовываем PictureBox при изменении размера формы
        }

        // Метод для обновления визуализации пластины
        private void UpdateWaferVisualization()
        {
            pictureBox1.Refresh(); // Обновляем содержимое PictureBox
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            uint.TryParse(SizeX.Text, out var sizeX);
            uint.TryParse(SizeY.Text, out var sizeY);
            uint.TryParse(WaferDiameter.Text, out var waferDiameter);
            var waferInfo = new WaferInfo
            {
                SizeX = sizeX,
                SizeY = sizeY,
                WaferDiameter = waferDiameter,
            };
            var serializer = new Serializer();
            serializer.Serialize(waferInfo);
        }

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
            
            if (!float.TryParse(SizeX.Text, out float value) || value <= 0) // Если не число или отрицательное
            {
                SizeX.BackColor = Color.Red; // Подсвечиваем красным
            }
            else
            {
                SizeX.BackColor = Color.White; // Возвращаем белый цвет, если всё верно
            }
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void SizeY_TextChanged(object sender, EventArgs e)
        {
            

            if (!float.TryParse(SizeY.Text, out float value) || value <= 0) // Если не число или отрицательное
            {
                SizeX.BackColor = Color.Red; // Подсвечиваем красным
            }
            else
            {
                SizeX.BackColor = Color.White; // Возвращаем белый цвет, если всё верно
            }
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void WaferDiameter_TextChanged(object sender, EventArgs e)
        {
            
            if (!float.TryParse(WaferDiameter.Text, out float value) || value <= 0) // Если не число или отрицательное
            {
                SizeX.BackColor = Color.Red; // Подсвечиваем красным
            }
            else
            {
                SizeX.BackColor = Color.White; // Возвращаем белый цвет, если всё верно
            }
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void Create_Click(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void fToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
        }
    }
}
