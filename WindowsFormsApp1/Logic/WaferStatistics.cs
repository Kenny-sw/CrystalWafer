using System;
using System.Collections.Generic;
using System.Linq;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    /// <summary>
    /// Класс для расчета статистики по пластине
    /// </summary>
    public class WaferStatistics
    {
        private readonly List<Crystal> crystals;
        private readonly float waferDiameter;

        /// <summary>
        /// Конструктор класса статистики
        /// </summary>
        /// <param name="crystals">Список кристаллов на пластине</param>
        /// <param name="waferDiameter">Диаметр пластины в мм</param>
        public WaferStatistics(List<Crystal> crystals, float waferDiameter)
        {
            this.crystals = crystals ?? new List<Crystal>();
            this.waferDiameter = waferDiameter;
        }

        /// <summary>
        /// Рассчитывает процент заполнения пластины кристаллами
        /// </summary>
        /// <param name="crystalWidth">Ширина кристалла в мм</param>
        /// <param name="crystalHeight">Высота кристалла в мм</param>
        /// <returns>Процент заполнения</returns>
        public float CalculateFillPercentage(float crystalWidth, float crystalHeight)
        {
            if (waferDiameter <= 0 || crystalWidth <= 0 || crystalHeight <= 0)
                return 0;

            // Площадь пластины (круг)
            float waferArea = (float)(Math.PI * Math.Pow(waferDiameter / 2, 2));

            // Площадь всех кристаллов
            float totalCrystalArea = crystals.Count * crystalWidth * crystalHeight;

            // Процент заполнения
            return (totalCrystalArea / waferArea) * 100f;
        }

        /// <summary>
        /// Получает распределение кристаллов по рядам
        /// </summary>
        /// <returns>Словарь: номер ряда -> количество кристаллов</returns>
        public Dictionary<int, int> GetRowDistribution()
        {
            var distribution = new Dictionary<int, int>();

            if (crystals.Count == 0)
                return distribution;

            // Группируем по Y-координате (ряды)
            var rows = crystals
                .GroupBy(c => Math.Round(c.RealY, 1))
                .OrderBy(g => g.Key)
                .ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                distribution[i] = rows[i].Count();
            }

            return distribution;
        }

        /// <summary>
        /// Получает распределение кристаллов по колонкам
        /// </summary>
        /// <returns>Словарь: номер колонки -> количество кристаллов</returns>
        public Dictionary<int, int> GetColumnDistribution()
        {
            var distribution = new Dictionary<int, int>();

            if (crystals.Count == 0)
                return distribution;

            // Группируем по X-координате (колонки)
            var columns = crystals
                    .GroupBy(c => Math.Round(c.RealX, 1))
                .OrderBy(g => g.Key)
                    .ToList();

            for (int i = 0; i < columns.Count; i++)
            {
                distribution[i] = columns[i].Count();
            }

            return distribution;
        }

        /// <summary>
        /// Получает статистику по радиальному распределению (зоны от центра)
        /// </summary>
        /// <param name="zones">Количество концентрических зон</param>
        /// <returns>Словарь: зона (0=центр) -> количество кристаллов</returns>
        public Dictionary<int, int> GetRadialZoneDistribution(int zones = 5)
        {
            var distribution = new Dictionary<int, int>();

            if (zones <= 0 || waferDiameter <= 0)
                return distribution;

            float radius = waferDiameter / 2;
            float zoneSize = radius / zones;

            // Инициализация зон
            for (int i = 0; i < zones; i++)
            {
                distribution[i] = 0;
            }

            // Подсчет кристаллов в каждой зоне
            foreach (var crystal in crystals)
            {
                // Расстояние от центра
                float distance = (float)Math.Sqrt(
                    crystal.RealX * crystal.RealX +
                    crystal.RealY * crystal.RealY);

                // Определяем в какую зону попадает
                int zoneIndex = (int)(distance / zoneSize);
                if (zoneIndex >= zones)
                    zoneIndex = zones - 1;

                distribution[zoneIndex]++;
            }

            return distribution;
        }

        /// <summary>
        /// Получает оценку времени сканирования маршрута
        /// </summary>
        /// <param name="crystalWidth">Ширина кристалла в мм</param>
        /// <param name="crystalHeight">Высота кристалла в мм</param>
        /// <param name="speedMmPerSec">Скорость перемещения в мм/с</param>
        /// <param name="measurementTimeMs">Время измерения на кристалл в мс</param>
        /// <returns>Оценка времени в секундах</returns>
        public float EstimateScanTime(float crystalWidth, float crystalHeight, 
            float speedMmPerSec = 10f, float measurementTimeMs = 100f)
        {
            if (crystals.Count == 0 || speedMmPerSec <= 0)
                return 0;

            // Оценка общего расстояния (змейка по рядам)
            float totalDistance = 0;
            var rows = crystals
                .GroupBy(c => Math.Round(c.RealY, 1))
                .OrderBy(g => g.Key)
                .ToList();

            for (int i = 0; i < rows.Count - 1; i++)
            {
                var currentRow = rows[i].OrderBy(c => c.RealX).ToList();
                var nextRow = rows[i + 1].OrderBy(c => c.RealX).ToList();

                // Расстояние внутри ряда
                for (int j = 0; j < currentRow.Count - 1; j++)
                {
                    totalDistance += crystalWidth;
                }

                // Переход между рядами
                if (currentRow.Count > 0 && nextRow.Count > 0)
                {
                    totalDistance += crystalHeight;
                }
            }

            float travelTime = totalDistance / speedMmPerSec;
            float measurementTime = (crystals.Count * measurementTimeMs) / 1000f;

            return travelTime + measurementTime;
        }

        /// <summary>
        /// Проверяет корректность исключения краев пластины
        /// </summary>
        /// <param name="edgeExclusionMm">Размер зоны исключения от края в мм</param>
        /// <returns>Информация о кристаллах в зоне исключения</returns>
        public Dictionary<string, object> ValidateEdgeExclusion(float edgeExclusionMm)
        {
            if (waferDiameter <= 0)
                return new Dictionary<string, object>();

            float maxRadius = waferDiameter / 2f;
            float minRadius = maxRadius - edgeExclusionMm;

            int inExclusionZone = 0;
            int outsideWafer = 0;

            foreach (var crystal in crystals)
            {
                float distance = (float)Math.Sqrt(
                    crystal.RealX * crystal.RealX +
                    crystal.RealY * crystal.RealY);

                if (distance > maxRadius)
                    outsideWafer++;
                else if (distance > minRadius)
                    inExclusionZone++;
            }

            return new Dictionary<string, object>
            {
                ["Зона исключения (мм)"] = edgeExclusionMm,
                ["Кристаллы в зоне исключения"] = inExclusionZone,
                ["Кристаллы за пределами пластины"] = outsideWafer,
                ["Корректность"] = outsideWafer == 0 ? "✓ OK" : "✗ Ошибка"
            };
        }

        /// <summary>
        /// Получает границы области с кристаллами
        /// </summary>
        /// <returns>Информация о границах</returns>
        public Dictionary<string, object> GetBounds()
        {
            if (crystals.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["Границы"] = "Нет кристаллов"
                };
            }

            float minX = crystals.Min(c => c.RealX);
            float maxX = crystals.Max(c => c.RealX);
            float minY = crystals.Min(c => c.RealY);
            float maxY = crystals.Max(c => c.RealY);

            float width = maxX - minX;
            float height = maxY - minY;

            return new Dictionary<string, object>
            {
                ["Мин X (мм)"] = minX,
                ["Макс X (мм)"] = maxX,
                ["Мин Y (мм)"] = minY,
                ["Макс Y (мм)"] = maxY,
                ["Ширина области (мм)"] = width,
                ["Высота области (мм)"] = height
            };
        }

        /// <summary>
        /// Генерирует полный отчет статистики
        /// </summary>
        /// <param name="crystalWidth">Ширина кристалла в мм</param>
        /// <param name="crystalHeight">Высота кристалла в мм</param>
        /// <returns>Словарь с полной статистикой</returns>
        public Dictionary<string, object> GenerateFullReport(
            float crystalWidth, float crystalHeight)
        {
            var report = new Dictionary<string, object>
            {
                ["Общее количество кристаллов"] = crystals.Count,
                ["Диаметр пластины (мм)"] = waferDiameter,
                ["Размер кристалла (мм)"] = $"{crystalWidth:F3} x {crystalHeight:F3}",
                ["Процент заполнения"] = $"{CalculateFillPercentage(crystalWidth, crystalHeight):F2}%"
            };

            // Добавляем границы
            var bounds = GetBounds();
            foreach (var kv in bounds)
            {
                report[kv.Key] = kv.Value;
            }

            // Добавляем оценку времени сканирования
            float scanTime = EstimateScanTime(crystalWidth, crystalHeight);
            report["Оценка времени сканирования"] = $"{scanTime:F1} сек ({scanTime / 60:F1} мин)";

            // Распределение по рядам/колонкам
            var rowDist = GetRowDistribution();
            if (rowDist.Count > 0)
            {
                report["Количество рядов"] = rowDist.Count;
                report["Макс. кристаллов в ряду"] = rowDist.Values.Max();
                report["Мин. кристаллов в ряду"] = rowDist.Values.Min();
            }

            var colDist = GetColumnDistribution();
            if (colDist.Count > 0)
            {
                report["Количество колонок"] = colDist.Count;
                report["Макс. кристаллов в колонке"] = colDist.Values.Max();
                report["Мин. кристаллов в колонке"] = colDist.Values.Min();
            }

            return report;
        }

        /// <summary>
        /// Получает список кристаллов в заданной области
        /// </summary>
        /// <param name="centerX">X координата центра области</param>
        /// <param name="centerY">Y координата центра области</param>
        /// <param name="radius">Радиус области</param>
        /// <returns>Список кристаллов в области</returns>
        public List<Crystal> GetCrystalsInArea(float centerX, float centerY, float radius)
        {
            return crystals.Where(crystal =>
            {
                float dx = crystal.RealX - centerX;
                float dy = crystal.RealY - centerY;
                return Math.Sqrt(dx * dx + dy * dy) <= radius;
            }).ToList();
        }

        /// <summary>
        /// Получает статистику по выбранным кристаллам
        /// </summary>
        /// <param name="selectedIndices">Индексы выбранных кристаллов</param>
        /// <returns>Статистика по выбранным кристаллам</returns>
        public Dictionary<string, object> GetSelectionStatistics(HashSet<int> selectedIndices)
        {
            var selectedCrystals = crystals.Where(c => selectedIndices.Contains(c.Index)).ToList();

            if (selectedCrystals.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["Выбрано кристаллов"] = 0
                };
            }

            // Находим границы выделенной области
            float minX = selectedCrystals.Min(c => c.RealX);
            float maxX = selectedCrystals.Max(c => c.RealX);
            float minY = selectedCrystals.Min(c => c.RealY);
            float maxY = selectedCrystals.Max(c => c.RealY);

            // Центр выделения
            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;

            return new Dictionary<string, object>
            {
                ["Выбрано кристаллов"] = selectedCrystals.Count,
                ["Процент от общего"] = $"{(selectedCrystals.Count * 100.0 / crystals.Count):F1}%",
                ["Границы X (мм)"] = $"[{minX:F2}, {maxX:F2}]",
                ["Границы Y (мм)"] = $"[{minY:F2}, {maxY:F2}]",
                ["Центр выделения (мм)"] = $"({centerX:F2}, {centerY:F2})",
                ["Размер области (мм)"] = $"{(maxX - minX):F2} x {(maxY - minY):F2}"
            };
        }
    }
}
