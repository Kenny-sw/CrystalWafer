using System.Collections.Generic;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        private List<Crystal> crystals = new List<Crystal>(); // Создание объекта коллекции

        // Структура для представления данных о кристалле
        public class Crystal
        {
            public int Index { get; set; }            // Индекс кристалла
            public Color Color { get; set; }           // Цвет кристалла для PictureBox
            public float RealX { get; set; }           // Реальная координата X (в физическом масштабе)
            public float RealY { get; set; }           // Реальная координата Y (в физическом масштабе)
            public float DisplayX { get; set; }        // Масштабированная координата X (для отображения)
            public float DisplayY { get; set; }        // Масштабированная координата Y (для отображения)
        }

        public void SelectCrystal(int index)
        {
            if (index >= 0 && index < crystals.Count)
            {
                selectedCrystalIndex = index; // Устанавливаем индекс выбранного кристалла
                pictureBox1.Invalidate(); // Перерисовываем PictureBox для отображения выделения
            }
        }
    }
}
