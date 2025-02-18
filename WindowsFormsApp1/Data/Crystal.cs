using System.Drawing;

namespace CrystalTable.Data
{
    public class Crystal
    {
        public int Index { get; set; }            // Уникальный номер кристалла
        public Color Color { get; set; }          // Цвет для отрисовки
        public float RealX { get; set; }          // Реальная X координата в мм
        public float RealY { get; set; }          // Реальная Y координата в мм
        public float DisplayX { get; set; }       // X координата на экране
        public float DisplayY { get; set; }       // Y координата на экране

        // Добавленные свойства для границ отображения кристалла
        public float DisplayLeft { get; set; }    // Левая граница
        public float DisplayRight { get; set; }   // Правая граница
        public float DisplayTop { get; set; }     // Верхняя граница
        public float DisplayBottom { get; set; }  // Нижняя граница
    }
}
