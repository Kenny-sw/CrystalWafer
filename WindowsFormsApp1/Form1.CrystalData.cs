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
            public Color Color { get; set; }          // Цвет кристалла для PictureBox
            public float ContactHeight { get; set; }  // Высота контактирования
                                                      //  public DateTime TimeOnCrystal { get; set; } // Время, когда было произведено измерение
            public float X { get; set; }              // Координата X кристалла на PictureBox
            public float Y { get; set; }              // Координата Y кристалла на PictureBox
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
