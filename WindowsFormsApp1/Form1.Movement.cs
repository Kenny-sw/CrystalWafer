using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable
{
    public partial class Form1
    {
        private const byte CMD_LEFT = 1;
        private const byte CMD_RIGHT = 2;
        private const byte CMD_UP = 3;
        private const byte CMD_DOWN = 4;

        private PointF pointerMm = new PointF(0, 0);
        private bool isLoadingInProgress;

        public PointF GetPointerMm() => pointerMm;

        public void SetPointerMm(float xMm, float yMm)
        {
            pointerMm = new PointF(xMm, yMm);
            pictureBox1?.Invalidate();
            UpdateUI();
        }

        public void CenterPointer()
        {
            pointerMm = new PointF(0f, 0f);
            pictureBox1?.Invalidate();
        }

        private async void buttonMoveLeft_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.X, negative: true);
        private async void buttonMoveRight_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.X, negative: false);
        private async void buttonMoveUp_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.Y, negative: true);
        private async void buttonMoveDown_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.Y, negative: false);

        private async void scan_Click(object sender, EventArgs e)
        {
            if (!TryGetMoveSteps(out uint stepXum, out uint stepYum))
            {
                MessageBox.Show("Ошибка вычисления шага.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            float dxMm = stepXum / 1000f;
            float dyMm = stepYum / 1000f;

            float newX = pointerMm.X + dxMm;
            float newY = pointerMm.Y - dyMm;

            if (!CanMoveTo(newX, newY))
            {
                MessageBox.Show("Нельзя выйти за пределы пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!await TrySendAsync(CMD_RIGHT, stepXum)) return;
            if (!await TrySendAsync(CMD_UP, stepYum)) return;

            pointerMm = new PointF(newX, newY);
            pictureBox1.Invalidate();
            UpdateUI();
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            await ExecuteLoadingSequenceAsync();
        }

        private async Task ExecuteLoadingSequenceAsync()
        {
            if (isLoadingInProgress)
            {
                return;
            }

            if (!IsInputValid())
            {
                MessageBox.Show("Проверьте параметры пластины перед запуском.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            waferController.BuildCrystalsCached();
            UpdateUI();

            if (CrystalManager.Instance.Crystals.Count == 0)
            {
                MessageBox.Show("Карта кристаллов не сформирована.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isLoadingInProgress = true;
            buttonStart.Enabled = false;

            try
            {
                if (!await MoveToCenterAsync())
                {
                    return;
                }

                await MovePointerToSelectedCrystalAsync();
            }
            finally
            {
                buttonStart.Enabled = true;
                isLoadingInProgress = false;
            }
        }

        private enum Axis { X, Y }

        private async Task MoveAxisAsync(Axis axis, bool negative)
        {
            if (!TryGetMoveSteps(out uint stepXum, out uint stepYum))
            {
                MessageBox.Show("Ошибка вычисления шага.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte cmd;
            uint stepUm;
            float deltaMm;

            if (axis == Axis.X)
            {
                cmd = negative ? CMD_LEFT : CMD_RIGHT;
                stepUm = stepXum;
                deltaMm = (negative ? -1f : 1f) * (stepXum / 1000f);

                float newX = pointerMm.X + deltaMm;
                if (!CanMoveTo(newX, pointerMm.Y))
                {
                    MessageBox.Show("Нельзя выйти за пределы пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!await TrySendAsync(cmd, stepUm)) return;
                pointerMm = new PointF(newX, pointerMm.Y);
            }
            else
            {
                cmd = negative ? CMD_UP : CMD_DOWN;
                stepUm = stepYum;
                deltaMm = (negative ? -1f : 1f) * (stepYum / 1000f);

                float newY = pointerMm.Y + deltaMm;
                if (!CanMoveTo(pointerMm.X, newY))
                {
                    MessageBox.Show("Нельзя выйти за пределы пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!await TrySendAsync(cmd, stepUm)) return;
                pointerMm = new PointF(pointerMm.X, newY);
            }

            pictureBox1.Invalidate();
            UpdateUI();
        }

        private async Task<bool> MoveToCenterAsync()
        {
            return await MovePointerToAsync(0f, 0f);
        }

        private async Task MovePointerToSelectedCrystalAsync()
        {
            var crystals = CrystalManager.Instance.Crystals;
            if (crystals.Count == 0)
            {
                return;
            }

            Crystal target = null;

            if (mouseController.SelectedCrystals.Count > 0)
            {
                foreach (int index in mouseController.SelectedCrystals)
                {
                    target = crystals.FirstOrDefault(c => c.Index == index);
                    if (target != null)
                    {
                        break;
                    }
                }
            }

            target ??= crystals.FirstOrDefault();
            if (target == null)
            {
                return;
            }

            await MovePointerToAsync(target.RealX, target.RealY);
        }

        private async Task<bool> MovePointerToAsync(float targetXmm, float targetYmm)
        {
            if (!CanMoveTo(targetXmm, targetYmm))
            {
                MessageBox.Show("Цель вне пределов пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            float dx = targetXmm - pointerMm.X;
            float dy = targetYmm - pointerMm.Y;

            uint moveXum = (uint)Math.Round(Math.Abs(dx) * 1000f);
            uint moveYum = (uint)Math.Round(Math.Abs(dy) * 1000f);

            if (moveXum > 0)
            {
                byte command = dx > 0 ? CMD_RIGHT : CMD_LEFT;
                if (!await TrySendAsync(command, moveXum))
                {
                    return false;
                }

                float movedMm = moveXum / 1000f;
                pointerMm = new PointF(pointerMm.X + (dx > 0 ? movedMm : -movedMm), pointerMm.Y);
            }

            if (moveYum > 0)
            {
                byte command = dy > 0 ? CMD_DOWN : CMD_UP;
                if (!await TrySendAsync(command, moveYum))
                {
                    return false;
                }

                float movedMm = moveYum / 1000f;
                pointerMm = new PointF(pointerMm.X, pointerMm.Y + (dy > 0 ? movedMm : -movedMm));
            }

            pointerMm = new PointF(targetXmm, targetYmm);
            UpdateUI();
            return true;
        }

        private bool UseDiscreteStep()
        {
            var ctrl = this.Controls.Find("checkBoxDiscreteStep", true).FirstOrDefault() as CheckBox;
            return ctrl?.Checked ?? true;
        }

        private bool TryGetMoveSteps(out uint pitchXum, out uint pitchYum)
        {
            pitchXum = pitchYum = 0;

            if (UseDiscreteStep())
            {
                return TryGetPitchUm(out pitchXum, out pitchYum);
            }

            const uint defaultStepUm = 100;
            pitchXum = defaultStepUm;
            pitchYum = defaultStepUm;
            return true;
        }

        private bool TryGetPitchUm(out uint pitchXum, out uint pitchYum)
        {
            pitchXum = pitchYum = 0;
            if (!uint.TryParse(SizeX.Text.Trim(), out pitchXum)) return false;
            if (!uint.TryParse(SizeY.Text.Trim(), out pitchYum)) return false;
            if (pitchXum == 0 || pitchYum == 0) return false;
            return true;
        }

        private bool CanMoveTo(float xMm, float yMm)
        {
            float r = waferController?.WaferDiameter > 0 ? waferController.WaferDiameter / 2f : 100f;
            return (xMm * xMm + yMm * yMm) <= (r * r) + 1e-6f;
        }

        private async Task<bool> TrySendAsync(byte commandByte, uint stepUm)
        {
            try
            {
                if (MyserialPort == null || !MyserialPort.IsOpen)
                {
                    MessageBox.Show("COM-порт не открыт.");
                    return false;
                }

                byte[] dataToSend = new byte[]
                {
                    commandByte,
                    (byte)(stepUm & 0xFF),
                    (byte)((stepUm >> 8) & 0xFF),
                    (byte)((stepUm >> 16) & 0xFF),
                    (byte)((stepUm >> 24) & 0xFF)
                };

                await MyserialPort.BaseStream.WriteAsync(dataToSend, 0, dataToSend.Length);
                await MyserialPort.BaseStream.FlushAsync();
                await Task.Delay(50);
                return true;
            }
            catch
            {
                MessageBox.Show("Ошибка обмена. Проверьте COM-порт.");
                return false;
            }
        }
    }
}
