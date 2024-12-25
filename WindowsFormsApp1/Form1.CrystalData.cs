using System.Collections.Generic;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        // Коллекция для хранения всех кристаллов на пластине
        private List<Crystal> crystals = new List<Crystal>();

        // Класс для хранения информации о каждом кристалле
        public class Crystal
        {
            public int Index { get; set; }            // Уникальный номер кристалла
            public Color Color { get; set; }          // Цвет для отрисовки
            public float RealX { get; set; }          // Реальная X координата в мм
            public float RealY { get; set; }          // Реальная Y координата в мм
            public float DisplayX { get; set; }       // X координата на экране
            public float DisplayY { get; set; }       // Y координата на экране
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
