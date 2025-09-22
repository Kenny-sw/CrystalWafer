using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    public class MouseController
    {
        private readonly Form1 form;
        private readonly WaferController waferController;

        public HashSet<int> SelectedCrystals { get; private set; } = new HashSet<int>();
        private int selectedCrystalIndex = -1;

        private Rectangle selectionRectangle;
        private bool isSelecting = false;
        private Point selectionStart;

        private bool isCtrlPressed = false;

        private bool isPanning = false;
        private Point lastMousePosition;

        public MouseController(Form1 form, WaferController waferController)
        {
            this.form = form;
            this.waferController = waferController;
        }

        public void HandleMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = true;
                lastMousePosition = e.Location;
                form.PictureBox.Cursor = Cursors.Hand;
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                var transformedPoint = TransformMousePoint(e.Location);
                bool hitCrystal = false;

                for (int i = CrystalManager.Instance.Crystals.Count - 1; i >= 0; i--)
                {
                    var crystal = CrystalManager.Instance.Crystals[i];

                    if (IsPointInCrystal(transformedPoint, crystal))
                    {
                        hitCrystal = true;
                        HandleCrystalSelection(crystal);
                        return;
                    }
                }

                if (!hitCrystal && !isCtrlPressed)
                {
                    isSelecting = true;
                    selectionStart = e.Location;
                    selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                    SelectedCrystals.Clear();
                    form.UpdateUI();
                }
            }
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            // form.CoordinatesLabel.Text = $"X: {e.X}, Y: {e.Y}"; // Эта строка закомментирована, так как UIController обновляет координаты

            if (isPanning && e.Button == MouseButtons.Middle)
            {
                HandlePanning(e);
                return;
            }

            if (isSelecting)
            {
                UpdateSelection(e);
                return;
            }

            ShowCrystalInfo(e);
        }

        public void HandleMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = false;
                form.PictureBox.Cursor = Cursors.Default;
            }
            else if (e.Button == MouseButtons.Left && isSelecting)
            {
                isSelecting = false;
                form.UpdateUI();
            }
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                isCtrlPressed = true;
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (!e.Control)
            {
                isCtrlPressed = false;
            }
        }

        public void SelectAll(List<Crystal> crystals)
        {
            SelectedCrystals.Clear();
            foreach (var crystal in crystals)
            {
                SelectedCrystals.Add(crystal.Index);
            }
        }

        public void ClearSelection()
        {
            SelectedCrystals.Clear();
            selectedCrystalIndex = -1;
        }

        public Rectangle GetSelectionRectangle()
        {
            return isSelecting ? selectionRectangle : Rectangle.Empty;
        }

        public bool IsSelecting => isSelecting;

        public int SelectedCrystalIndex => selectedCrystalIndex;

        private PointF TransformMousePoint(Point mousePoint)
        {
            var zoom = form.ZoomPanController;
            return zoom.TransformPoint(new PointF(mousePoint.X, mousePoint.Y));
        }

        private bool IsPointInCrystal(PointF point, Crystal crystal)
        {
            return point.X >= crystal.DisplayLeft &&
                   point.X <= crystal.DisplayRight &&
                   point.Y >= crystal.DisplayTop &&
                   point.Y <= crystal.DisplayBottom;
        }

        private void HandleCrystalSelection(Crystal crystal)
        {
            if (isCtrlPressed)
            {
                if (SelectedCrystals.Contains(crystal.Index))
                {
                    SelectedCrystals.Remove(crystal.Index);
                }
                else
                {
                    SelectedCrystals.Add(crystal.Index);
                }
                form.UpdateUI();
            }
            else
            {
                form.CommandHistory.ExecuteCommand(
                    new SelectCrystalCommand(
                        selectedCrystalIndex,
                        crystal.Index,
                        (index) => {
                            selectedCrystalIndex = index;
                            SelectedCrystals.Clear();
                            if (index >= 0) 
                            {
                                SelectedCrystals.Add(index);
                            }
                        },
                        () => form.UpdateUI()
                    )
                );
            }
        }

        private void HandlePanning(MouseEventArgs e)
        {
            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;

            form.ZoomPanController.Pan(deltaX, deltaY);
            lastMousePosition = e.Location;
            form.PictureBox.Invalidate();
        }

        private void UpdateSelection(MouseEventArgs e)
        {
            int screenLeft = Math.Min(selectionStart.X, e.X);
            int screenTop = Math.Min(selectionStart.Y, e.Y);
            int screenRight = Math.Max(selectionStart.X, e.X);
            int screenBottom = Math.Max(selectionStart.Y, e.Y);

            selectionRectangle = Rectangle.FromLTRB(screenLeft, screenTop, screenRight, screenBottom);

            SelectedCrystals.Clear();

            var zoom = form.ZoomPanController;
            var topLeft = zoom.TransformPoint(new PointF(selectionRectangle.Left, selectionRectangle.Top));
            var bottomRight = zoom.TransformPoint(new PointF(selectionRectangle.Right, selectionRectangle.Bottom));

            float selLeft = Math.Min(topLeft.X, bottomRight.X);
            float selTop = Math.Min(topLeft.Y, bottomRight.Y);
            float selRight = Math.Max(topLeft.X, bottomRight.X);
            float selBottom = Math.Max(topLeft.Y, bottomRight.Y);

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                if (selRight >= crystal.DisplayLeft &&
                    selLeft <= crystal.DisplayRight &&
                    selBottom >= crystal.DisplayTop &&
                    selTop <= crystal.DisplayBottom)
                {
                    SelectedCrystals.Add(crystal.Index);
                }
            }

            form.UpdateUI();
        }

        private void ShowCrystalInfo(MouseEventArgs e)
        {
            var transformedPoint = TransformMousePoint(e.Location);

            for (int i = CrystalManager.Instance.Crystals.Count - 1; i >= 0; i--)
            {
                var crystal = CrystalManager.Instance.Crystals[i];
                if (IsPointInCrystal(transformedPoint, crystal))
                {
                    form.UiController.ShowHoveredCrystal(crystal);
                    return;
                }
            }
            form.UiController.ShowHoveredCrystal(null);
        }

        public Point LastMousePosition => lastMousePosition;
        public bool IsPanning => isPanning;
    }
}
