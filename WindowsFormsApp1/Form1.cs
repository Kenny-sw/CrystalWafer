// Главный файл формы Form1, в котором содержатся основные параметры и обработка событий Resize и инициализация компонента

using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
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

        
    }
}
