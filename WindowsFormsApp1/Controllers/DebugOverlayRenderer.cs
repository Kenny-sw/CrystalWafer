using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Рендерер отладочных оверлеев для визуализации координат и диагностики
    /// </summary>
    public class DebugOverlayRenderer : IDisposable
    {
        // ===== НАСТРОЙКИ ВКЛЮЧЕНИЯ/ВЫКЛЮЧЕНИЯ =====
        public bool ShowPositionDiagnostics { get; set; }
        public bool ShowCalibrationPoints { get; set; }

        // ===== НАСТРОЙКИ ВИЗУАЛИЗАЦИИ =====
        private readonly Font inspectorFont = new Font("Consolas", 9f, FontStyle.Regular);
        private readonly Font inspectorBoldFont = new Font("Consolas", 9f, FontStyle.Bold);
        
        private bool _disposed = false;
     
        private readonly Color overlayBackColor = Color.FromArgb(230, 40, 40, 40);
        private readonly Color overlayTextColor = Color.White;
        private readonly Color calibrationColor = Color.LimeGreen;

        /// <summary>
   /// Главный метод отрисовки всех оверлеев
   /// </summary>
    public void Draw(Graphics g, Form1 form)
        {
     if (g == null || form == null) return;

       // Качество отрисовки
            g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

   // Порядок отрисовки (от фона к переднему плану)
    if (ShowCalibrationPoints)
 DrawCalibrationPoints(g, form);

       // Диагностика позиции всегда поверх всего
      if (ShowPositionDiagnostics)
            DrawPositionDiagnostics(g, form);
    }

        /// <summary>
    /// Рисует панель диагностики позиции (без перекрестия на пластине)
    /// </summary>
        private void DrawPositionDiagnostics(Graphics g, Form1 form)
      {
            try
     {
   // ===== ПОЛУЧЕНИЕ ДАННЫХ =====
  var waferController = form.WaferController;
      var pointerVirtual = form.GetPointerMm();
   var pointerPhysical = form.GetPointerMachineMm();

         // ===== ПАНЕЛЬ ИНФОРМАЦИИ =====
  DrawDiagnosticsPanel(g, form, pointerVirtual, pointerPhysical);
  }
catch (Exception ex)
          {
  AppLogger.Warning($"Ошибка отрисовки диагностики позиции: {ex.Message}");
            }
        }

        /// <summary>
  /// Рисует информационную панель диагностики
        /// </summary>
      private void DrawDiagnosticsPanel(Graphics g, Form1 form, PointF pointerVirtual, PointF pointerPhysical)
     {
            var waferController = form.WaferController;

       // Поиск ближайшего кристалла
            Crystal nearestCrystal = null;
    float minDistance = float.MaxValue;
         
    foreach (var crystal in CrystalManager.Instance.Crystals)
 {
                float dx = crystal.RealX - pointerVirtual.X;
      float dy = crystal.RealY - pointerVirtual.Y;
        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
   
             if (dist < minDistance)
              {
  minDistance = dist;
  nearestCrystal = crystal;
    }
        }

 // Формирование текста
string[] lines = new[]
            {
                "📍 ДИАГНОСТИКА ПОЗИЦИИ",
   "─────────────────────────",
       "Координаты ЛШД (карта):",
      $"  X: {pointerVirtual.X:F3} мм",
      $"  Y: {pointerVirtual.Y:F3} мм",
 "─────────────────────────",
     "Машинные координаты:",
     $"  X: {pointerPhysical.X:F3} мм",
           $"  Y: {pointerPhysical.Y:F3} мм",
    "─────────────────────────",
       nearestCrystal != null ? "Ближайший кристалл:" : "Нет кристаллов",
           nearestCrystal != null ? $"  Индекс: #{nearestCrystal.Index}" : "",
                nearestCrystal != null ? $"  Расстояние: {minDistance:F3} мм" : "",
       "─────────────────────────",
  waferController.IsCalibrated ? "Калибровка: ✅ Активна" : "Калибровка: ❌ Нет",
          waferController.IsCalibrated 
       ? $"  Смещение: ({waferController.CalibrationOffsetX:+0.00;-0.00;0}, {waferController.CalibrationOffsetY:+0.00;-0.00;0}) мм"
          : ""
         };

    // Вычисление размеров панели
        float lineHeight = inspectorFont.Height + 2;
 float maxWidth = 0;
            foreach (var line in lines)
    {
     if (!string.IsNullOrWhiteSpace(line))
             {
        var size = g.MeasureString(line, line.StartsWith("📍") ? inspectorBoldFont : inspectorFont);
        if (size.Width > maxWidth) maxWidth = size.Width;
                }
          }

 float panelWidth = maxWidth + 20;
            float panelHeight = lines.Length * lineHeight + 20;
      float panelX = form.PictureBox.Width - panelWidth - 10;
   float panelY = 10;

            // Фон панели
            using (Brush bgBrush = new SolidBrush(overlayBackColor))
            using (Pen borderPen = new Pen(Color.FromArgb(150, 255, 255, 255), 1f))
          using (Brush textBrush = new SolidBrush(overlayTextColor))
            {
     RectangleF panelRect = new RectangleF(panelX, panelY, panelWidth, panelHeight);
           g.FillRoundedRectangle(bgBrush, panelRect, 8f);
   g.DrawRoundedRectangle(borderPen, panelRect, 8f);

     // Текст
  float y = panelY + 10;
      foreach (var line in lines)
    {
       if (!string.IsNullOrWhiteSpace(line))
   {
           Font font = line.StartsWith("📍") ? inspectorBoldFont : inspectorFont;
          g.DrawString(line, font, textBrush, panelX + 10, y);
     }
        y += lineHeight;
         }
    }
  }

        /// <summary>
/// Рисует точки калибровки
        /// </summary>
 private void DrawCalibrationPoints(Graphics g, Form1 form)
    {
            try
         {
         var waferController = form.WaferController;
    if (!waferController.HasFirstRef && !waferController.HasLastRef)
 return;

    float scale = form.ZoomPanController.ZoomFactor * waferController.ScaleFactor;
          float centerX = form.PictureBox.Width / 2f + form.ZoomPanController.PanOffset.X;
            float centerY = form.PictureBox.Height / 2f + form.ZoomPanController.PanOffset.Y;

   using (Brush markerBrush = new SolidBrush(calibrationColor))
                using (Pen markerPen = new Pen(Color.White, 2f))
          using (Pen linePen = new Pen(calibrationColor, 1.5f))
          using (Font labelFont = new Font("Arial", 8f, FontStyle.Bold))
       using (Brush textBrush = new SolidBrush(Color.White))
        using (Brush textBgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
           linePen.DashStyle = DashStyle.Dash;

          if (waferController.HasFirstRef)
               {
     var firstRef = waferController.FirstRefMm;
    float x = firstRef.X * scale + centerX;
            float y = firstRef.Y * scale + centerY;

 // Маркер
       g.FillEllipse(markerBrush, x - 6, y - 6, 12, 12);
         g.DrawEllipse(markerPen, x - 6, y - 6, 12, 12);

         // Подпись
     DrawLabel(g, "First ref", x, y - 20, labelFont, textBrush, textBgBrush);
          }

    if (waferController.HasLastRef)
              {
      var lastRef = waferController.LastRefMm;
   float x = lastRef.X * scale + centerX;
  float y = lastRef.Y * scale + centerY;

  // Маркер
    g.FillEllipse(markerBrush, x - 6, y - 6, 12, 12);
               g.DrawEllipse(markerPen, x - 6, y - 6, 12, 12);

     // Подпись
       DrawLabel(g, "Last ref", x, y + 10, labelFont, textBrush, textBgBrush);
          }

             // Линия между точками
          if (waferController.HasFirstRef && waferController.HasLastRef)
       {
              var firstRef = waferController.FirstRefMm;
    var lastRef = waferController.LastRefMm;
           float x1 = firstRef.X * scale + centerX;
     float y1 = firstRef.Y * scale + centerY;
    float x2 = lastRef.X * scale + centerX;
          float y2 = lastRef.Y * scale + centerY;

   g.DrawLine(linePen, x1, y1, x2, y2);
  }
                }
  }
       catch (Exception ex)
            {
      AppLogger.Warning($"Ошибка отрисовки точек калибровки: {ex.Message}");
       }
        }

        /// <summary>
        /// Вспомогательный метод для рисования подписей с фоном
        /// </summary>
        private void DrawLabel(Graphics g, string text, float x, float y, Font font, Brush textBrush, Brush bgBrush)
      {
            var size = g.MeasureString(text, font);
 float padding = 4;
            RectangleF bgRect = new RectangleF(x - size.Width / 2 - padding, y - padding, size.Width + padding * 2, size.Height + padding * 2);

       g.FillRoundedRectangle(bgBrush, bgRect, 4f);
       g.DrawString(text, font, textBrush, x - size.Width / 2, y);
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    inspectorFont?.Dispose();
                    inspectorBoldFont?.Dispose();
                }
                _disposed = true;
            }
        }
        
        ~DebugOverlayRenderer()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Вспомогательные методы расширения для Graphics
    /// </summary>
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
     {
          using (GraphicsPath path = GetRoundedRectPath(rect, radius))
 {
             g.FillPath(brush, path);
}
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius)
  {
    using (GraphicsPath path = GetRoundedRectPath(rect, radius))
      {
    g.DrawPath(pen, path);
      }
        }

        private static GraphicsPath GetRoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
      float diameter = radius * 2;
      
    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
     path.CloseFigure();
            
      return path;
      }
    }
}
