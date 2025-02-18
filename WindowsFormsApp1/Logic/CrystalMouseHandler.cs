using System;
using System.Windows.Forms;
using System.Drawing;

namespace CrystalTable.Logic
{
    public class CrystalMouseHandler
    {
        // Элементы управления для обновления информации о курсоре и выбранном кристалле
        private PictureBox _pictureBox;
        private Label _labelX;
        private Label _labelY;
        private Label _labelIndex;

        // Делегаты для получения актуальных размеров кристалла (с учётом масштабирования)
        private Func<float> _getDisplayCrystalWidth;
        private Func<float> _getDisplayCrystalHeight;

        // Свойство для хранения выбранного индекса кристалла
        public int SelectedCrystalIndex { get; private set; } = -1;

        // Конструктор, через который передаются зависимости
        public CrystalMouseHandler(PictureBox pictureBox, Label labelX, Label labelY, Label labelIndex,
            Func<float> getDisplayCrystalWidth, Func<float> getDisplayCrystalHeight)
        {
            _pictureBox = pictureBox;
            _labelX = labelX;
            _labelY = labelY;
            _labelIndex = labelIndex;
            _getDisplayCrystalWidth = getDisplayCrystalWidth;
            _getDisplayCrystalHeight = getDisplayCrystalHeight;
        }

        // Метод для обработки перемещения мыши
        public void HandleMouseMove(MouseEventArgs e)
        {
            // Обновляем метки с текущими координатами мыши
            _labelX.Text = $"X: {e.X}";
            _labelY.Text = $"Y: {e.Y}";

            float displayCrystalWidth = _getDisplayCrystalWidth();
            float displayCrystalHeight = _getDisplayCrystalHeight();

            // Перебираем кристаллы и проверяем, находится ли мышь над одним из них
            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                float left = crystal.DisplayX - displayCrystalWidth / 2;
                float right = crystal.DisplayX + displayCrystalWidth / 2;
                float top = crystal.DisplayY - displayCrystalHeight / 2;
                float bottom = crystal.DisplayY + displayCrystalHeight / 2;

                if (e.X >= left && e.X <= right && e.Y >= top && e.Y <= bottom)
                {
                    _labelIndex.Text = $"Индекс кристалла: {crystal.Index}";
                    return;
                }
            }

            _labelIndex.Text = "Индекс кристалла: -";
        }

        // Метод для обработки нажатия кнопки мыши
        public void HandleMouseDown(MouseEventArgs e)
        {
            float displayCrystalWidth = _getDisplayCrystalWidth();
            float displayCrystalHeight = _getDisplayCrystalHeight();

            // Проверяем кристаллы в обратном порядке, чтобы выбрать верхний при перекрытии
            for (int i = CrystalManager.Instance.Crystals.Count - 1; i >= 0; i--)
            {
                var crystal = CrystalManager.Instance.Crystals[i];

                float left = crystal.DisplayX - displayCrystalWidth / 2;
                float right = crystal.DisplayX + displayCrystalWidth / 2;
                float top = crystal.DisplayY - displayCrystalHeight / 2;
                float bottom = crystal.DisplayY + displayCrystalHeight / 2;

                if (e.X >= left && e.X <= right && e.Y >= top && e.Y <= bottom)
                {
                    SelectedCrystalIndex = crystal.Index;
                    _pictureBox.Invalidate(); // Обновляем отрисовку для выделения кристалла
                    return;
                }
            }

            SelectedCrystalIndex = -1;
            _pictureBox.Invalidate();
        }
    }
}
