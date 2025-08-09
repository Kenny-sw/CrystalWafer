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

        // Основные параметры (из UI): шаги в мкм, диаметр в мм
        public float WaferDiameter { get; set; } = 100f;
        public float ScaleFactor { get; set; } = 4.0f;
        public float CrystalWidthRaw { get; set; }   // в мкм
        public float CrystalHeightRaw { get; set; }  // в мкм

        // Константы
        public const float MinWaferDiameter = 50f;
        public const float MaxWaferDiameter = 300f;

        // Кеширование
        private float lastCrystalWidthRaw = -1f;
        private float lastCrystalHeightRaw = -1f;
        private float lastWaferDiameter = -1f;

        // Временные переменные для валидации (используются UI)
        public float SizeXtemp = 0;
        public float SizeYtemp = 0;
        public float WaferDiameterTemp = 0;

        // Режим отображения
        public bool WaferDisplayMode { get; set; } = false;

        // Счетчик кристаллов
        private int nextCrystalIndex = 1;

        // ======= Карта/калибровка по ТЗ =======
        public bool MapLocked { get; private set; } = false;   // если true — BuildCrystalsCached() не трогает карту
        public float StepXmm { get; private set; }             // шаги в мм (из Raw или пресета)
        public float StepYmm { get; private set; }
        public int CrystalsPerRow { get; private set; }        // Nx
        public int RowsTotal { get; private set; }             // Ny

        // Две опорные точки (в мм, центр кристалла)
        private float? firstRefX, firstRefY;
        private float? lastRefX, lastRefY;

        public WaferController(Form1 form)
        {
            this.form = form;
        }

        // ---------- Публичное API калибровки/карты ----------
        public void SetFirstReference(float xMm, float yMm)
        {
            firstRefX = xMm; firstRefY = yMm;
            MapLocked = false; // новая калибровка — снимаем блокировку
        }

        public void SetLastReference(float xMm, float yMm)
        {
            lastRefX = xMm; lastRefY = yMm;
            MapLocked = false;
        }

        public bool IsCalibrationReady() =>
            firstRefX.HasValue && firstRefY.HasValue && lastRefX.HasValue && lastRefY.HasValue;

        public bool IsPresetReady() =>
            CrystalWidthRaw > 0 && CrystalHeightRaw > 0 &&
            WaferDiameter >= MinWaferDiameter && WaferDiameter <= MaxWaferDiameter;

        /// <summary>
        /// Построить карту по двум якорям (угол игнорируем, оси параллельны X/Y).
        /// A = первый (лево-верх), B = последний (право-низ).
        /// </summary>
        public void BuildMapFromReferences()
        {
            EnsureStepsFromRaw();

            if (!IsCalibrationReady())
                throw new InvalidOperationException("Не выбраны обе опорные точки");

            // A и B в корректном порядке
            var ax = firstRefX!.Value; var ay = firstRefY!.Value;
            var bx = lastRefX!.Value; var by = lastRefY!.Value;
            if (bx < ax) (ax, bx) = (bx, ax);
            if (by < ay) (ay, by) = (by, ay);

            int nx = Math.Max(1, (int)Math.Round((bx - ax) / StepXmm) + 1);
            int ny = Math.Max(1, (int)Math.Round((by - ay) / StepYmm) + 1);

            GenerateGrid(new PointF(ax, ay), nx, ny);

            CrystalsPerRow = nx;
            RowsTotal = ny;
            MapLocked = true;
            form.LabelTotalCrystals.Text = $"Общее количество кристаллов: {nx * ny}";
        }

        /// <summary>
        /// Построить карту из текущих настроек (без якорей): сетка центрирована, угол = 0°.
        /// </summary>
        public void BuildMapFromCurrentSettings()
        {
            EnsureStepsFromRaw();

            float R = WaferDiameter / 2f;

            // сколько центров умещается в радиусе по шагу
            int halfCols = Math.Max(0, (int)Math.Floor(R / StepXmm));
            int halfRows = Math.Max(0, (int)Math.Floor(R / StepYmm));
            int nx = 2 * halfCols + 1;
            int ny = 2 * halfRows + 1;

            float startX = -halfCols * StepXmm;
            float startY = -halfRows * StepYmm;

            GenerateGrid(new PointF(startX, startY), nx, ny);

            CrystalsPerRow = nx;
            RowsTotal = ny;
            MapLocked = true;
            form.LabelTotalCrystals.Text = $"Общее количество кристаллов: {nx * ny}";
        }

        /// <summary>
        /// Построить карту из ПРЕСЕТА: Nx/Ny/шаги обязательны; якоря опциональны.
        /// - Если переданы якоря → используем их (угол = 0°, оси X/Y), шаги берём из пресета.
        /// - Если якорей нет → сетка центрирована, FirstRef = верхний-левый центр, LastRef вычисляется.
        /// </summary>
        public void BuildMapFromPreset(int nx, int ny, float pitchXum, float pitchYum,
                                       PointF? firstRefMm = null, PointF? lastRefMm = null)
        {
            if (nx <= 0 || ny <= 0)
                throw new ArgumentException("Nx/Ny должны быть > 0.");

            if (pitchXum <= 0 || pitchYum <= 0)
                throw new ArgumentException("PitchX/PitchY должны быть > 0 мкм.");

            // Зафиксируем шаги в мм и в raw для совместимости с остальной логикой/кешем
            CrystalWidthRaw = pitchXum;
            CrystalHeightRaw = pitchYum;
            StepXmm = pitchXum / 1000f;
            StepYmm = pitchYum / 1000f;

            if (firstRefMm.HasValue && lastRefMm.HasValue)
            {
                // Прямо используем якоря
                firstRefX = firstRefMm.Value.X; firstRefY = firstRefMm.Value.Y;
                lastRefX = lastRefMm.Value.X; lastRefY = lastRefMm.Value.Y;

                // Пересоберём по якорям (Nx/Ny будут вычислены по геометрии и шагам)
                BuildMapFromReferences();
                return;
            }

            // Без якорей — центрируем сетку, FirstRef = верхний-левый центр
            float startX = -((nx - 1) / 2f) * StepXmm;
            float startY = -((ny - 1) / 2f) * StepYmm;

            GenerateGrid(new PointF(startX, startY), nx, ny);

            CrystalsPerRow = nx;
            RowsTotal = ny;
            MapLocked = true;
            form.LabelTotalCrystals.Text = $"Общее количество кристаллов: {nx * ny}";
        }

        // ---------- Базовая логика проекта (оставлена без изменений по контракту) ----------

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

            // Сброс калибровки/карты
            firstRefX = firstRefY = lastRefX = lastRefY = null;
            MapLocked = false;
            CrystalsPerRow = RowsTotal = 0;
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
            // Новое: если карта уже построена (по ТЗ), ничего не делаем — не перетирать! 
            if (MapLocked && CrystalManager.Instance.Crystals.Count > 0)
                return;

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
                BuildCrystals(); // центрированная сетка по кругу (старый режим)

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
        /// Построение кристаллов (старый режим: только по кругу, без частичных).
        /// Оставляем для совместимости/кеша.
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

        // ---------- Внутренние помощники ----------
        private void EnsureStepsFromRaw()
        {
            if (CrystalWidthRaw <= 0 || CrystalHeightRaw <= 0)
                throw new InvalidOperationException("Задайте корректные шаги по X/Y.");

            StepXmm = CrystalWidthRaw / 1000f;
            StepYmm = CrystalHeightRaw / 1000f;

            if (WaferDiameter <= 0)
                throw new InvalidOperationException("Задайте корректный диаметр пластины.");
        }

        private void GenerateGrid(PointF start, int nx, int ny)
        {
            CrystalManager.Instance.Crystals.Clear();
            nextCrystalIndex = 1;

            bool reverse = false;
            for (int j = 0; j < ny; j++)
            {
                if (!reverse)
                {
                    for (int i = 0; i < nx; i++)
                    {
                        float x = start.X + i * StepXmm;
                        float y = start.Y + j * StepYmm;
                        CrystalManager.Instance.Crystals.Add(new Crystal
                        {
                            Index = nextCrystalIndex++,
                            RealX = x,
                            RealY = y,
                            Color = Color.Blue
                        });
                    }
                }
                else
                {
                    for (int i = nx - 1; i >= 0; i--)
                    {
                        float x = start.X + i * StepXmm;
                        float y = start.Y + j * StepYmm;
                        CrystalManager.Instance.Crystals.Add(new Crystal
                        {
                            Index = nextCrystalIndex++,
                            RealX = x,
                            RealY = y,
                            Color = Color.Blue
                        });
                    }
                }
                reverse = !reverse;
            }
        }
    }
}
