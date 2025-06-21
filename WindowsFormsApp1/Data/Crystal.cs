using System.Drawing;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    public class Crystal
    {
        public int Index { get; set; }            // Уникальный номер кристалла
        // Цвет кристалла. System.Drawing.Color не сериализуется по умолчанию,
        // поэтому основное свойство помечено как XmlIgnore, а для сохранения
        // используется дублирующее свойство ColorArgb.
        [XmlIgnore]
        public Color Color { get; set; }          // Цвет для отрисовки

        // Служебное свойство для сериализации цвета в формате ARGB.
        [XmlElement("Color")]
        public int ColorArgb
        {
            get => Color.ToArgb();
            set => Color = Color.FromArgb(value);
        }
        public float RealX { get; set; }          // Реальная X координата в мм
        public float RealY { get; set; }          // Реальная Y координата в мм
        // Координаты на экране не имеют смысла при сохранении и будут восстановлены
        // при следующем отображении, поэтому исключаем их из сериализации.
        [XmlIgnore]
        public float DisplayX { get; set; }       // X координата на экране
        [XmlIgnore]
        public float DisplayY { get; set; }       // Y координата на экране

        // Добавленные свойства для границ отображения кристалла
        [XmlIgnore]
        public float DisplayLeft { get; set; }    // Левая граница
        [XmlIgnore]
        public float DisplayRight { get; set; }   // Правая граница
        [XmlIgnore]
        public float DisplayTop { get; set; }     // Верхняя граница
        [XmlIgnore]
        public float DisplayBottom { get; set; }  // Нижняя граница
    }
}
