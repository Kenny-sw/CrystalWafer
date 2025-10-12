using System;
using System.Collections.Generic;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    public sealed class WaferMapBuilder
    {
        public WaferMapBuildResult Build(WaferMapParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.DiameterMm <= 0f || parameters.CrystalWidthMm <= 0f || parameters.CrystalHeightMm <= 0f)
            {
                return WaferMapBuildResult.Empty;
            }

            float radius = parameters.DiameterMm / 2f;
            float width = parameters.CrystalWidthMm;
            float height = parameters.CrystalHeightMm;

            if (parameters.SwapOrientation)
            {
                (width, height) = (height, width);
            }

            float street = Math.Max(0f, parameters.StreetMm);
            float stepX = width + street;
            float stepY = height + street;

            if (stepX <= 0f || stepY <= 0f)
            {
                return WaferMapBuildResult.Empty;
            }

            var crystals = new List<Crystal>();
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            int limitX = (int)Math.Ceiling((radius + Math.Abs(parameters.OffsetXMm) + halfWidth) / stepX) + 2;
            int limitY = (int)Math.Ceiling((radius + Math.Abs(parameters.OffsetYMm) + halfHeight) / stepY) + 2;

            float edgeExclusion = Math.Max(0f, parameters.EdgeExclusionMm);
            float effectiveRadius = Math.Max(0f, radius - edgeExclusion);
            float effectiveRadiusSq = effectiveRadius * effectiveRadius;
            float radiusSq = radius * radius;

            int index = 0;
            int rowsTotal = 0;
            int maxPerRow = 0;

            for (int gy = -limitY; gy <= limitY; gy++)
            {
                float baseY = gy * stepY + parameters.OffsetYMm;
                float centerY = parameters.MirrorY ? -baseY : baseY;
                float rectTop = centerY - halfHeight;
                float rectBottom = centerY + halfHeight;

                if (rectBottom < -radius || rectTop > radius)
                {
                    continue;
                }

                int rowCount = 0;
                for (int gx = -limitX; gx <= limitX; gx++)
                {
                    float baseX = gx * stepX + parameters.OffsetXMm;
                    float centerX = parameters.MirrorX ? -baseX : baseX;
                    float rectLeft = centerX - halfWidth;
                    float rectRight = centerX + halfWidth;

                    if (rectRight < -radius || rectLeft > radius)
                    {
                        continue;
                    }

                    var status = ClassifyCell(centerX, centerY, halfWidth, halfHeight, radiusSq, effectiveRadiusSq);
                    if (status == null)
                    {
                        continue;
                    }

                    crystals.Add(new Crystal
                    {
                        Index = index++,
                        RealX = centerX,
                        RealY = centerY,
                        WidthMm = width,
                        HeightMm = height,
                        PlacementStatus = status.Value
                    });

                    rowCount++;
                }

                if (rowCount > 0)
                {
                    rowsTotal++;
                    if (rowCount > maxPerRow)
                    {
                        maxPerRow = rowCount;
                    }
                }
            }

            return crystals.Count == 0
                ? WaferMapBuildResult.Empty
                : new WaferMapBuildResult(width, height, stepX, stepY, rowsTotal, maxPerRow, crystals);
        }

        private static CrystalPlacementStatus? ClassifyCell(float centerX,
                                                            float centerY,
                                                            float halfWidth,
                                                            float halfHeight,
                                                            float radiusSq,
                                                            float effectiveRadiusSq)
        {
            bool allInsideOuter = true;
            bool anyInsideOuter = false;
            bool insideEffective = effectiveRadiusSq > 0f;

            for (int i = 0; i < 4; i++)
            {
                float dx = (i % 2 == 0 ? -halfWidth : halfWidth);
                float dy = (i / 2 == 0 ? -halfHeight : halfHeight);
                float cornerX = centerX + dx;
                float cornerY = centerY + dy;
                float distSq = cornerX * cornerX + cornerY * cornerY;

                if (distSq <= radiusSq + 1e-6f)
                {
                    anyInsideOuter = true;
                }
                else
                {
                    allInsideOuter = false;
                }

                if (effectiveRadiusSq > 0f && distSq > effectiveRadiusSq + 1e-6f)
                {
                    insideEffective = false;
                }
            }

            if (effectiveRadiusSq > 0f && !insideEffective)
            {
                return null;
            }

            if (allInsideOuter)
            {
                return CrystalPlacementStatus.Full;
            }

            if (!anyInsideOuter && !RectangleIntersectsCircle(centerX, centerY, halfWidth, halfHeight, radiusSq))
            {
                return null;
            }

            return CrystalPlacementStatus.Partial;
        }

        private static bool RectangleIntersectsCircle(float centerX,
                                                       float centerY,
                                                       float halfWidth,
                                                       float halfHeight,
                                                       float radiusSq)
        {
            float rectLeft = centerX - halfWidth;
            float rectRight = centerX + halfWidth;
            float rectTop = centerY - halfHeight;
            float rectBottom = centerY + halfHeight;

            float nearestX = Clamp(0f, rectLeft, rectRight);
            float nearestY = Clamp(0f, rectTop, rectBottom);

            float dx = nearestX;
            float dy = nearestY;
            float distanceSq = dx * dx + dy * dy;
            if (distanceSq <= radiusSq + 1e-6f)
            {
                return true;
            }

            bool containsCenter = rectLeft <= 0f && rectRight >= 0f && rectTop <= 0f && rectBottom >= 0f;
            return containsCenter;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    public sealed class WaferMapBuildResult
    {
        public static readonly WaferMapBuildResult Empty = new WaferMapBuildResult(0f, 0f, 0f, 0f, 0, 0, new List<Crystal>());

        public WaferMapBuildResult(float cellWidthMm,
                                   float cellHeightMm,
                                   float stepXmm,
                                   float stepYmm,
                                   int rowsTotal,
                                   int maxPerRow,
                                   List<Crystal> crystals)
        {
            CellWidthMm = cellWidthMm;
            CellHeightMm = cellHeightMm;
            StepXmm = stepXmm;
            StepYmm = stepYmm;
            RowsTotal = rowsTotal;
            CrystalsPerRow = maxPerRow;
            Crystals = crystals ?? new List<Crystal>();
        }

        public float CellWidthMm { get; }
        public float CellHeightMm { get; }
        public float StepXmm { get; }
        public float StepYmm { get; }
        public int RowsTotal { get; }
        public int CrystalsPerRow { get; }
        public List<Crystal> Crystals { get; }
    }
}
