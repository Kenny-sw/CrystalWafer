using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    public class WaferController : IDisposable
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

        private readonly WaferMapBuilder mapBuilder = new WaferMapBuilder();
        private readonly WaferBitmapRenderer bitmapRenderer = new WaferBitmapRenderer();
        private WaferMapParameters activeParameters;
        private WaferMapParameters draftParameters;
        private int mapVersion;
        private Size displayCacheSize;
        private float displayCacheScale;
        private bool displayCacheValid;

        public bool HasActiveMap => activeParameters != null;
        public bool IsMapEditing => draftParameters != null;
        public int MapVersion => mapVersion;

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

        public void Dispose()
        {
            bitmapRenderer.Dispose();
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
            float newScale = Math.Max(1f, Math.Min(sx, sy));
            if (Math.Abs(newScale - ScaleFactor) > 1e-6f)
            {
                ScaleFactor = newScale;
                displayCacheValid = false;
            }
        }

        public void CreateNewWafer()
        {
            firstRefSet = false;
            lastRefSet = false;
            StepXmm = 0f;
            StepYmm = 0f;
            RotationAngleDeg = 0f;
            activeParameters = null;
            draftParameters = null;
            CrystalManager.Instance.Crystals.Clear();
            mapVersion++;
            displayCacheValid = false;
        }

        public void BuildCrystalsCached()
        {
            var parameters = draftParameters ?? activeParameters;
            if (parameters != null)
            {
                BuildFromParameters(parameters, draftParameters == null);
                return;
            }

            BuildLegacyFactory();
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
            draftParameters = null;
            activeParameters = null;

            if (StepXmm <= 0f)
            {
                StepXmm = CrystalWidthRaw / 1000f;
            }

            if (StepYmm <= 0f)
            {
                StepYmm = CrystalHeightRaw / 1000f;
            }

            BuildLegacyFactory();
        }

        public void BuildMapFromPreset()
        {
            draftParameters = null;
            activeParameters = null;

            if (StepXmm <= 0f)
            {
                StepXmm = CrystalWidthRaw / 1000f;
            }

            if (StepYmm <= 0f)
            {
                StepYmm = CrystalHeightRaw / 1000f;
            }

            BuildLegacyFactory();
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
            displayCacheValid = false;
        }

        public bool CreateWaferFromInput(string sizeXRaw, string sizeYRaw, string diaRaw, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(sizeXRaw) ||
                string.IsNullOrWhiteSpace(sizeYRaw) ||
                string.IsNullOrWhiteSpace(diaRaw))
            {
                errorMessage = "Заполните размеры кристалла и диаметр пластины.";
                return false;
            }

            bool okX = uint.TryParse(sizeXRaw, System.Globalization.NumberStyles.Integer, CrystalTable.CultureSettings.NumericCulture, out uint sizeX);
            bool okY = uint.TryParse(sizeYRaw, System.Globalization.NumberStyles.Integer, CrystalTable.CultureSettings.NumericCulture, out uint sizeY);
            bool okD = uint.TryParse(diaRaw, System.Globalization.NumberStyles.Integer, CrystalTable.CultureSettings.NumericCulture, out uint diameterMm);

            if (!okX || !okY || !okD || sizeX == 0 || sizeY == 0 || diameterMm == 0)
            {
                errorMessage = "Размеры X/Y и диаметр должны быть положительными целыми числами (мм).";
                return false;
            }

            CrystalWidthRaw = sizeX;
            CrystalHeightRaw = sizeY;
            WaferDiameter = diameterMm;
            WaferDiameterTemp = WaferDiameter;

            CreateNewWafer();
            return true;
        }

        public WaferMapParameters GetActiveMapSnapshot() => activeParameters?.Clone();
        public WaferMapParameters GetDraftMapSnapshot() => draftParameters?.Clone();
        public WaferMapParameters GetEffectiveMapSnapshot() => (draftParameters ?? activeParameters)?.Clone();

        public void LoadActiveMap(WaferMapParameters parameters)
        {
            draftParameters = null;
            BuildFromParameters(parameters, true);
        }
        public void SetActiveMapMetadata(WaferMapParameters parameters)
        {
            activeParameters = parameters?.Clone();
            draftParameters = null;
            mapVersion++;
            displayCacheValid = false;
        }

        public void BeginMapCreation(WaferMapParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            draftParameters = parameters.Clone();
            ClampParameterValues(draftParameters);
            ClearReferences();
            BuildFromParameters(draftParameters, false);
        }

        public void BeginMapEdit()
        {
            if (activeParameters == null)
            {
                throw new InvalidOperationException("Active wafer map is not available.");
            }

            draftParameters = activeParameters.Clone();
            BuildFromParameters(draftParameters, false);
        }

        public void ApplyDraftMap()
        {
            if (draftParameters == null)
            {
                return;
            }

            BuildFromParameters(draftParameters, true);
            draftParameters = null;
        }

        public void CancelDraftMap()
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters = null;
            if (activeParameters != null)
            {
                BuildFromParameters(activeParameters, true);
            }
            else
            {
                CrystalManager.Instance.Crystals.Clear();
                mapVersion++;
                displayCacheValid = false;
            }
        }

        public void UpdateDraftOffsets(float offsetXMm, float offsetYMm)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.OffsetXMm = offsetXMm;
            draftParameters.OffsetYMm = offsetYMm;
            BuildFromParameters(draftParameters, false);
        }

        public void NudgeDraftOffsets(float deltaXMm, float deltaYMm)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.OffsetXMm += deltaXMm;
            draftParameters.OffsetYMm += deltaYMm;
            BuildFromParameters(draftParameters, false);
        }

        public void UpdateDraftCrystalSize(float widthMm, float heightMm)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.CrystalWidthMm = Math.Max(0.01f, widthMm);
            draftParameters.CrystalHeightMm = Math.Max(0.01f, heightMm);
            BuildFromParameters(draftParameters, false);
        }

        public void UpdateDraftStreet(float streetMm)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.StreetMm = Math.Max(0f, streetMm);
            BuildFromParameters(draftParameters, false);
        }

        public void UpdateDraftDiameter(float diameterMm)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.DiameterMm = ClampDiameter(diameterMm);
            BuildFromParameters(draftParameters, false);
        }

        public void ToggleDraftOrientation()
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.SwapOrientation = !draftParameters.SwapOrientation;
            (draftParameters.CrystalWidthMm, draftParameters.CrystalHeightMm) = (draftParameters.CrystalHeightMm, draftParameters.CrystalWidthMm);
            BuildFromParameters(draftParameters, false);
        }

        public void SetDraftMirror(bool mirrorX, bool mirrorY)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.MirrorX = mirrorX;
            draftParameters.MirrorY = mirrorY;
            BuildFromParameters(draftParameters, false);
        }

        public void UpdateDraftEdgeExclusion(float edgeMm)
        {
            if (draftParameters == null)
            {
                return;
            }

            draftParameters.EdgeExclusionMm = Math.Max(0f, Math.Min(edgeMm, draftParameters.DiameterMm / 2f));
            BuildFromParameters(draftParameters, false);
        }

        public void UpdateDisplayCache(int viewportWidth, int viewportHeight)
        {
            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                displayCacheValid = false;
                return;
            }

            if (displayCacheValid && displayCacheSize.Width == viewportWidth && displayCacheSize.Height == viewportHeight && Math.Abs(displayCacheScale - ScaleFactor) < 1e-3f)
            {
                return;
            }

            float cx = viewportWidth / 2f;
            float cy = viewportHeight / 2f;
            float scale = ScaleFactor;

            foreach (var crystal in CrystalManager.Instance.Crystals)
            {
                float widthPx = crystal.WidthMm * scale;
                float heightPx = crystal.HeightMm * scale;
                float centerX = crystal.RealX * scale + cx;
                float centerY = crystal.RealY * scale + cy;

                crystal.DisplayX = centerX;
                crystal.DisplayY = centerY;
                crystal.DisplayLeft = centerX - widthPx / 2f;
                crystal.DisplayTop = centerY - heightPx / 2f;
                crystal.DisplayRight = centerX + widthPx / 2f;
                crystal.DisplayBottom = centerY + heightPx / 2f;
            }

            displayCacheSize = new Size(viewportWidth, viewportHeight);
            displayCacheScale = scale;
            displayCacheValid = true;
        }

        public Bitmap GetWaferBitmap(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            if (!displayCacheValid || displayCacheSize.Width != width || displayCacheSize.Height != height)
            {
                UpdateDisplayCache(width, height);
            }

            return bitmapRenderer.Render(CrystalManager.Instance.Crystals, WaferDiameter, width, height, ScaleFactor, mapVersion);
        }

        private void BuildFromParameters(WaferMapParameters source, bool commitToActive)
        {
            var snapshot = source?.Clone();
            if (snapshot == null)
            {
                if (commitToActive)
                {
                    activeParameters = null;
                }
                else
                {
                    draftParameters = null;
                }

                CrystalManager.Instance.Crystals.Clear();
                CrystalsPerRow = 0;
                RowsTotal = 0;
                StepXmm = 0f;
                StepYmm = 0f;
                mapVersion++;
                displayCacheValid = false;
                return;
            }

            ClampParameterValues(snapshot);
            var result = mapBuilder.Build(snapshot);
            ApplyMapResult(snapshot, result, commitToActive);
        }

        private void ApplyMapResult(WaferMapParameters snapshot, WaferMapBuildResult result, bool commitToActive)
        {
            var store = CrystalManager.Instance.Crystals;
            store.Clear();
            if (result != null && result.Crystals.Count > 0)
            {
                store.AddRange(result.Crystals);
            }

            WaferDiameter = snapshot.DiameterMm;
            WaferDiameterTemp = WaferDiameter;

            float widthMm = result?.CellWidthMm > 0f ? result.CellWidthMm : snapshot.CrystalWidthMm;
            float heightMm = result?.CellHeightMm > 0f ? result.CellHeightMm : snapshot.CrystalHeightMm;

            StepXmm = result?.StepXmm > 0f ? result.StepXmm : widthMm;
            StepYmm = result?.StepYmm > 0f ? result.StepYmm : heightMm;
            CrystalsPerRow = result?.CrystalsPerRow ?? 0;
            RowsTotal = result?.RowsTotal ?? 0;

            CrystalWidthRaw = (uint)Math.Max(1, Math.Round(widthMm * 1000f));
            CrystalHeightRaw = (uint)Math.Max(1, Math.Round(heightMm * 1000f));
            SizeXtemp = CrystalWidthRaw;
            SizeYtemp = CrystalHeightRaw;

            if (commitToActive)
            {
                activeParameters = snapshot;
            }
            else
            {
                draftParameters = snapshot;
            }

            RotationAngleDeg = 0f;
            mapVersion++;
            displayCacheValid = false;
        }

        private void BuildLegacyFactory()
        {
            var crystals = CrystalManager.Instance.Crystals;
            crystals.Clear();

            float stepX = StepXmm > 0f ? StepXmm : CrystalWidthRaw / 1000f;
            float stepY = StepYmm > 0f ? StepYmm : CrystalHeightRaw / 1000f;

            if (stepX <= 0f || stepY <= 0f || WaferDiameter <= 0f)
            {
                CrystalsPerRow = 0;
                RowsTotal = 0;
                mapVersion++;
                displayCacheValid = false;
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

            foreach (var crystal in crystals)
            {
                crystal.WidthMm = stepX;
                crystal.HeightMm = stepY;
                crystal.PlacementStatus = CrystalPlacementStatus.Full;
            }

            mapVersion++;
            displayCacheValid = false;
        }

        private void BuildDefaultGrid(List<Crystal> crystals, float stepXmm, float stepYmm)
        {
            float radius = WaferDiameter / 2f;
            int index = 0;
            int rows = 0;
            int maxPerRow = 0;
            float widthMm = stepXmm;
            float heightMm = stepYmm;

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
                        WidthMm = widthMm,
                        HeightMm = heightMm,
                        PlacementStatus = CrystalPlacementStatus.Full,
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

            float widthMm = stepXmm;
            float heightMm = stepYmm;

            for (int offset = 0; offset <= maxRowOffset; offset++)
            {
                bool anyRowCreated = false;
                int[] offsets = offset == 0 ? new[] { 0 } : new[] { offset, -offset };

                foreach (int signedOffset in offsets)
                {
                    PointF rowOrigin = new PointF(
                        firstRefMm.X + normalUnit.X * stepYmm * signedOffset,
                        firstRefMm.Y + normalUnit.Y * stepYmm * signedOffset);

                    var rowCrystals = GenerateCalibratedRow(rowOrigin, rowUnit, stepXmm, radius, maxStepsAlongRow, ref index, widthMm, heightMm);
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
            ref int index,
            float widthMm,
            float heightMm)
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
                    WidthMm = widthMm,
                    HeightMm = heightMm,
                    PlacementStatus = CrystalPlacementStatus.Full,
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

        private static void ClampParameterValues(WaferMapParameters parameters)
        {
            parameters.DiameterMm = ClampDiameter(parameters.DiameterMm);
            parameters.CrystalWidthMm = Math.Max(0.01f, parameters.CrystalWidthMm);
            parameters.CrystalHeightMm = Math.Max(0.01f, parameters.CrystalHeightMm);
            parameters.StreetMm = Math.Max(0f, parameters.StreetMm);
            parameters.OffsetXMm = float.IsNaN(parameters.OffsetXMm) ? 0f : parameters.OffsetXMm;
            parameters.OffsetYMm = float.IsNaN(parameters.OffsetYMm) ? 0f : parameters.OffsetYMm;
            parameters.EdgeExclusionMm = Math.Max(0f, Math.Min(parameters.EdgeExclusionMm, parameters.DiameterMm / 2f));
            parameters.NotchWidthMm = Math.Max(0f, parameters.NotchWidthMm);
        }

        private static float ClampDiameter(float diameterMm)
        {
            return Math.Max(MinWaferDiameter, Math.Min(MaxWaferDiameter, diameterMm));
        }
    }
}

