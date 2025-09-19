using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrystalTable.Controllers
{
    public class ZoomPanController
    {
        private readonly Form1 form;

        public float ZoomFactor { get; private set; } = 1.0f;
        public PointF PanOffset { get; private set; } = new PointF(0, 0);

        private const float MinZoom = 0.1f;
        private const float MaxZoom = 10.0f;
        private const float ZoomStep = 0.1f;

        public ZoomPanController(Form1 form)
        {
            this.form = form;
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
            PointF mousePos = new PointF(e.X, e.Y);
            float oldZoom = ZoomFactor;

            if (e.Delta > 0)
            {
                ZoomFactor = Math.Min(ZoomFactor + ZoomStep, MaxZoom);
            }
            else
            {
                ZoomFactor = Math.Max(ZoomFactor - ZoomStep, MinZoom);
            }

            if (oldZoom != ZoomFactor)
            {
                float newZoom = ZoomFactor;
                PanOffset = new PointF(
                    PanOffset.X + mousePos.X * (1 / newZoom - 1 / oldZoom),
                    PanOffset.Y + mousePos.Y * (1 / newZoom - 1 / oldZoom)
                );
            }
        }

        public void Zoom(float delta)
        {
            float centerX = form.PictureBox.Width / 2;
            float centerY = form.PictureBox.Height / 2;
            PointF centerPos = new PointF(centerX, centerY);

            float oldZoom = ZoomFactor;
            ZoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, ZoomFactor + delta));

            if (oldZoom != ZoomFactor)
            {
                float newZoom = ZoomFactor;
                PanOffset = new PointF(
                    PanOffset.X + centerPos.X * (1 / newZoom - 1 / oldZoom),
                    PanOffset.Y + centerPos.Y * (1 / newZoom - 1 / oldZoom)
                );
            }
        }

        public void Pan(float deltaX, float deltaY)
        {
            PanOffset = new PointF(
                PanOffset.X + deltaX / ZoomFactor,
                PanOffset.Y + deltaY / ZoomFactor
            );
        }

        public void Reset()
        {
            ZoomFactor = 1.0f;
            PanOffset = new PointF(0, 0);
        }

        public void SetState(float zoomFactor, PointF panOffset)
        {
            if (zoomFactor <= 0f)
            {
                ZoomFactor = 1.0f;
            }
            else
            {
                ZoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, zoomFactor));
            }

            PanOffset = panOffset;
        }

        public PointF TransformPoint(PointF point)
        {
            return new PointF(
                (point.X / ZoomFactor) - PanOffset.X,
                (point.Y / ZoomFactor) - PanOffset.Y
            );
        }

        public PointF InverseTransformPoint(PointF point)
        {
            return new PointF(
                (point.X + PanOffset.X) * ZoomFactor,
                (point.Y + PanOffset.Y) * ZoomFactor
            );
        }
    }
}
