using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для управления мышью и выделением кристаллов
    /// </summary>
    public class MouseController
    {
        private readonly Form1 form;
        private readonly WaferController waferController;

        // Состояние выделения
        public HashSet<int> SelectedCrystals { get; private set; } = new HashSet<int>();
        private int selectedCrystalIndex = -1;

        // Прямоугольное выделение
        private Rectangle selectionRectangle;
        private bool isSelecting = false;
        private Point selectionStart;

        // Модификаторы
        private bool isCtrlPressed = false;

        // Панорамирование
        private bool isPanning = false;
        private Point lastMousePosition;

        public MouseController(Form1 form, WaferController waferController)
        {
            this.form = form;
            this.waferController = waferController;
        }

        /// <summary>
        /// Обработка нажатия кнопки мыши
        /// </summary>
        public void HandleMouseDown(MouseEventArgs e)
        {
            // Панорамирование средней кнопкой
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

                // Проверяем попадание в кристалл
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

                // Начинаем прямоугольное выделение
                if (!hitCrystal && !isCtrlPressed)
                {
                    isSelecting = true;
                    selectionStart = e.Location;
                    selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                    SelectedCrystals.Clear();
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Обработка движения мыши
        /// </summary>
        public void HandleMouseMove(MouseEventArgs e)
        {
            // Обновляем координаты
            form.LabelX.Text = $"X: {e.X}";
            form.LabelY.Text = $"Y: {e.Y}";

            // Панорамирование
            if (isPanning && e.Button == MouseButtons.Middle)
            {
                HandlePanning(e);
                return;
            }

            // Прямоугольное выделение
            if (isSelecting)
            {
                UpdateSelection(e);
                return;
            }

            // Подсветка кристалла под курсором
            ShowCrystalInfo(e);
        }

        /// <summary>
        /// Обработка отпускания кнопки мыши
        /// </summary>
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
                UpdateUI();
            }
        }

        /// <summary>
        /// Обработка нажатия клавиш
        /// </summary>
        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                isCtrlPressed = true;
            }
        }

        /// <summary>
        /// Обработка отпускания клавиш
        /// </summary>
        public void HandleKeyUp(KeyEventArgs e)
        {
            if (!e.Control)
            {
                isCtrlPressed = false;
            }
        }

        /// <summary>
        /// Выделить все кристаллы
        /// </summary>
        public void SelectAll(List<Crystal> crystals)
        {
            SelectedCrystals.Clear();
            foreach (var crystal in crystals)
            {
                SelectedCrystals.Add(crystal.Index);
            }
        }

        /// <summary>
        /// Очистить выделение
        /// </summary>
        public void ClearSelection()
        {
            SelectedCrystals.Clear();
            selectedCrystalIndex = -1;
        }

        /// <summary>
        /// Получить прямоугольник выделения для отрисовки
        /// </summary>
        public Rectangle GetSelectionRectangle()
        {
            return isSelecting ? selectionRectangle : Rectangle.Empty;
        }

        /// <summary>
        /// Проверка, идет ли выделение
        /// </summary>
        public bool IsSelecting => isSelecting;

        /// <summary>
        /// Получить выбранный кристалл
        /// </summary>
        public int SelectedCrystalIndex => selectedCrystalIndex;

        // === Приватные методы ===

        private PointF TransformMousePoint(Point mousePoint)
        {
            // Здесь должна быть трансформация с учетом зума и панорамирования
            // Для простоты пока возвращаем как есть
            return new PointF(mousePoint.X, mousePoint.Y);
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
            int oldSelectedIndex = selectedCrystalIndex;

            if (isCtrlPressed)
            {
                // Множественный выбор
                if (SelectedCrystals.Contains(crystal.Index))
                {
                    SelectedCrystals.Remove(crystal.Index);
                }
                else
                {
                    SelectedCrystals.Add(crystal.Index);
                }
            }
            else
            {
                // Одиночный выбор
                var oldSelection = new HashSet<int>(SelectedCrystals);
                SelectedCrystals.Clear();
                SelectedCrystals.Add(crystal.Index);
                selectedCrystalIndex = crystal.Index;

                // Добавляем в историю
                form.CommandHistory.ExecuteCommand(
                    new SelectCrystalCommand(
                        oldSelectedIndex,
                        crystal.Index,
                        (index) => {
                            selectedCrystalIndex = index;
                            SelectedCrystals.Clear();
                            if (index > 0) SelectedCrystals.Add(index);
                        },
                        () => form.PictureBox.Invalidate()
                    )
                );
            }

            UpdateUI();
        }

        private void HandlePanning(MouseEventArgs e)
        {
            // Панорамирование должно быть реализовано в ZoomPanController
            // Здесь просто вызываем соответствующий метод
            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;

            // Передаем управление ZoomPanController через Form1
            lastMousePosition = e.Location;
            form.PictureBox.Invalidate();
        }

        private void UpdateSelection(MouseEventArgs e)
        {
            // Обновляем прямоугольник выделения
            int x = Math.Min(selectionStart.X, e.X);
            int y = Math.Min(selectionStart.Y, e.Y);
            int width = Math.Abs(e.X - selectionStart.X);
            int height = Math.Abs(e.Y - selectionStart.Y);

            selectionRectangle = new Rectangle(x, y, width, height);

            // Обновляем выбранные кристаллы
            SelectedCrystals.Clear();

            // Здесь должна быть проверка пересечения с учетом трансформаций
            // Упрощенная версия:
            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                Rectangle crystalRect = new Rectangle(
                    (int)crystal.DisplayLeft,
                    (int)crystal.DisplayTop,
                    (int)(crystal.DisplayRight - crystal.DisplayLeft),
                    (int)(crystal.DisplayBottom - crystal.DisplayTop)
                );

                if (selectionRectangle.IntersectsWith(crystalRect))
                {
                    SelectedCrystals.Add(crystal.Index);
                }
            }

            UpdateUI();
        }

        private void ShowCrystalInfo(MouseEventArgs e)
        {
            var transformedPoint = TransformMousePoint(e.Location);

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                if (IsPointInCrystal(transformedPoint, crystal))
                {
                    form.LabelIndex.Text = $"Индекс кристалла: {crystal.Index}";
                    return;
                }
            }

            form.LabelIndex.Text = "Индекс кристалла: -";
        }

        private void UpdateUI()
        {
            form.PictureBox.Invalidate();
        }

        public Point LastMousePosition => lastMousePosition;
        public bool IsPanning => isPanning;
    }
}