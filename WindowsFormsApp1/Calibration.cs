using System;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        private void InitializeCalibrationUiState()
        {
            if (lblFirstRef != null) lblFirstRef.Text = "Первый: —";
            if (lblLastRef != null) lblLastRef.Text = "Последний: —";
            if (lblPitchX != null) lblPitchX.Text = "PitchX: —";
            if (lblPitchY != null) lblPitchY.Text = "PitchY: —";
            if (lblCols != null) lblCols.Text = "Cols (Nx): —";
            if (lblRows != null) lblRows.Text = "Rows (Ny): —";
            UpdateBuildMapEnabled();
        }

        private void UpdateBuildMapEnabled()
        {
            if (btnBuildMap == null || waferController == null) return;
            btnBuildMap.Enabled = waferController.IsCalibrationReady() || waferController.IsPresetReady();
        }

        private void UpdateCalibrationLabelsAfterBuild()
        {
            if (waferController == null) return;

            if (lblPitchX != null) lblPitchX.Text = $"PitchX: {waferController.StepXmmOrDefault * 1000:F0} µm";
            if (lblPitchY != null) lblPitchY.Text = $"PitchY: {waferController.StepYmmOrDefault * 1000:F0} µm";
            if (lblCols != null) lblCols.Text = $"Cols (Nx): {waferController.CrystalsPerRow}";
            if (lblRows != null) lblRows.Text = $"Rows (Ny): {waferController.RowsTotal}";

            pictureBox1?.Invalidate();
            UpdateUI();
        }

        private void UpdateCalibrationLabelsAfterSelection() => UpdateCalibrationLabelsAfterBuild();
    }
}
