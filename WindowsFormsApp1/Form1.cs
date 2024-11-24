// Главный файл формы Form1, в котором содержатся основные параметры и обработка событий Resize и инициализация компонента

using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Основные параметры формы
        private float waferDiameter = 100f; // Диаметр пластины в мм
        private float scaleFactor = 4.0f;   // Коэффициент увеличения масштаба отображения
        private const float MinWaferDiameter = 50f;  // Минимальный диаметр пластины
        private const float MaxWaferDiameter = 300f; // Максимальный диаметр пластины
        private int selectedCrystalIndex = -1; // Индекс выбранного кристалла (-1 значит, что ничего не выбрано)
        private int nextCrystalIndex = 1; // Счетчик для отслеживания индексации кристаллов



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
