using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    /// <summary>
    /// Класс для предварительного просмотра маршрута сканирования
    /// </summary>
    public class RoutePreview
    {
        // Текущий маршрут
        private List<Crystal> route;

        // Текущий шаг в маршруте для анимации
        private int currentStep = 0;

        // Настройки отображения
        private bool showNumbers = true;
        private bool showArrows = true;
        private bool animateRoute = false;

        // Таймер для анимации
        private System.Windows.Forms.Timer animationTimer;

        /// <summary>
        /// Событие изменения текущего шага маршрута
        /// </summary>
        public event EventHandler RouteStepChanged;

        /// <summary>
        /// Текущий маршрут
        /// </summary>
        public List<Crystal> Route => route;

        /// <summary>
        /// Текущий шаг в маршруте
        /// </summary>
        public int CurrentStep => currentStep;

        /// <summary>
        /// Общее количество шагов в маршруте
        /// </summary>
        public int TotalSteps => route?.Count ?? 0;

        /// <summary>
        /// Конструктор
        /// </summary>
        public RoutePreview()
        {
            // Инициализация таймера для анимации
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 500; // 500мс между шагами
            animationTimer.Tick += AnimationTimer_Tick;
        }

        /// <summary>
        /// Генерирует оптимальный маршрут сканирования
        /// </summary>
        /// <param name="crystals">Список кристаллов для маршрутизации</param>
        /// <param name="routeType">Тип маршрута</param>
        /// <returns>Упорядоченный список кристаллов</returns>
        public List<Crystal> GenerateScanRoute(List<Crystal> crystals, RouteType routeType)
        {
            if (crystals == null || crystals.Count == 0)
                return new List<Crystal>();

            switch (routeType)
            {
                case RouteType.RowByRow:
                    return GenerateRowByRowRoute(crystals);

                case RouteType.Spiral:
                    return GenerateSpiralRoute(crystals);

                case RouteType.Shortest:
                    return GenerateShortestPathRoute(crystals);

                case RouteType.SnakePattern:
                    return GenerateSnakePatternRoute(crystals);

                case RouteType.OutsideIn:
                    return GenerateOutsideInRoute(crystals);

                default:
                    return crystals.ToList();
            }
        }

        /// <summary>
        /// Генерирует маршрут построчного сканирования (зигзаг)
        /// </summary>
        private List<Crystal> GenerateRowByRowRoute(List<Crystal> crystals)
        {
            var result = new List<Crystal>();

            // Группируем кристаллы по строкам (по Y координате)
            var rows = crystals
                .GroupBy(c => Math.Round(c.RealY, 1)) // Округляем для группировки
                .OrderBy(g => g.Key)
                .ToList();

            bool reverseRow = false;

            foreach (var row in rows)
            {
                var rowCrystals = row.OrderBy(c => c.RealX).ToList();

                // Каждую вторую строку проходим в обратном направлении
                if (reverseRow)
                {
                    rowCrystals.Reverse();
                }

                result.AddRange(rowCrystals);
                reverseRow = !reverseRow;
            }

            return result;
        }

        /// <summary>
        /// Генерирует спиральный маршрут от центра к краю
        /// </summary>
        private List<Crystal> GenerateSpiralRoute(List<Crystal> crystals)
        {
            // Сортируем по расстоянию от центра и углу
            return crystals.OrderBy(c =>
            {
                double distance = Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY);
                double angle = Math.Atan2(c.RealY, c.RealX);

                // Комбинируем расстояние и угол для спирального порядка
                return distance * 10 + angle;
            }).ToList();
        }

        /// <summary>
        /// Генерирует маршрут с минимальным общим расстоянием
        /// (упрощенный алгоритм ближайшего соседа)
        /// </summary>
        private List<Crystal> GenerateShortestPathRoute(List<Crystal> crystals)
        {
            if (crystals.Count <= 2)
                return crystals.ToList();

            var result = new List<Crystal>();
            var remaining = crystals.ToList();

            // Начинаем с кристалла ближайшего к центру
            var current = remaining
                .OrderBy(c => Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY))
                .First();

            result.Add(current);
            remaining.Remove(current);

            // Жадный алгоритм: всегда выбираем ближайший кристалл
            while (remaining.Count > 0)
            {
                var nearest = remaining.OrderBy(c =>
                    Math.Sqrt(Math.Pow(c.RealX - current.RealX, 2) +
                             Math.Pow(c.RealY - current.RealY, 2))).First();

                result.Add(nearest);
                remaining.Remove(nearest);
                current = nearest;
            }

            return result;
        }

        /// <summary>
        /// Генерирует змеевидный маршрут (более плавный зигзаг)
        /// </summary>
        private List<Crystal> GenerateSnakePatternRoute(List<Crystal> crystals)
        {
            var result = new List<Crystal>();

            // Делим на вертикальные колонки
            var columns = crystals
                .GroupBy(c => Math.Round(c.RealX, 1))
                .OrderBy(g => g.Key)
                .ToList();

            bool reverseColumn = false;

            foreach (var column in columns)
            {
                var columnCrystals = column.OrderBy(c => c.RealY).ToList();

                if (reverseColumn)
                {
                    columnCrystals.Reverse();
                }

                result.AddRange(columnCrystals);
                reverseColumn = !reverseColumn;
            }

            return result;
        }

        /// <summary>
        /// Генерирует маршрут от края к центру
        /// </summary>
        private List<Crystal> GenerateOutsideInRoute(List<Crystal> crystals)
        {
            // Сортируем по расстоянию от центра (от большего к меньшему)
            return crystals
                .OrderByDescending(c => Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY))
                .ToList();
        }

        /// <summary>
        /// Рисует предварительный просмотр маршрута
        /// </summary>
        public void DrawRoutePreview(Graphics g, List<Crystal> crystals,
            float scaleFactor, float centerX, float centerY)
        {
            if (route == null || route.Count < 2) return;

            // Настройки рисования
            using (Pen routePen = new Pen(Color.Red, 2))
            using (Pen highlightPen = new Pen(Color.Orange, 3))
            using (Font font = new Font("Arial", 8))
            {
                routePen.DashStyle = DashStyle.Solid;
                routePen.StartCap = LineCap.Round;
                routePen.EndCap = LineCap.Round;

                // Рисуем линии маршрута
                for (int i = 0; i < route.Count - 1; i++)
                {
                    var from = route[i];
                    var to = route[i + 1];

                    float x1 = from.RealX * scaleFactor + centerX;
                    float y1 = from.RealY * scaleFactor + centerY;
                    float x2 = to.RealX * scaleFactor + centerX;
                    float y2 = to.RealY * scaleFactor + centerY;

                    // Выделяем текущий сегмент при анимации
                    if (animateRoute && i == currentStep)
                    {
                        g.DrawLine(highlightPen, x1, y1, x2, y2);
                    }
                    else
                    {
                        g.DrawLine(routePen, x1, y1, x2, y2);
                    }

                    // Рисуем стрелку направления
                    if (showArrows && i % 5 == 0) // Каждая 5-я стрелка для читаемости
                    {
                        DrawArrow(g, x1, y1, x2, y2, routePen.Color);
                    }
                }

                // Рисуем номера порядка на кристаллах
                if (showNumbers)
                {
                    using (Brush numberBrush = new SolidBrush(Color.DarkRed))
                    using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                    {
                        for (int i = 0; i < Math.Min(route.Count, 20); i++) // Первые 20
                        {
                            var crystal = route[i];
                            float x = crystal.RealX * scaleFactor + centerX;
                            float y = crystal.RealY * scaleFactor + centerY;

                            string number = (i + 1).ToString();
                            SizeF textSize = g.MeasureString(number, font);

                            // Фон для числа
                            g.FillEllipse(bgBrush,
                                x - textSize.Width / 2 - 2,
                                y - textSize.Height / 2 - 2,
                                textSize.Width + 4,
                                textSize.Height + 4);

                            // Число
                            g.DrawString(number, font, numberBrush,
                                x - textSize.Width / 2,
                                y - textSize.Height / 2);
                        }
                    }
                }

                // Подсвечиваем текущий кристалл при анимации
                if (animateRoute && currentStep < route.Count)
                {
                    var current = route[currentStep];
                    float x = current.RealX * scaleFactor + centerX;
                    float y = current.RealY * scaleFactor + centerY;

                    using (Brush highlightBrush = new SolidBrush(Color.FromArgb(128, Color.Yellow)))
                    {
                        g.FillEllipse(highlightBrush, x - 15, y - 15, 30, 30);
                    }

                    // Рисуем прогресс
                    DrawProgress(g, centerX, centerY);
                }

                // Отображаем статистику маршрута
                DrawRouteStatistics(g);
            }
        }

        /// <summary>
        /// Рисует стрелку направления
        /// </summary>
        private void DrawArrow(Graphics g, float x1, float y1, float x2, float y2, Color color)
        {
            float angle = (float)Math.Atan2(y2 - y1, x2 - x1);
            float arrowLength = 10;
            float arrowAngle = (float)(Math.PI / 6); // 30 градусов

            // Находим середину линии для размещения стрелки
            float midX = (x1 + x2) / 2;
            float midY = (y1 + y2) / 2;

            using (Pen arrowPen = new Pen(color, 2))
            {
                arrowPen.StartCap = LineCap.Round;
                arrowPen.EndCap = LineCap.Round;

                g.DrawLine(arrowPen,
                    midX,
                    midY,
                    midX - arrowLength * (float)Math.Cos(angle - arrowAngle),
                    midY - arrowLength * (float)Math.Sin(angle - arrowAngle));

                g.DrawLine(arrowPen,
                    midX,
                    midY,
                    midX - arrowLength * (float)Math.Cos(angle + arrowAngle),
                    midY - arrowLength * (float)Math.Sin(angle + arrowAngle));
            }
        }

        /// <summary>
        /// Отображает прогресс прохождения маршрута
        /// </summary>
        private void DrawProgress(Graphics g, float centerX, float centerY)
        {
            if (route == null || route.Count == 0) return;

            string progressText = $"Шаг {currentStep + 1} из {route.Count}";
            float progress = (float)(currentStep + 1) / route.Count;

            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
            using (Brush progressBrush = new SolidBrush(Color.LightGreen))
            using (Pen borderPen = new Pen(Color.DarkGray, 1))
            {
                // Позиция прогресс-бара
                float barWidth = 200;
                float barHeight = 20;
                float x = 10;
                float y = 10;

                // Фон
                g.FillRectangle(bgBrush, x, y, barWidth + 60, barHeight + 30);
                g.DrawRectangle(borderPen, x, y, barWidth + 60, barHeight + 30);

                // Прогресс-бар
                g.DrawRectangle(borderPen, x + 10, y + 25, barWidth, barHeight);
                g.FillRectangle(progressBrush, x + 10, y + 25, barWidth * progress, barHeight);

                // Текст
                g.DrawString(progressText, font, textBrush, x + 10, y + 5);
            }
        }

        /// <summary>
        /// Отображает статистику маршрута
        /// </summary>
        private void DrawRouteStatistics(Graphics g)
        {
            if (route == null || route.Count < 2) return;

            // Вычисляем общую длину маршрута
            float totalDistance = 0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                var from = route[i];
                var to = route[i + 1];
                totalDistance += (float)Math.Sqrt(
                    Math.Pow(to.RealX - from.RealX, 2) +
                    Math.Pow(to.RealY - from.RealY, 2));
            }

            string stats = $"Длина маршрута: {totalDistance:F1} мм";

            using (Font font = new Font("Arial", 9))
            using (Brush textBrush = new SolidBrush(Color.Black))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
            {
                SizeF textSize = g.MeasureString(stats, font);
                float x = 10;
                float y = 70;

                g.FillRectangle(bgBrush, x, y, textSize.Width + 10, textSize.Height + 10);
                g.DrawString(stats, font, textBrush, x + 5, y + 5);
            }
        }

        /// <summary>
        /// Устанавливает маршрут для предпросмотра
        /// </summary>
        public void SetRoute(List<Crystal> newRoute)
        {
            route = newRoute;
            currentStep = 0;
            OnRouteStepChanged();
        }

        /// <summary>
        /// Переход к следующему шагу маршрута
        /// </summary>
        public void NextStep()
        {
            if (currentStep < route.Count - 1)
            {
                currentStep++;
                OnRouteStepChanged();
            }
        }

        /// <summary>
        /// Переход к предыдущему шагу маршрута
        /// </summary>
        public void PreviousStep()
        {
            if (currentStep > 0)
            {
                currentStep--;
                OnRouteStepChanged();
            }
        }

        /// <summary>
        /// Сброс к началу маршрута
        /// </summary>
        public void Reset()
        {
            currentStep = 0;
            OnRouteStepChanged();
        }

        /// <summary>
        /// Запускает анимацию маршрута
        /// </summary>
        public void StartAnimation()
        {
            animateRoute = true;
            animationTimer.Start();
        }

        /// <summary>
        /// Останавливает анимацию
        /// </summary>
        public void StopAnimation()
        {
            animateRoute = false;
            animationTimer.Stop();
        }

        /// <summary>
        /// Обработчик таймера анимации
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            NextStep();

            // Если достигли конца, начинаем сначала
            if (currentStep >= route.Count - 1)
            {
                Reset();
            }
        }

        /// <summary>
        /// Вызывает событие изменения шага маршрута
        /// </summary>
        private void OnRouteStepChanged()
        {
            RouteStepChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Настройки отображения
        /// </summary>
        public bool ShowNumbers
        {
            get => showNumbers;
            set => showNumbers = value;
        }

        public bool ShowArrows
        {
            get => showArrows;
            set => showArrows = value;
        }

        public bool AnimateRoute
        {
            get => animateRoute;
            set => animateRoute = value;
        }

        /// <summary>
        /// Получает информацию о текущем кристалле
        /// </summary>
        public Crystal GetCurrentCrystal()
        {
            if (route != null && currentStep < route.Count)
                return route[currentStep];
            return null;
        }
    }

    /// <summary>
    /// Типы маршрутов сканирования
    /// </summary>
    public enum RouteType
    {
        /// <summary>
        /// Построчное сканирование (зигзаг по строкам)
        /// </summary>
        RowByRow,

        /// <summary>
        /// Спиральное сканирование от центра
        /// </summary>
        Spiral,

        /// <summary>
        /// Кратчайший путь (приближенный)
        /// </summary>
        Shortest,

        /// <summary>
        /// Змеевидный паттерн (по колонкам)
        /// </summary>
        SnakePattern,

        /// <summary>
        /// От края к центру
        /// </summary>
        OutsideIn
    }
}