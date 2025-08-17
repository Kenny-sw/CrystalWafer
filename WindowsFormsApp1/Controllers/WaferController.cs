using System;
using System.Collections.Generic;
using System.Drawing;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Логика ваферы: параметры, калибровка, построение сетки кристаллов и маршрут.
    /// Отрисовки здесь нет — она в Form1.Drawing.cs.
    /// </summary>
    public class WaferController
    {
        private readonly Form1 form;

        // Диапазоны
        public const float MinWaferDiameter = 50f;   // мм
        public const float MaxWaferDiameter = 450f;  // мм

        // Ввод
        public uint CrystalWidthRaw { get; set; }   // µm
        public uint CrystalHeightRaw { get; set; }   // µm
        public float WaferDiameter { get; set; }   // mm

        // Буферы для UI-вакуума
        public uint SizeXtemp, SizeYtemp;
        public float WaferDiameterTemp;

        // Производные
        public float StepXmm { get; private set; }   // мм (если 0 — берём из размеров кристалла)
        public float StepYmm { get; private set; }   // мм
        public int CrystalsPerRow { get; private set; }
        public int RowsTotal { get; private set; }
        public float ScaleFactor { get; private set; } = 1f;
        public bool WaferDisplayMode { get; set; } = false;

        // Опорные точки (для подсветки до построения)
        private bool firstRefSet, lastRefSet;
        private PointF firstRefMm, lastRefMm;

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

        // Масштаб под размер viewport’а
        public void AutoSetScaleFactor(int viewportWidth, int viewportHeight)
        {
            float d = WaferDiameter > 0 ? WaferDiameter : 200f;
            if (d <= 0) { ScaleFactor = 1f; return; }

            float pad = 20f;
            float usableW = Math.Max(1, viewportWidth - pad * 2);
            float usableH = Math.Max(1, viewportHeight - pad * 2);

            float sx = usableW / d;
            float sy = usableH / d;
            ScaleFactor = Math.Max(1f, Math.Min(sx, sy));
        }

        // Создать/очистить вафер
        public void CreateNewWafer()
        {
            firstRefSet = lastRefSet = false;
            StepXmm = StepYmm = 0f;
            BuildCrystalsCached();
        }

        /// <summary>
        /// Построить «ковёр» кристаллов по кругу ваферы. Ничего не рисует.
        /// </summary>
        public void BuildCrystalsCached()
        {
            var cm = CrystalManager.Instance;
            cm.Crystals.Clear();

            float stepX = StepXmm > 0f ? StepXmm : (CrystalWidthRaw / 1000f);
            float stepY = StepYmm > 0f ? StepYmm : (CrystalHeightRaw / 1000f);

            if (stepX <= 0f || stepY <= 0f || WaferDiameter <= 0f)
            {
                CrystalsPerRow = 0; RowsTotal = 0;
                return;
            }

            float r = WaferDiameter / 2f;
            int index = 1, rows = 0, maxPerRow = 0;

            for (float y = -r; y <= r + 1e-6f; y += stepY)
            {
                var xs = new List<float>();
                for (float x = -r; x <= r + 1e-6f; x += stepX)
                    if (x * x + y * y <= r * r + 1e-6f)
                        xs.Add(x);

                if (xs.Count == 0) continue;

                if ((rows & 1) == 1) xs.Reverse(); // змейка

                foreach (var x in xs)
                {
                    cm.Crystals.Add(new Crystal
                    {
                        Index = index++,
                        RealX = x,
                        RealY = y
                    });
                }

                rows++;
                if (xs.Count > maxPerRow) maxPerRow = xs.Count;
            }

            CrystalsPerRow = maxPerRow;
            RowsTotal = rows;
        }

        // Калибровка
        public void SetFirstReference(float xMm, float yMm) { firstRefMm = new PointF(xMm, yMm); firstRefSet = true; }
        public void SetLastReference(float xMm, float yMm) { lastRefMm = new PointF(xMm, yMm); lastRefSet = true; }

        public bool IsCalibrationReady() => firstRefSet && lastRefSet;
        public bool IsPresetReady()
        {
            if (StepXmm > 0f && StepYmm > 0f) return true;
            return (CrystalWidthRaw > 0 && CrystalHeightRaw > 0);
        }

        public void BuildMapFromReferences()
        {
            if (StepXmm <= 0f) StepXmm = CrystalWidthRaw / 1000f;
            if (StepYmm <= 0f) StepYmm = CrystalHeightRaw / 1000f;
            BuildCrystalsCached();
        }

        public void BuildMapFromPreset()
        {
            if (StepXmm <= 0f) StepXmm = CrystalWidthRaw / 1000f;
            if (StepYmm <= 0f) StepYmm = CrystalHeightRaw / 1000f;
            BuildCrystalsCached();
        }

        // Маршрут
        public void GenerateRoute(RoutePreview preview, HashSet<int> selectedCrystals)
        {
            if (preview == null) return;

            var all = CrystalManager.Instance.Crystals;
            List<Crystal> list;

            if (selectedCrystals != null && selectedCrystals.Count > 0)
            {
                list = new List<Crystal>(selectedCrystals.Count);
                foreach (var c in all) if (selectedCrystals.Contains(c.Index)) list.Add(c);
            }
            else
            {
                list = new List<Crystal>(all);
            }

            // сортируем змейкой по рядам (Y), внутри — по X
            list.Sort((a, b) =>
            {
                int yCmp = a.RealY.CompareTo(b.RealY);
                if (yCmp != 0) return yCmp;

                float stepY = StepYmm > 0 ? StepYmm : Math.Max(0.0001f, CrystalHeightRaw / 1000f);
                int rowIdxA = (int)Math.Round((a.RealY - (-WaferDiameter / 2f)) / stepY);
                bool reverse = (rowIdxA & 1) == 1;

                return reverse ? b.RealX.CompareTo(a.RealX) : a.RealX.CompareTo(b.RealX);
            });

            preview.SetRoute(list);
        }

        public WaferStatistics GetStatistics() => new WaferStatistics(CrystalManager.Instance.Crystals, WaferDiameter);

        // Удобные свойства для UI
        public float StepXmmOrDefault => StepXmm > 0f ? StepXmm : (CrystalWidthRaw / 1000f);
        public float StepYmmOrDefault => StepYmm > 0f ? StepYmm : (CrystalHeightRaw / 1000f);

        public void SetSteps(float stepXmm, float stepYmm)
        {
            StepXmm = Math.Max(0f, stepXmm);
            StepYmm = Math.Max(0f, stepYmm);
        }
    }
}
