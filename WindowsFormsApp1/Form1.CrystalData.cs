using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        private List<Crystal> crystals = new List<Crystal>(); //создание объекта коллекции


        // Структура для представления данных о кристалле
        public class Crystal
        {
            public int Index { get; set; }            // Индекс кристалла
            public Color Color { get; set; }          // Цвет кристалла для PictureBox
            public float ContactHeight { get; set; }  // Высота контактирования
            public TimeOnCrystal { get; set; }        // Время, когда было произведено измерение
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

    // Метод для изменения статуса кристалла
    /*  public void ChangeCrystalStatus(int index, int newStatus)
      {
          // Проверяем, что индекс находится в допустимом диапазоне
          if (index >= 0 && index < crystals.Count)
          {
              // Обновляем статус кристалла
              crystals[index].Status = newStatus;
              // Устанавливаем цвет в зависимости от статуса (0 - зеленый, 1 - красный)
              crystals[index].Color = (newStatus == 0) ? Color.Green : Color.Red;
              // Перерисовываем PictureBox, чтобы изменения стали видны на экране
              pictureBox1.Invalidate();
          } */

}
}



/*
  Возможные доработки
Выбор кристаллов: Можно добавить функциональность для выбора конкретного кристалла мышью, чтобы изменить его тип или цвет.
Фильтрация: Для отображения только хороших или плохих кристаллов можно добавить фильтрацию кристаллов по типу.
Хранение состояния: Мы можем сохранять информацию о кристаллах (их типах, индексах) в файл, чтобы в дальнейшем восстанавливать состояние формы.
Графическая информация: Можно добавить отображение индексов кристаллов на экране для отладки или анализа.
*/