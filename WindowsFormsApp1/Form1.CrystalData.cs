using System.Collections.Generic;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable
{
    public partial class Form1
    {
        // Класс для хранения информации о каждом кристалле


        public void SelectCrystal(int index)
        {
            if (index >= 0 && index < CrystalManager.Instance.Crystals.Count)
            {
                selectedCrystalIndex = index; // Устанавливаем индекс выбранного кристалла
                pictureBox1.Invalidate(); // Перерисовываем PictureBox для отображения выделения
            }
        }
    }
}
