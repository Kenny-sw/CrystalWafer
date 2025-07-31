using CrystalTable.Data;
using CrystalTable.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // Основные параметры формы
        private float waferDiameter = 100f;     // Диаметр пластины в миллиметрах
        private float scaleFactor = 4.0f;       // Коэффициент масштабирования
        private const float MinWaferDiameter = 50f;  // Минимальный диаметр пластины
        private const float MaxWaferDiameter = 300f; // Максимальный диаметр пластины
        private int selectedCrystalIndex = -1;   // Индекс выбранного кристалла
        private int nextCrystalIndex = 1;        // Счетчик для кристаллов
        private float crystalWidthRaw;           // Ширина кристалла в микрометрах
        private float crystalHeightRaw;          // Высота кристалла в микрометрах

        // Параметры последнего расчёта для кеширования
        private float lastCrystalWidthRaw = -1f;
        private float lastCrystalHeightRaw = -1f;
        private float lastWaferDiameter = -1f;

        // Глобальные переменные для отслеживания значений
        private float SizeXtemp = 0;
        private float SizeYtemp = 0;
        private float WaferDiameterTemp = 0;
        private bool isDataChanged;

        // ===== НОВЫЕ ПОЛЯ ДЛЯ РАСШИРЕННОЙ ФУНКЦИОНАЛЬНОСТИ =====

        // Масштабирование и панорамирование
        private PointF panOffset = new PointF(0, 0);     // Смещение для панорамирования
        private float zoomFactor = 1.0f;                 // Коэффициент масштабирования
        private const float MinZoom = 0.1f;              // Минимальный зум
        private const float MaxZoom = 10.0f;             // Максимальный зум
        private const float ZoomStep = 0.1f;             // Шаг изменения зума
        private bool isPanning = false;                   // Флаг режима панорамирования
        private Point lastMousePosition;                  // Последняя позиция мыши для панорамирования

        // Групповое выделение кристаллов
        private Rectangle selectionRectangle;            // Прямоугольник выделения
        private bool isSelecting = false;                // Флаг режима выделения
        private Point selectionStart;                     // Начальная точка выделения
        private HashSet<int> selectedCrystals = new HashSet<int>(); // Множество выбранных кристаллов
        private bool isCtrlPressed = false;              // Флаг нажатия Ctrl для множественного выбора

        // История операций
        private CommandHistory commandHistory = new CommandHistory();

        // Предпросмотр маршрута
        private RoutePreview routePreview = new RoutePreview();
        private bool showRoutePreview = false;

        public Form1()
        {
            InitializeComponent();

            // Устанавливаем событие Validated для TextBox
            SizeX.Validated += SizeX_Validated;
            SizeY.Validated += SizeY_Validated;
            WaferDiameter.Validated += WaferDiameter_Validated;

            // ===== НОВЫЕ ОБРАБОТЧИКИ СОБЫТИЙ =====

            // Обработчики для масштабирования и панорамирования
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;

            // Обработчики клавиш для множественного выбора
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.KeyPreview = true; // Важно для перехвата клавиш

            // Обработчики для Undo/Redo (Ctrl+Z, Ctrl+Y)
            this.KeyDown += Form1_KeyDown_UndoRedo;
        }

        // ===== НОВЫЕ МЕТОДЫ ДЛЯ МАСШТАБИРОВАНИЯ И ПАНОРАМИРОВАНИЯ =====

        /// <summary>
        /// Обработчик прокрутки колесика мыши для масштабирования
        /// </summary>
        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Получаем позицию мыши относительно PictureBox
            PointF mousePos = new PointF(e.X, e.Y);

            // Сохраняем старый зум
            float oldZoom = zoomFactor;

            // Изменяем зум в зависимости от направления прокрутки
            if (e.Delta > 0)
            {
                zoomFactor = Math.Min(zoomFactor + ZoomStep, MaxZoom);
            }
            else
            {
                zoomFactor = Math.Max(zoomFactor - ZoomStep, MinZoom);
            }

            // Корректируем смещение, чтобы зум происходил относительно позиции курсора
            if (oldZoom != zoomFactor)
            {
                float zoomRatio = zoomFactor / oldZoom;
                panOffset.X = mousePos.X - (mousePos.X - panOffset.X) * zoomRatio;
                panOffset.Y = mousePos.Y - (mousePos.Y - panOffset.Y) * zoomRatio;
            }

            // Обновляем статусную строку
            UpdateStatusBar();

            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Преобразует координаты мыши с учетом трансформаций
        /// </summary>
        private PointF TransformMousePoint(Point mousePoint)
        {
            float x = (mousePoint.X - panOffset.X) / zoomFactor;
            float y = (mousePoint.Y - panOffset.Y) / zoomFactor;
            return new PointF(x, y);
        }

        // ===== НОВЫЕ МЕТОДЫ ДЛЯ ГРУППОВОГО ВЫДЕЛЕНИЯ =====

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                isCtrlPressed = true;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Control)
            {
                isCtrlPressed = false;
            }
        }

        /// <summary>
        /// Обновляет метку с информацией о выделенных кристаллах
        /// </summary>
        private void UpdateSelectionLabel()
        {
            if (selectedCrystals.Count == 0)
            {
                labelSelectedCrystal.Text = "Кристаллы не выбраны";
            }
            else if (selectedCrystals.Count == 1)
            {
                labelSelectedCrystal.Text = $"Выбран кристалл: {selectedCrystals.First()}";
            }
            else
            {
                labelSelectedCrystal.Text = $"Выбрано кристаллов: {selectedCrystals.Count}";
            }
        }

        // ===== МЕТОДЫ ДЛЯ UNDO/REDO =====

        private void Form1_KeyDown_UndoRedo(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                if (commandHistory.CanUndo())
                {
                    commandHistory.Undo();
                    pictureBox1.Invalidate();
                }
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                if (commandHistory.CanRedo())
                {
                    commandHistory.Redo();
                    pictureBox1.Invalidate();
                }
            }
        }

        // ===== МЕТОДЫ ДЛЯ СТАТИСТИКИ =====

        /// <summary>
        /// Обновляет статусную строку с информацией о пластине
        /// </summary>
        private void UpdateStatusBar()
        {
            if (CrystalManager.Instance.Crystals.Count > 0 && IsInputValid())
            {
                var stats = new WaferStatistics(CrystalManager.Instance.Crystals, waferDiameter);
                float fillPercentage = stats.CalculateFillPercentage(
                    crystalWidthRaw / 1000f,
                    crystalHeightRaw / 1000f);

                // Здесь можно обновить статусную строку, если она добавлена в форму
                // statusLabel.Text = $"Масштаб: {zoomFactor:F1}x | Заполнение: {fillPercentage:F1}%";
            }
        }

        // ===== МЕТОДЫ ДЛЯ ПРЕДПРОСМОТРА МАРШРУТА =====

        /// <summary>
        /// Переключает отображение предпросмотра маршрута
        /// </summary>
        public void ToggleRoutePreview()
        {
            showRoutePreview = !showRoutePreview;

            if (showRoutePreview && CrystalManager.Instance.Crystals.Count > 0)
            {
                // Генерируем маршрут для выбранных или всех кристаллов
                List<Crystal> crystalsToRoute;
                if (selectedCrystals.Count > 0)
                {
                    crystalsToRoute = CrystalManager.Instance.Crystals
                        .Where(c => selectedCrystals.Contains(c.Index))
                        .ToList();
                }
                else
                {
                    crystalsToRoute = CrystalManager.Instance.Crystals;
                }

                var route = routePreview.GenerateScanRoute(crystalsToRoute, RouteType.RowByRow);
                routePreview.SetRoute(route);
            }

            pictureBox1.Invalidate();
        }

        // ===== СУЩЕСТВУЮЩИЕ МЕТОДЫ =====

        // Обработчик события изменения размеров формы
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        // Метод для обновления визуализации пластины
        private void UpdateWaferVisualization()
        {
            pictureBox1.Refresh();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            uint.TryParse(SizeX.Text, out var sizeX);
            uint.TryParse(SizeY.Text, out var sizeY);
            uint.TryParse(WaferDiameter.Text, out var waferDiameter);
            var waferInfo = new WaferInfo
            {
                SizeX = sizeX,
                SizeY = sizeY,
                WaferDiameter = waferDiameter,
            };
            var serializer = new Serializer();
            serializer.Serialize(waferInfo);
        }

        private void SizeX_Validated(object sender, EventArgs e)
        {
            if (!float.TryParse(SizeX.Text, out float value) || value <= 0)
            {
                SizeX.BackColor = Color.Red; // Подсвечиваем красным при неверном вводе
                SizeXtemp = 0; // Сбрасываем значение
            }
            else
            {
                SizeX.BackColor = Color.GreenYellow; // Возвращаем белый цвет
                if (value != SizeXtemp) // Сравниваем с предыдущим значением
                {
                    SizeXtemp = value; // Обновляем временную переменную
                    isDataChanged = true; // Устанавливаем флаг изменения
                }
            }
        }

        private void SizeY_Validated(object sender, EventArgs e)
        {
            if (!float.TryParse(SizeY.Text, out float value) || value <= 0)
            {
                SizeY.BackColor = Color.Red; // Подсвечиваем красным при неверном вводе
                SizeYtemp = 0; // Сбрасываем значение
            }
            else
            {
                SizeY.BackColor = Color.GreenYellow; // Возвращаем белый цвет
                if (value != SizeYtemp) // Сравниваем с предыдущим значением
                {
                    SizeYtemp = value; // Обновляем временную переменную
                    isDataChanged = true; // Устанавливаем флаг изменения
                }
            }
        }

        private void WaferDiameter_Validated(object sender, EventArgs e)
        {
            if (!float.TryParse(WaferDiameter.Text, out float value) || value < MinWaferDiameter || value > MaxWaferDiameter)
            {
                WaferDiameter.BackColor = Color.Red; // Подсвечиваем красным при неверном вводе
                WaferDiameterTemp = 0; // Сбрасываем значение
            }
            else
            {
                WaferDiameter.BackColor = Color.GreenYellow; // Возвращаем белый цвет
                if (value != WaferDiameterTemp) // Сравниваем с предыдущим значением
                {
                    WaferDiameterTemp = value; // Обновляем временную переменную
                    isDataChanged = true; // Устанавливаем флаг изменения
                }
            }
        }

        private void checkBoxFillWafer_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Create_Click(object sender, EventArgs e)
        {
            // Сбрасываем масштаб и панорамирование при создании новой пластины
            zoomFactor = 1.0f;
            panOffset = new PointF(0, 0);
            selectedCrystals.Clear();
            commandHistory.Clear();

            pictureBox1.Invalidate();
        }
    }
}