using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для управления масштабированием и панорамированием
    /// </summary>
    public class ZoomPanController
    {
        private readonly Form1 form;

        // Параметры масштабирования
        public float ZoomFactor { get; private set; } = 1.0f;
        public PointF PanOffset { get; private set; } = new PointF(0, 0);

        // Константы
        private const float MinZoom = 0.1f;
        private const float MaxZoom = 10.0f;
        private const float ZoomStep = 0.1f;

        public ZoomPanController(Form1 form)
        {
            this.form = form;
        }

        /// <summary>
        /// Обработка прокрутки колесика мыши
        /// </summary>
        public void HandleMouseWheel(MouseEventArgs e)
        {
            PointF mousePos = new PointF(e.X, e.Y);
            float oldZoom = ZoomFactor;

            // Изменяем масштаб
            if (e.Delta > 0)
            {
                ZoomFactor = Math.Min(ZoomFactor + ZoomStep, MaxZoom);
            }
            else
            {
                ZoomFactor = Math.Max(ZoomFactor - ZoomStep, MinZoom);
            }

            // Корректируем смещение для зума относительно курсора
            if (oldZoom != ZoomFactor)
            {
                float zoomRatio = ZoomFactor / oldZoom;
                PanOffset = new PointF(
                    mousePos.X - (mousePos.X - PanOffset.X) * zoomRatio,
                    mousePos.Y - (mousePos.Y - PanOffset.Y) * zoomRatio
                );
            }
        }

        /// <summary>
        /// Изменить масштаб на заданную величину
        /// </summary>
        public void Zoom(float delta)
        {
            float centerX = form.PictureBox.Width / 2;
            float centerY = form.PictureBox.Height / 2;

            float oldZoom = ZoomFactor;
            ZoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, ZoomFactor + delta));

            // Зум относительно центра
            if (oldZoom != ZoomFactor)
            {
                float zoomRatio = ZoomFactor / oldZoom;
                PanOffset = new PointF(
                    centerX - (centerX - PanOffset.X) * zoomRatio,
                    centerY - (centerY - PanOffset.Y) * zoomRatio
                );
            }
        }

        /// <summary>
        /// Панорамирование
        /// </summary>
        public void Pan(float deltaX, float deltaY)
        {
            PanOffset = new PointF(PanOffset.X + deltaX, PanOffset.Y + deltaY);
        }

        /// <summary>
        /// Сброс масштаба и панорамирования
        /// </summary>
        public void Reset()
        {
            ZoomFactor = 1.0f;
            PanOffset = new PointF(0, 0);
        }

        /// <summary>
        /// Преобразование точки с учетом трансформаций
        /// </summary>
        public PointF TransformPoint(PointF point)
        {
            return new PointF(
                (point.X - PanOffset.X) / ZoomFactor,
                (point.Y - PanOffset.Y) / ZoomFactor
            );
        }

        /// <summary>
        /// Обратное преобразование точки
        /// </summary>
        public PointF InverseTransformPoint(PointF point)
        {
            return new PointF(
                point.X * ZoomFactor + PanOffset.X,
                point.Y * ZoomFactor + PanOffset.Y
            );
        }
    }
}