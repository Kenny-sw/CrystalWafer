using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    /// <summary>Предварительный просмотр маршрута сканирования</summary>
    public class RoutePreview
    {
        private List<Crystal> route;
        private int currentStep = 0;

        // Настройки отображения
        private bool showNumbers = true;
        private bool showArrows = true;
        private bool animateRoute = false;

        private readonly System.Windows.Forms.Timer animationTimer;

        public event EventHandler RouteStepChanged;

        public List<Crystal> Route => route;
        public int CurrentStep => currentStep;
        public int TotalSteps => route?.Count ?? 0;

        public RoutePreview()
        {
            animationTimer = new System.Windows.Forms.Timer
            {
                Interval = 500 // мс между шагами
            };
            animationTimer.Tick += AnimationTimer_Tick;
        }

        // ==== Генерация маршрутов (на будущее; сейчас сетка строится в контроллере) ====

        public List<Crystal> GenerateScanRoute(List<Crystal> crystals, RouteType routeType)
        {
            if (crystals == null || crystals.Count == 0)
                return new List<Crystal>();

            switch (routeType)
            {
                case RouteType.RowByRow: return GenerateRowByRowRoute(crystals);
                case RouteType.Spiral: return GenerateSpiralRoute(crystals);
                case RouteType.Shortest: return GenerateShortestPathRoute(crystals);
                case RouteType.SnakePattern: return GenerateSnakePatternRoute(crystals);
                case RouteType.OutsideIn: return GenerateOutsideInRoute(crystals);
                default: return crystals.ToList();
            }
        }

        private List<Crystal> GenerateRowByRowRoute(List<Crystal> crystals)
        {
            var result = new List<Crystal>();
            var rows = crystals
                .GroupBy(c => Math.Round(c.RealY, 1))
                .OrderBy(g => g.Key)
                .ToList();

            bool reverseRow = false;
            foreach (var row in rows)
            {
                var rowCrystals = row.OrderBy(c => c.RealX).ToList();
                if (reverseRow) rowCrystals.Reverse();
                result.AddRange(rowCrystals);
                reverseRow = !reverseRow;
            }
            return result;
        }

        private List<Crystal> GenerateSpiralRoute(List<Crystal> crystals)
        {
            return crystals.OrderBy(c =>
            {
                double d = Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY);
                double a = Math.Atan2(c.RealY, c.RealX);
                return d * 10 + a;
            }).ToList();
        }

        private List<Crystal> GenerateShortestPathRoute(List<Crystal> crystals)
        {
            if (crystals.Count <= 2) return crystals.ToList();

            var result = new List<Crystal>();
            var remaining = crystals.ToList();

            var current = remaining
                .OrderBy(c => Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY))
                .First();

            result.Add(current);
            remaining.Remove(current);

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

        private List<Crystal> GenerateSnakePatternRoute(List<Crystal> crystals)
        {
            var result = new List<Crystal>();
            var columns = crystals
                .GroupBy(c => Math.Round(c.RealX, 1))
                .OrderBy(g => g.Key)
                .ToList();

            bool reverse = false;
            foreach (var col in columns)
            {
                var list = col.OrderBy(c => c.RealY).ToList();
                if (reverse) list.Reverse();
                result.AddRange(list);
                reverse = !reverse;
            }
            return result;
        }

        private List<Crystal> GenerateOutsideInRoute(List<Crystal> crystals)
        {
            return crystals
                .OrderByDescending(c => Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY))
                .ToList();
        }

        // ==== Рисование ====

        /// <summary>
        /// Рисует предварительный просмотр маршрута (совместимо с вызовом из Form1.Drawing.cs)
        /// </summary>
        public void DrawRoutePreview(Graphics g, List<Crystal> crystals,
                                     float scaleFactor, float centerX, float centerY)
        {
            if (g == null) return;
            if (route == null || route.Count < 2) return;

            using (Pen routePen = new Pen(Color.Red, 2))
            using (Pen highlightPen = new Pen(Color.Orange, 3))
            using (Font font = new Font("Arial", 8))
            {
                routePen.DashStyle = DashStyle.Solid;
                routePen.StartCap = LineCap.Round;
                routePen.EndCap = LineCap.Round;

                // линии маршрута
                for (int i = 0; i < route.Count - 1; i++)
                {
                    var from = route[i];
                    var to = route[i + 1];

                    float x1 = from.RealX * scaleFactor + centerX;
                    float y1 = from.RealY * scaleFactor + centerY;
                    float x2 = to.RealX * scaleFactor + centerX;
                    float y2 = to.RealY * scaleFactor + centerY;

                    if (animateRoute && i == currentStep)
                        g.DrawLine(highlightPen, x1, y1, x2, y2);
                    else
                        g.DrawLine(routePen, x1, y1, x2, y2);

                    if (showArrows && i % 5 == 0) // каждая 5-я стрелка
                        DrawArrow(g, x1, y1, x2, y2, routePen.Color);
                }

                // номера первых точек
                if (showNumbers)
                {
                    using (Brush numberBrush = new SolidBrush(Color.DarkRed))
                    using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                    {
                        for (int i = 0; i < Math.Min(route.Count, 20); i++)
                        {
                            var c = route[i];
                            float x = c.RealX * scaleFactor + centerX;
                            float y = c.RealY * scaleFactor + centerY;

                            string num = (i + 1).ToString();
                            SizeF sz = g.MeasureString(num, font);

                            g.FillEllipse(bgBrush, x - sz.Width / 2 - 2, y - sz.Height / 2 - 2, sz.Width + 4, sz.Height + 4);
                            g.DrawString(num, font, numberBrush, x - sz.Width / 2, y - sz.Height / 2);
                        }
                    }
                }

                // подсветка текущей точки при анимации + прогресс
                if (animateRoute && currentStep < route.Count)
                {
                    var cur = route[currentStep];
                    float x = cur.RealX * scaleFactor + centerX;
                    float y = cur.RealY * scaleFactor + centerY;

                    using (Brush hb = new SolidBrush(Color.FromArgb(128, Color.Yellow)))
                        g.FillEllipse(hb, x - 15, y - 15, 30, 30);

                    DrawProgress(g, centerX, centerY);
                }

                DrawRouteStatistics(g);
            }
        }

        private void DrawArrow(Graphics g, float x1, float y1, float x2, float y2, Color color)
        {
            float angle = (float)Math.Atan2(y2 - y1, x2 - x1);
            float L = 10f;
            float A = (float)(Math.PI / 6); // 30°

            float midX = (x1 + x2) / 2f;
            float midY = (y1 + y2) / 2f;

            using (Pen p = new Pen(color, 2))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;

                g.DrawLine(p, midX, midY, midX - L * (float)Math.Cos(angle - A), midY - L * (float)Math.Sin(angle - A));
                g.DrawLine(p, midX, midY, midX - L * (float)Math.Cos(angle + A), midY - L * (float)Math.Sin(angle + A));
            }
        }

        private void DrawProgress(Graphics g, float centerX, float centerY)
        {
            if (route == null || route.Count == 0) return;

            string text = $"Шаг {currentStep + 1} из {route.Count}";
            float progress = (float)(currentStep + 1) / route.Count;

            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (Brush tb = new SolidBrush(Color.Black))
            using (Brush bg = new SolidBrush(Color.FromArgb(200, Color.White)))
            using (Brush pb = new SolidBrush(Color.LightGreen))
            using (Pen border = new Pen(Color.DarkGray, 1))
            {
                float barW = 200, barH = 20, x = 10, y = 10;
                g.FillRectangle(bg, x, y, barW + 60, barH + 30);
                g.DrawRectangle(border, x, y, barW + 60, barH + 30);

                g.DrawRectangle(border, x + 10, y + 25, barW, barH);
                g.FillRectangle(pb, x + 10, y + 25, barW * progress, barH);

                g.DrawString(text, font, tb, x + 10, y + 5);
            }
        }

        private void DrawRouteStatistics(Graphics g)
        {
            if (route == null || route.Count < 2) return;

            float total = 0f;
            for (int i = 0; i < route.Count - 1; i++)
            {
                var a = route[i];
                var b = route[i + 1];
                total += (float)Math.Sqrt(Math.Pow(b.RealX - a.RealX, 2) + Math.Pow(b.RealY - a.RealY, 2));
            }

            string stats = $"Длина маршрута: {total:F1} мм";

            using (Font font = new Font("Arial", 9))
            using (Brush tb = new SolidBrush(Color.Black))
            using (Brush bg = new SolidBrush(Color.FromArgb(200, Color.White)))
            {
                SizeF sz = g.MeasureString(stats, font);
                float x = 10, y = 70;
                g.FillRectangle(bg, x, y, sz.Width + 10, sz.Height + 10);
                g.DrawString(stats, font, tb, x + 5, y + 5);
            }
        }

        // ==== Управление ====

        public void SetRoute(List<Crystal> newRoute)
        {
            route = newRoute;
            currentStep = 0;
            OnRouteStepChanged();
        }

        public void NextStep()
        {
            if (route == null || route.Count == 0) return;
            if (currentStep < route.Count - 1)
            {
                currentStep++;
                OnRouteStepChanged();
            }
        }

        public void PreviousStep()
        {
            if (route == null || route.Count == 0) return;
            if (currentStep > 0)
            {
                currentStep--;
                OnRouteStepChanged();
            }
        }

        public void Reset()
        {
            currentStep = 0;
            OnRouteStepChanged();
        }

        public void StartAnimation()
        {
            animateRoute = true;
            animationTimer.Start();
        }

        public void StopAnimation()
        {
            animateRoute = false;
            animationTimer.Stop();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            NextStep();
            if (route != null && currentStep >= route.Count - 1)
                Reset();
        }

        private void OnRouteStepChanged() => RouteStepChanged?.Invoke(this, EventArgs.Empty);

        // ==== Настройки отображения ====
        public bool ShowNumbers { get => showNumbers; set => showNumbers = value; }
        public bool ShowArrows { get => showArrows; set => showArrows = value; }
        public bool AnimateRoute { get => animateRoute; set => animateRoute = value; }

        public Crystal GetCurrentCrystal()
        {
            if (route != null && currentStep < route.Count)
                return route[currentStep];
            return null;
        }
    }

    public enum RouteType
    {
        RowByRow,
        Spiral,
        Shortest,
        SnakePattern,
        OutsideIn
    }
}
