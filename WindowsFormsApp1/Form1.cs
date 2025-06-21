using CrystalTable.Data;
using CrystalTable.Logic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Основные параметры формы
        private float waferDiameter = 100f;     // Диаметр пластины в миллиметрах
        private float scaleFactor = 4.0f;       // Коэффициент масштабирования
        private const float MinWaferDiameter = 50f;  // Минимальный диаметр пластины
        private const float MaxWaferDiameter = 300f; // Максимальный диаметр пластины
        private int selectedCrystalIndex = -1;   // Индекс выбранного кристалла
        private int nextCrystalIndex = 1;        // Счетчик для кристаллов
        private float crystalWidthRaw;           // Ширина кристалла в микрометрах
        private float crystalHeightRaw;          // Высота кристалла в микрометрах

        // Глобальные переменные для отслеживания значений
        private float SizeXtemp = 0;
        private float SizeYtemp = 0;
        private float WaferDiameterTemp = 0;
        private bool isDataChanged;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Paint += pictureBox1_Paint;
            this.Resize += new EventHandler(Form1_Resize);

            // Устанавливаем событие Validated для TextBox
            SizeX.Validated += SizeX_Validated;
            SizeY.Validated += SizeY_Validated;
            WaferDiameter.Validated += WaferDiameter_Validated;

            // Инициализация меток
            UpdateLabels();
        }

        // Метод для обновления меток
        private void UpdateLabels()
        {
            SizeXtempLabel.Text = SizeXtemp.ToString();
            SizeYtempLabel.Text = SizeYtemp.ToString();
            WaferDiameterTempLabel.Text = WaferDiameterTemp.ToString();
        }

        // Метод для перерасчета
  //    private void RecalculateWafer() //для будущей переделки
  //    {
  //        if (isDataChanged)
  //        {
  //            // Здесь выполняется перерасчет на основе SizeXtemp, SizeYtemp, WaferDiameterTemp
  //            pictureBox1.Invalidate(); // Перерисовываем pictureBox1
  //            isDataChanged = false; // Сбрасываем флаг после перерасчета
  //        }
  //    }

        // Обработчик события изменения размеров формы
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        // Метод для обновления визуализации пластины
        private void UpdateWaferVisualization()
        {
            pictureBox1.Refresh();
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


        private void SizeX_Validated(object sender, EventArgs e)
        {
            if (!float.TryParse(SizeX.Text, out float value) || value <= 0)
            {
                SizeX.BackColor = Color.Red; // Подсвечиваем красным при неверном вводе
                SizeXtemp = 0; // Сбрасываем значение
            }
            else
            {
                SizeX.BackColor = Color.GreenYellow; // Возвращаем белый цвет
                if (value != SizeXtemp) // Сравниваем с предыдущим значением
                {
                    SizeXtemp = value; // Обновляем временную переменную
                    isDataChanged = true; // Устанавливаем флаг изменения
                }
            }

            SizeXtempLabel.Text = SizeXtemp.ToString(); // Обновляем метку
            
        }

        private void SizeY_Validated(object sender, EventArgs e)
        {
            if (!float.TryParse(SizeY.Text, out float value) || value <= 0)
            {
                SizeY.BackColor = Color.Red; // Подсвечиваем красным при неверном вводе
                SizeYtemp = 0; // Сбрасываем значение
            }
            else
            {
                SizeY.BackColor = Color.GreenYellow; // Возвращаем белый цвет
                if (value != SizeYtemp) // Сравниваем с предыдущим значением
                {
                    SizeYtemp = value; // Обновляем временную переменную
                    isDataChanged = true; // Устанавливаем флаг изменения
                }
            }

            SizeYtempLabel.Text = SizeYtemp.ToString(); // Обновляем метку
             
        }

        private void WaferDiameter_Validated(object sender, EventArgs e)
        {
            if (!float.TryParse(WaferDiameter.Text, out float value) || value < MinWaferDiameter || value > MaxWaferDiameter)
            {
                WaferDiameter.BackColor = Color.Red; // Подсвечиваем красным при неверном вводе
                WaferDiameterTemp = 0; // Сбрасываем значение
            }
            else
            {
                WaferDiameter.BackColor = Color.GreenYellow; // Возвращаем белый цвет
                if (value != WaferDiameterTemp) // Сравниваем с предыдущим значением
                {
                    WaferDiameterTemp = value; // Обновляем временную переменную
                    isDataChanged = true; // Устанавливаем флаг изменения
                }
            }

            WaferDiameterTempLabel.Text = WaferDiameterTemp.ToString(); // Обновляем метку
            
        }


        private void fToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Пустой обработчик
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Пустой обработчик
        }
    }
}