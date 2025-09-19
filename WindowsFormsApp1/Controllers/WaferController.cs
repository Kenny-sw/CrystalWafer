using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    public class WaferController
    {
        private readonly Form1 form;

        public const float MinWaferDiameter = 50f;
        public const float MaxWaferDiameter = 450f;

        public uint CrystalWidthRaw { get; set; }
        public uint CrystalHeightRaw { get; set; }
        public float WaferDiameter { get; set; }

        public uint SizeXtemp, SizeYtemp;
        public float WaferDiameterTemp;

        public float StepXmm { get; private set; }
        public float StepYmm { get; private set; }
        public int CrystalsPerRow { get; private set; }
        public int RowsTotal { get; private set; }
        public float ScaleFactor { get; private set; } = 1f;
        public bool WaferDisplayMode { get; set; }
        public float RotationAngleDeg { get; private set; }

        private bool firstRefSet;
        private bool lastRefSet;
        private PointF firstRefMm;
        private PointF lastRefMm;

        public bool HasFirstRef => firstRefSet;
        public bool HasLastRef => lastRefSet;
        public PointF FirstRefMm => firstRefMm;
        public PointF LastRefMm => lastRefMm;

        public WaferController(Form1 form)
        {
            this.form = form;
            SizeXtemp = 100;
            SizeYtemp = 100;
            WaferDiameterTemp = 100f;
        }

        public void AutoSetScaleFactor(int viewportWidth, int viewportHeight)
        {
            float d = WaferDiameter > 0 ? WaferDiameter : 200f;
            if (d <= 0)
            {
                ScaleFactor = 1f;
                return;
            }

            const float padding = 20f;
            float usableWidth = Math.Max(1, viewportWidth - padding * 2);
            float usableHeight = Math.Max(1, viewportHeight - padding * 2);

            float sx = usableWidth / d;
            float sy = usableHeight / d;
            ScaleFactor = Math.Max(1f, Math.Min(sx, sy));
        }

        public void CreateNewWafer()
        {
            firstRefSet = false;
            lastRefSet = false;
            StepXmm = 0f;
            StepYmm = 0f;
            RotationAngleDeg = 0f;
            BuildCrystalsCached();
        }

        public void BuildCrystalsCached()
        {
            var crystals = CrystalManager.Instance.Crystals;
            crystals.Clear();

            float stepX = StepXmm > 0f ? StepXmm : CrystalWidthRaw / 1000f;
            float stepY = StepYmm > 0f ? StepYmm : CrystalHeightRaw / 1000f;

            if (stepX <= 0f || stepY <= 0f || WaferDiameter <= 0f)
            {
                CrystalsPerRow = 0;
                RowsTotal = 0;
                return;
            }

            if (HasFirstRef && HasLastRef)
            {
                RotationAngleDeg = BuildCalibratedGrid(crystals, stepX, stepY);
                if (crystals.Count == 0)
                {
                    RotationAngleDeg = 0f;
                    BuildDefaultGrid(crystals, stepX, stepY);
                }
            }
            else
            {
                RotationAngleDeg = 0f;
                BuildDefaultGrid(crystals, stepX, stepY);
            }
        }

        public void SetFirstReference(float xMm, float yMm)
        {
            firstRefMm = new PointF(xMm, yMm);
            firstRefSet = true;
        }

        public void SetLastReference(float xMm, float yMm)
        {
            lastRefMm = new PointF(xMm, yMm);
            lastRefSet = true;
        }
        public void ClearReferences()
        {
            firstRefSet = false;
            lastRefSet = false;
        }


        public bool IsCalibrationReady() => firstRefSet && lastRefSet;

        public bool IsPresetReady()
        {
            if (StepXmm > 0f && StepYmm > 0f)
            {
                return true;
            }

            return CrystalWidthRaw > 0 && CrystalHeightRaw > 0;
        }

        public void BuildMapFromReferences()
        {
            if (StepXmm <= 0f)
            {
                StepXmm = CrystalWidthRaw / 1000f;
            }

            if (StepYmm <= 0f)
            {
                StepYmm = CrystalHeightRaw / 1000f;
            }

            BuildCrystalsCached();
        }

        public void BuildMapFromPreset()
        {
            if (StepXmm <= 0f)
            {
                StepXmm = CrystalWidthRaw / 1000f;
            }

            if (StepYmm <= 0f)
            {
                StepYmm = CrystalHeightRaw / 1000f;
            }

            BuildCrystalsCached();
        }

        public void GenerateRoute(RoutePreview preview, HashSet<int> selectedCrystals)
        {
            if (preview == null)
            {
                return;
            }

            var source = CrystalManager.Instance.Crystals;
            List<Crystal> route;

            if (selectedCrystals != null && selectedCrystals.Count > 0)
            {
                route = source.Where(c => selectedCrystals.Contains(c.Index)).ToList();
            }
            else
            {
                route = new List<Crystal>(source);
            }

            route.Sort((a, b) =>
            {
                int byY = a.RealY.CompareTo(b.RealY);
                if (byY != 0)
                {
                    return byY;
                }

                float stepY = StepYmm > 0 ? StepYmm : Math.Max(0.0001f, CrystalHeightRaw / 1000f);
                int rowIndex = (int)Math.Round((a.RealY - (-WaferDiameter / 2f)) / stepY);
                bool reverse = (rowIndex & 1) == 1;
                return reverse ? b.RealX.CompareTo(a.RealX) : a.RealX.CompareTo(b.RealX);
            });

            preview.SetRoute(route);
        }

        public WaferStatistics GetStatistics()
        {
            return new WaferStatistics(CrystalManager.Instance.Crystals, WaferDiameter);
        }

        public float StepXmmOrDefault => StepXmm > 0f ? StepXmm : CrystalWidthRaw / 1000f;
        public float StepYmmOrDefault => StepYmm > 0f ? StepYmm : CrystalHeightRaw / 1000f;

        public void SetSteps(float stepXmm, float stepYmm)
        {
            StepXmm = Math.Max(0f, stepXmm);
            StepYmm = Math.Max(0f, stepYmm);
        }

        private void BuildDefaultGrid(List<Crystal> crystals, float stepXmm, float stepYmm)
        {
            float radius = WaferDiameter / 2f;
            int index = 0;
            int rows = 0;
            int maxPerRow = 0;

            for (float y = -radius; y <= radius + 1e-6f; y += stepYmm)
            {
                var rowPoints = new List<float>();
                for (float x = -radius; x <= radius + 1e-6f; x += stepXmm)
                {
                    if (IsInsideWafer(x, y, radius))
                    {
                        rowPoints.Add(x);
                    }
                }

                if (rowPoints.Count == 0)
                {
                    continue;
                }

                if ((rows & 1) == 1)
                {
                    rowPoints.Reverse();
                }

                foreach (var x in rowPoints)
                {
                    crystals.Add(new Crystal
                    {
                        Index = index++,
                        RealX = x,
                        RealY = y,
                        Color = Color.Blue
                    });
                }

                rows++;
                if (rowPoints.Count > maxPerRow)
                {
                    maxPerRow = rowPoints.Count;
                }
            }

            CrystalsPerRow = maxPerRow;
            RowsTotal = rows;
        }

        private float BuildCalibratedGrid(List<Crystal> crystals, float stepXmm, float stepYmm)
        {
            PointF baseline = new PointF(lastRefMm.X - firstRefMm.X, lastRefMm.Y - firstRefMm.Y);
            double baselineLength = Math.Sqrt(baseline.X * baseline.X + baseline.Y * baseline.Y);

            if (baselineLength < 1e-6)
            {
                return 0f;
            }

            PointF rowUnit = Normalize(baseline);
            PointF normalUnit = new PointF(-rowUnit.Y, rowUnit.X);

            float radius = WaferDiameter / 2f;
            int maxStepsAlongRow = (int)Math.Ceiling(WaferDiameter / stepXmm) + 4;
            int maxRowOffset = (int)Math.Ceiling(WaferDiameter / stepYmm) + 4;

            int index = 0;
            int maxPerRow = 0;
            int rowsTotal = 0;
            bool reverse = false;

            for (int offset = 0; offset <= maxRowOffset; offset++)
            {
                bool anyRowCreated = false;
                int[] offsets = offset == 0 ? new[] { 0 } : new[] { offset, -offset };

                foreach (int signedOffset in offsets)
                {
                    PointF rowOrigin = new PointF(
                        firstRefMm.X + normalUnit.X * stepYmm * signedOffset,
                        firstRefMm.Y + normalUnit.Y * stepYmm * signedOffset);

                    var rowCrystals = GenerateCalibratedRow(rowOrigin, rowUnit, stepXmm, radius, maxStepsAlongRow, ref index);
                    if (rowCrystals.Count == 0)
                    {
                        continue;
                    }

                    anyRowCreated = true;
                    if (reverse)
                    {
                        rowCrystals.Reverse();
                    }

                    crystals.AddRange(rowCrystals);

                    if (rowCrystals.Count > maxPerRow)
                    {
                        maxPerRow = rowCrystals.Count;
                    }

                    rowsTotal++;
                    reverse = !reverse;
                }

                if (!anyRowCreated && offset > 0)
                {
                    break;
                }
            }

            CrystalsPerRow = maxPerRow;
            RowsTotal = rowsTotal;
            return (float)(Math.Atan2(rowUnit.Y, rowUnit.X) * 180.0 / Math.PI);
        }

        private static List<Crystal> GenerateCalibratedRow(
            PointF origin,
            PointF rowUnit,
            float stepXmm,
            float radius,
            int maxSteps,
            ref int index)
        {
            var candidates = new List<(float offset, Crystal crystal)>();

            for (int step = -maxSteps; step <= maxSteps; step++)
            {
                float offset = step * stepXmm;
                float x = origin.X + rowUnit.X * offset;
                float y = origin.Y + rowUnit.Y * offset;

                if (!IsInsideWafer(x, y, radius))
                {
                    continue;
                }

                candidates.Add((offset, new Crystal
                {
                    Index = index++,
                    RealX = x,
                    RealY = y,
                    Color = Color.Blue
                }));
            }

            candidates.Sort((a, b) => a.offset.CompareTo(b.offset));
            return candidates.Select(item => item.crystal).ToList();
        }

        private static PointF Normalize(PointF vector)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length < 1e-6f)
            {
                return new PointF(1f, 0f);
            }

            return new PointF(vector.X / length, vector.Y / length);
        }

        private static bool IsInsideWafer(float x, float y, float radius)
        {
            return x * x + y * y <= radius * radius + 1e-6f;
        }

        public bool CreateWaferFromInput(string sizeXRaw, string sizeYRaw, string diaRaw, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(sizeXRaw) ||
                string.IsNullOrWhiteSpace(sizeYRaw) ||
                string.IsNullOrWhiteSpace(diaRaw))
            {
                errorMessage = "Все поля должны быть заполнены.";
                return false;
            }

            bool okX = uint.TryParse(sizeXRaw, System.Globalization.NumberStyles.Integer, CrystalTable.CultureSettings.NumericCulture, out uint sizeX);
            bool okY = uint.TryParse(sizeYRaw, System.Globalization.NumberStyles.Integer, CrystalTable.CultureSettings.NumericCulture, out uint sizeY);
            bool okD = uint.TryParse(diaRaw, System.Globalization.NumberStyles.Integer, CrystalTable.CultureSettings.NumericCulture, out uint diameterMm);

            if (!okX || !okY || !okD || sizeX == 0 || sizeY == 0 || diameterMm == 0)
            {
                errorMessage = "Укажите корректные шаги (Размер X/Размер Y, мкм) и диаметр пластины (целые мм).";
                return false;
            }

            CrystalWidthRaw = sizeX;
            CrystalHeightRaw = sizeY;
            WaferDiameter = diameterMm;

            CreateNewWafer();
            return true;
        }
    }
}