using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Контроллер для управления пластиной и кристаллами
    /// </summary>
    public class WaferController
    {
        private readonly Form1 form;

        // Основные параметры
        public float WaferDiameter { get; set; } = 100f;
        public float ScaleFactor { get; set; } = 4.0f;
        public float CrystalWidthRaw { get; set; }
        public float CrystalHeightRaw { get; set; }

        // Константы
        public const float MinWaferDiameter = 50f;
        public const float MaxWaferDiameter = 300f;

        // Кеширование
        private float lastCrystalWidthRaw = -1f;
        private float lastCrystalHeightRaw = -1f;
        private float lastWaferDiameter = -1f;

        // Временные переменные для валидации
        public float SizeXtemp = 0;
        public float SizeYtemp = 0;
        public float WaferDiameterTemp = 0;

        // Режим отображения
        public bool WaferDisplayMode { get; set; } = false;

        // Счетчик кристаллов
        private int nextCrystalIndex = 1;

        public WaferController(Form1 form)
        {
            this.form = form;
        }

        /// <summary>
        /// Создает новую пластину
        /// </summary>
        public void CreateNewWafer()
        {
            CrystalManager.Instance.Crystals.Clear();
            nextCrystalIndex = 1;
            lastCrystalWidthRaw = -1f;
            lastCrystalHeightRaw = -1f;
            lastWaferDiameter = -1f;
        }

        /// <summary>
        /// Автоматическая установка масштаба
        /// </summary>
        public void AutoSetScaleFactor(int pictureBoxWidth, int pictureBoxHeight)
        {
            float minSide = Math.Min(pictureBoxWidth, pictureBoxHeight);
            ScaleFactor = minSide / WaferDiameter * 0.9f;
        }

        /// <summary>
        /// Построение кристаллов с кешированием
        /// </summary>
        public void BuildCrystalsCached()
        {
            if (CrystalWidthRaw == lastCrystalWidthRaw &&
                CrystalHeightRaw == lastCrystalHeightRaw &&
                WaferDiameter == lastWaferDiameter &&
                CrystalManager.Instance.Crystals.Count > 0)
            {
                return; // Кеш актуален
            }

            string cachePath = CrystalCache.GetCacheFilePath(CrystalWidthRaw, CrystalHeightRaw, WaferDiameter);
            if (CrystalCache.TryLoad(cachePath, out var cachedCrystals))
            {
                CrystalManager.Instance.Crystals.Clear();
                CrystalManager.Instance.Crystals.AddRange(cachedCrystals);
            }
            else
            {
                BuildCrystals();

                var info = new WaferInfo
                {
                    SizeX = (uint)CrystalWidthRaw,
                    SizeY = (uint)CrystalHeightRaw,
                    WaferDiameter = (uint)WaferDiameter
                };
                CrystalCache.Save(cachePath, info, CrystalManager.Instance.Crystals);
            }

            lastCrystalWidthRaw = CrystalWidthRaw;
            lastCrystalHeightRaw = CrystalHeightRaw;
            lastWaferDiameter = WaferDiameter;
        }

        /// <summary>
        /// Построение кристаллов
        /// </summary>
        private void BuildCrystals()
        {
            CrystalManager.Instance.Crystals.Clear();
            nextCrystalIndex = 1;

            float waferRadius = WaferDiameter / 2;
            float crystalWidth = CrystalWidthRaw / 1000f;
            float crystalHeight = CrystalHeightRaw / 1000f;

            float startX = -waferRadius;
            float endX = waferRadius;
            float startY = -waferRadius;
            float endY = waferRadius;

            int numCrystalsX = (int)((endX - startX) / crystalWidth);
            int numCrystalsY = (int)((endY - startY) / crystalHeight);

            int totalCrystals = 0;
            bool isReversed = false;
            List<Crystal> rowCrystals = new List<Crystal>();

            for (int j = 0; j <= numCrystalsY; j++)
            {
                rowCrystals.Clear();
                for (int i = 0; i <= numCrystalsX; i++)
                {
                    float crystalX = startX + i * crystalWidth + crystalWidth / 2;
                    float crystalY = startY + j * crystalHeight + crystalHeight / 2;

                    if ((crystalX * crystalX + crystalY * crystalY) <= (waferRadius * waferRadius))
                    {
                        var crystal = new Crystal
                        {
                            Index = nextCrystalIndex++,
                            RealX = crystalX,
                            RealY = crystalY,
                            Color = Color.Blue
                        };

                        rowCrystals.Add(crystal);
                        totalCrystals++;
                    }
                }

                if (isReversed)
                {
                    rowCrystals.Reverse();
                    for (int i = 0; i < rowCrystals.Count; i++)
                    {
                        rowCrystals[i].Index = nextCrystalIndex - rowCrystals.Count + i;
                    }
                }

                CrystalManager.Instance.Crystals.AddRange(rowCrystals);
                isReversed = !isReversed;
            }

            form.LabelTotalCrystals.Text = $"Общее количество кристаллов: {totalCrystals}";
        }

        /// <summary>
        /// Генерирует маршрут для предпросмотра
        /// </summary>
        public void GenerateRoute(RoutePreview routePreview, HashSet<int> selectedCrystals)
        {
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

        /// <summary>
        /// Получает статистику пластины
        /// </summary>
        public WaferStatistics GetStatistics()
        {
            if (CrystalManager.Instance.Crystals.Count == 0)
                return null;

            return new WaferStatistics(CrystalManager.Instance.Crystals, WaferDiameter);
        }
    }
}