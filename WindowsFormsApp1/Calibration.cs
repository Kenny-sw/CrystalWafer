using System;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        // === Заглушки координат стола. COM не трогаем. ===
        private float CurrentStageX => GetCurrentStageX();
        private float CurrentStageY => GetCurrentStageY();
        private float GetCurrentStageX() => 0f;
        private float GetCurrentStageY() => 0f;

        /// <summary>
        /// Инициализация UI-состояния секции калибровки.
        /// Вызови после CreateNewWafer() или при загрузке формы.
        /// </summary>
        private void InitializeCalibrationUiState()
        {
            if (btnBuildMap != null)
                btnBuildMap.Enabled = waferController.IsCalibrationReady() || waferController.IsPresetReady();

            if (lblFirstRef != null) lblFirstRef.Text = "Первый: —";
            if (lblLastRef != null) lblLastRef.Text = "Последний: —";
            if (lblPitchX != null) lblPitchX.Text = "PitchX: —";
            if (lblPitchY != null) lblPitchY.Text = "PitchY: —";
            if (lblCols != null) lblCols.Text = "Cols (Nx): —";
            if (lblRows != null) lblRows.Text = "Rows (Ny): —";
        }

        /// <summary>
        /// Кнопка «Выбрать первый» (левый-верхний кристалл первого ряда).
        /// </summary>
        private void btnSelectFirst_Click(object sender, EventArgs e)
        {
            float x = CurrentStageX;
            float y = CurrentStageY;

            waferController.SetFirstReference(x, y);

            if (lblFirstRef != null)
                lblFirstRef.Text = $"Первый: X={x:F2}, Y={y:F2} мм";

            if (btnBuildMap != null)
                btnBuildMap.Enabled = waferController.IsCalibrationReady() || waferController.IsPresetReady();
        }

        /// <summary>
        /// Кнопка «Выбрать последний» (правый-нижний кристалл последнего ряда).
        /// </summary>
        private void btnSelectLast_Click(object sender, EventArgs e)
        {
            float x = CurrentStageX;
            float y = CurrentStageY;

            waferController.SetLastReference(x, y);

            if (lblLastRef != null)
                lblLastRef.Text = $"Последний: X={x:F2}, Y={y:F2} мм";

            if (btnBuildMap != null)
                btnBuildMap.Enabled = waferController.IsCalibrationReady() || waferController.IsPresetReady();
        }

        /// <summary>
        /// Кнопка «Построить карту».
        /// Строим по двум якорям, иначе — из текущих настроек (пресет). Угол игнорируем.
        /// </summary>
        private void btnBuildMap_Click(object sender, EventArgs e)
        {
            try
            {
                if (waferController.IsCalibrationReady())
                {
                    waferController.BuildMapFromReferences();
                }
                else if (waferController.IsPresetReady())
                {
                    waferController.BuildMapFromCurrentSettings();
                }
                else
                {
                    MessageBox.Show("Задайте шаги и диаметр или выберите оба якоря.",
                        "Калибровка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (lblPitchX != null) lblPitchX.Text = $"PitchX: {waferController.StepXmm * 1000:F0} µm";
                if (lblPitchY != null) lblPitchY.Text = $"PitchY: {waferController.StepYmm * 1000:F0} µm";
                if (lblCols != null) lblCols.Text = $"Cols (Nx): {waferController.CrystalsPerRow}";
                if (lblRows != null) lblRows.Text = $"Rows (Ny): {waferController.RowsTotal}";

                pictureBox1?.Invalidate();
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения карты: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ВАЖНО:
        // - Здесь НЕТ Create_Click. Он должен остаться единственный в Form1.cs.
        // - Не используем RotationAngleDeg и другие поля угла — угол по ТЗ пока игнорируем.
        // - Метки: используем lblFirstRef/lblLastRef/lblPitchX/lblPitchY/lblCols/lblRows (а не labelStepX/...).
    }
}
