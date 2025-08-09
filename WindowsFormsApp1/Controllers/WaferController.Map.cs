using CrystalTable.Data;
using CrystalTable.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CrystalTable.Controllers
{
    public partial class WaferController
    {
        /* ---------- параметры построенной карты ---------- */
        public bool MapLocked { get; private set; } = false;
        public float RotationAngleDeg { get; private set; }
        public float StepXmm { get; private set; }
        public float StepYmm { get; private set; }
        public int CrystalsPerRow { get; private set; }
        public int RowsTotal { get; private set; }

        /// <summary>Построить карту по двум опорным кристаллам</summary>
        public void BuildMapFromReferences()
        {
            if (!IsCalibrationReady())
                throw new InvalidOperationException("Reference points are not set");

            // 1. вектор между опорами
            var dx = LastRefX!.Value - FirstRefX!.Value;
            var dy = LastRefY!.Value - FirstRefY!.Value;

            // 2. угол ряда
            RotationAngleDeg = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);

            // 3. шаги одного кристалла (мм)
            StepXmm = CrystalWidthRaw / 1000f;
            StepYmm = CrystalHeightRaw / 1000f;

            // 4. к-во кристаллов в строке
            var length = Math.Sqrt(dx * dx + dy * dy);
            CrystalsPerRow = Math.Max(2, (int)Math.Round(length / StepXmm) + 1);

            // 5. векторы вдоль строки и перпендикуляр вниз
            var stepVec = new PointF((float)(dx / (CrystalsPerRow - 1)),
                                     (float)(dy / (CrystalsPerRow - 1)));

            var perp = new PointF(-stepVec.Y, stepVec.X);
            var norm = (float)Math.Sqrt(perp.X * perp.X + perp.Y * perp.Y);
            perp.X *= StepYmm / norm;
            perp.Y *= StepYmm / norm;

            // 6. генерация «змейкой»
            CrystalManager.Instance.Crystals.Clear();
            nextCrystalIndex = 1;

            float radius = WaferDiameter / 2;
            var rowStart = new PointF(FirstRefX!.Value, FirstRefY!.Value);
            RowsTotal = 0;
            bool snake = false;

            while (true)
            {
                var row = new List<Crystal>();
                for (int i = 0; i < CrystalsPerRow; i++)
                {
                    var cx = rowStart.X + i * stepVec.X;
                    var cy = rowStart.Y + i * stepVec.Y;

                    if (cx * cx + cy * cy > radius * radius) continue; // за пределом пластины

                    row.Add(new Crystal
                    {
                        Index = nextCrystalIndex++,
                        RealX = cx,
                        RealY = cy,
                        Color = Color.Blue
                    });
                }

                if (row.Count == 0) break;     // дошли до края

                if (snake) row.Reverse();
                CrystalManager.Instance.Crystals.AddRange(row);

                snake = !snake;
                RowsTotal++;

                rowStart.X += perp.X;
                rowStart.Y += perp.Y;
            }

            MapLocked = true;                 // блокируем ручное редактирование
        }
    }
}
