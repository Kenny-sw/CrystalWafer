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
        // Структура для представления данных о кристалле
        public class Crystal
        {
            public int Index { get; set; }   // Уникальный индекс кристалла
            public Color Color { get; set; }  // Цвет кристалла для визуализации
            public int Status { get; set; }   // Статус кристалла (0 - хороший, 1 - плохой)
            public float X { get; set; }      // Координата X кристалла на PictureBox
            public float Y { get; set; }      // Координата Y кристалла на PictureBox
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