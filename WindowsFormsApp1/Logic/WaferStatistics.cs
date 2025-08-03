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
        /// Получает распределение кристаллов по радиусу (от центра к краю)
        /// </summary>
        /// <param name="bins">Количество концентрических колец для анализа</param>
        /// <returns>Словарь: радиус кольца -> количество кристаллов</returns>
        public Dictionary<float, int> GetRadialDistribution(int bins = 10)
        {
            var distribution = new Dictionary<float, int>();

            if (bins <= 0 || waferDiameter <= 0)
                return distribution;

            float radius = waferDiameter / 2;
            float binSize = radius / bins;

            // Инициализация bins
            for (int i = 0; i < bins; i++)
            {
                distribution[i * binSize] = 0;
            }

            // Подсчет кристаллов в каждом кольце
            foreach (var crystal in crystals)
            {
                // Расстояние от центра
                float distance = (float)Math.Sqrt(
                    crystal.RealX * crystal.RealX +
                    crystal.RealY * crystal.RealY);

                // Определяем в какое кольцо попадает
                int binIndex = (int)(distance / binSize);
                if (binIndex < bins)
                {
                    distribution[binIndex * binSize]++;
                }
            }

            return distribution;
        }

        /// <summary>
        /// Получает статистику по квадрантам пластины
        /// </summary>
        /// <returns>Распределение кристаллов по квадрантам</returns>
        public Dictionary<string, int> GetQuadrantDistribution()
        {
            var quadrants = new Dictionary<string, int>
            {
                ["Q1 (Верх-Право)"] = 0,
                ["Q2 (Верх-Лево)"] = 0,
                ["Q3 (Низ-Лево)"] = 0,
                ["Q4 (Низ-Право)"] = 0
            };

            foreach (var crystal in crystals)
            {
                if (crystal.RealX >= 0 && crystal.RealY >= 0)
                    quadrants["Q1 (Верх-Право)"]++;
                else if (crystal.RealX < 0 && crystal.RealY >= 0)
                    quadrants["Q2 (Верх-Лево)"]++;
                else if (crystal.RealX < 0 && crystal.RealY < 0)
                    quadrants["Q3 (Низ-Лево)"]++;
                else
                    quadrants["Q4 (Низ-Право)"]++;
            }

            return quadrants;
        }

        /// <summary>
        /// Получает распределение кристаллов по краям пластины
        /// </summary>
        /// <param name="edgeThickness">Толщина края в мм</param>
        /// <returns>Количество кристаллов на краю и в центре</returns>
        public Dictionary<string, int> GetEdgeDistribution(float edgeThickness = 5.0f)
        {
            var distribution = new Dictionary<string, int>
            {
                ["Край"] = 0,
                ["Центр"] = 0
            };

            float radius = waferDiameter / 2;
            float innerRadius = radius - edgeThickness;

            foreach (var crystal in crystals)
            {
                float distance = (float)Math.Sqrt(
                    crystal.RealX * crystal.RealX +
                    crystal.RealY * crystal.RealY);

                if (distance > innerRadius)
                    distribution["Край"]++;
                else
                    distribution["Центр"]++;
            }

            return distribution;
        }

        /// <summary>
        /// Получает координаты центра масс всех кристаллов
        /// </summary>
        /// <returns>Координаты центра масс (X, Y)</returns>
        public (float X, float Y) GetCenterOfMass()
        {
            if (crystals.Count == 0)
                return (0, 0);

            float sumX = 0;
            float sumY = 0;

            foreach (var crystal in crystals)
            {
                sumX += crystal.RealX;
                sumY += crystal.RealY;
            }

            return (sumX / crystals.Count, sumY / crystals.Count);
        }

        /// <summary>
        /// Получает плотность кристаллов (количество на единицу площади)
        /// </summary>
        /// <returns>Плотность кристаллов на мм²</returns>
        public float GetCrystalDensity()
        {
            if (waferDiameter <= 0)
                return 0;

            float waferArea = (float)(Math.PI * Math.Pow(waferDiameter / 2, 2));
            return crystals.Count / waferArea;
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
                ["Размер кристалла (мм)"] = $"{crystalWidth} x {crystalHeight}",
                ["Процент заполнения"] = CalculateFillPercentage(crystalWidth, crystalHeight),
                ["Плотность (кристаллов/мм²)"] = GetCrystalDensity(),
                ["Распределение по квадрантам"] = GetQuadrantDistribution(),
                ["Распределение край/центр"] = GetEdgeDistribution(),
                ["Центр масс (X, Y)"] = GetCenterOfMass()
            };

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
                ["Процент от общего"] = (selectedCrystals.Count * 100.0 / crystals.Count),
                ["Границы области (мм)"] = $"X: [{minX:F2}, {maxX:F2}], Y: [{minY:F2}, {maxY:F2}]",
                ["Центр выделения"] = $"({centerX:F2}, {centerY:F2})",
                ["Размер области (мм)"] = $"{(maxX - minX):F2} x {(maxY - minY):F2}"
            };
        }
    }
}