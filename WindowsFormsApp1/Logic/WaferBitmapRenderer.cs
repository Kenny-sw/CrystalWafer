using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    public sealed class WaferBitmapRenderer : IDisposable
    {
        private readonly object syncRoot = new object();
        private Bitmap cachedBitmap;
        private Size cachedSize;
        private float cachedScaleFactor;
        private int cachedVersion;
        private float cachedDiameter;

        public Bitmap Render(IReadOnlyList<Crystal> crystals,
                             float waferDiameter,
                             int width,
                             int height,
                             float scaleFactor,
                             int version)
        {
            if (crystals == null || width <= 0 || height <= 0 || waferDiameter <= 0f || scaleFactor <= 0f)
            {
                return null;
            }

            lock (syncRoot)
            {
                bool requiresRebuild = cachedBitmap == null ||
                                       cachedSize.Width != width ||
                                       cachedSize.Height != height ||
                                       Math.Abs(cachedScaleFactor - scaleFactor) > 1e-3f ||
                                       Math.Abs(cachedDiameter - waferDiameter) > 1e-3f ||
                                       cachedVersion != version;

                if (requiresRebuild)
                {
                    cachedBitmap?.Dispose();
                    cachedBitmap = new Bitmap(width, height);
                    cachedBitmap.SetResolution(96f, 96f);
                    cachedSize = new Size(width, height);
                    cachedScaleFactor = scaleFactor;
                    cachedVersion = version;
                    cachedDiameter = waferDiameter;

                    Rebuild(cachedBitmap, crystals, waferDiameter, scaleFactor);
                }

                return cachedBitmap;
            }
        }

        private static void Rebuild(Bitmap target,
                                     IReadOnlyList<Crystal> crystals,
                                     float waferDiameter,
                                     float scaleFactor)
        {
            using var g = Graphics.FromImage(target);
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            float cx = target.Width / 2f;
            float cy = target.Height / 2f;
            float radiusPx = (waferDiameter / 2f) * scaleFactor;
            var waferRect = new RectangleF(cx - radiusPx, cy - radiusPx, radiusPx * 2f, radiusPx * 2f);

            using var waferFill = new SolidBrush(Color.FromArgb(28, 64, 96, 64));
            using var waferBorder = new Pen(Color.FromArgb(120, 68, 100, 68), 2f);
            g.FillEllipse(waferFill, waferRect);
            g.DrawEllipse(waferBorder, waferRect);

            using var path = new GraphicsPath();
            path.AddEllipse(waferRect);
            var state = g.Save();
            g.SetClip(path, CombineMode.Replace);

            using var fullBrush = new SolidBrush(Color.FromArgb(180, 58, 112, 68));
            using var partialHatch = new HatchBrush(HatchStyle.ForwardDiagonal,
                                                    Color.FromArgb(190, 102, 138, 92),
                                                    Color.FromArgb(60, 48, 80, 48));
            using var cellBorder = new Pen(Color.FromArgb(160, 48, 82, 48), 0.8f);

            for (int i = 0; i < crystals.Count; i++)
            {
                var crystal = crystals[i];
                float left = crystal.DisplayLeft;
                float top = crystal.DisplayTop;
                float width = crystal.DisplayRight - crystal.DisplayLeft;
                float height = crystal.DisplayBottom - crystal.DisplayTop;

                if (width <= 0f || height <= 0f)
                {
                    continue;
                }

                var rect = new RectangleF(left, top, width, height);
                switch (crystal.PlacementStatus)
                {
                    case CrystalPlacementStatus.Full:
                        g.FillRectangle(fullBrush, rect);
                        break;
                    case CrystalPlacementStatus.Partial:
                        g.FillRectangle(partialHatch, rect);
                        break;
                }

                g.DrawRectangle(cellBorder, rect.X, rect.Y, rect.Width, rect.Height);
            }

            g.Restore(state);
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                cachedBitmap?.Dispose();
                cachedBitmap = null;
            }
        }
    }
}
