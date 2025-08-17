using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrystalTable
{
    public partial class Form1
    {
        private const byte CMD_LEFT = 1;
        private const byte CMD_RIGHT = 2;
        private const byte CMD_UP = 3;
        private const byte CMD_DOWN = 4;

        private PointF pointerMm = new PointF(0, 0);

        // Доступ для UI/статуса
        public PointF GetPointerMm() => pointerMm;

        // Сброс указателя в центр (0;0)
        public void CenterPointer()
        {
            pointerMm = new PointF(0f, 0f);
            pictureBox1?.Invalidate();
        }

        // ===== Обработчики кнопок движения =====
        private async void buttonMoveLeft_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.X, negative: true);
        private async void buttonMoveRight_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.X, negative: false);
        private async void buttonMoveUp_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.Y, negative: true);  // ВВЕРХ = минус Y
        private async void buttonMoveDown_Click(object sender, EventArgs e) => await MoveAxisAsync(Axis.Y, negative: false); // ВНИЗ  = плюс  Y

        // Диагональный шаг X+Y
        private async void scan_Click(object sender, EventArgs e)
        {
            if (!TryGetMoveSteps(out uint stepXum, out uint stepYum))
            {
                MessageBox.Show("Введите корректные шаги.", "Шаг", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            float dxMm = stepXum / 1000f;
            float dyMm = stepYum / 1000f;

            // Вверх = минус Y
            float newX = pointerMm.X + dxMm;
            float newY = pointerMm.Y - dyMm;

            if (!CanMoveTo(newX, newY))
            {
                MessageBox.Show("Выход за пределы пластины.");
                return;
            }

            if (!await TrySendAsync(CMD_RIGHT, stepXum)) return;
            if (!await TrySendAsync(CMD_UP, stepYum)) return;

            pointerMm = new PointF(newX, newY);
            pictureBox1.Invalidate();
            UpdateUI();
        }

        private enum Axis { X, Y }

        private async Task MoveAxisAsync(Axis axis, bool negative)
        {
            if (!TryGetMoveSteps(out uint stepXum, out uint stepYum))
            {
                MessageBox.Show("Введите корректные шаги.", "Шаг", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte cmd;
            uint stepUm;
            float dMm;

            if (axis == Axis.X)
            {
                cmd = negative ? CMD_LEFT : CMD_RIGHT;
                stepUm = stepXum;
                dMm = (negative ? -1f : 1f) * (stepXum / 1000f);

                float newX = pointerMm.X + dMm;
                if (!CanMoveTo(newX, pointerMm.Y)) { MessageBox.Show("Выход за пределы пластины."); return; }
                if (!await TrySendAsync(cmd, stepUm)) return;
                pointerMm = new PointF(newX, pointerMm.Y);
            }
            else
            {
                // ВВЕРХ = минус Y
                cmd = negative ? CMD_UP : CMD_DOWN;
                stepUm = stepYum;
                dMm = (negative ? -1f : 1f) * (stepYum / 1000f);

                float newY = pointerMm.Y + (negative ? -Math.Abs(dMm) : Math.Abs(dMm));
                if (!CanMoveTo(pointerMm.X, newY)) { MessageBox.Show("Выход за пределы пластины."); return; }
                if (!await TrySendAsync(cmd, stepUm)) return;
                pointerMm = new PointF(pointerMm.X, newY);
            }

            pictureBox1.Invalidate();
            UpdateUI();
        }

        // ===== Режим шага: дискретный (Pitch) / произвольный (фиксированные) =====
        // Если на форме есть CheckBox "checkBoxDiscreteStep":
        // ON  — дискретный шаг по Pitch (SizeX/SizeY)
        // OFF — фиксированный шаг (по умолчанию 100 µm)
        private bool UseDiscreteStep()
        {
            var ctrl = this.Controls.Find("checkBoxDiscreteStep", true).FirstOrDefault() as CheckBox;
            return ctrl?.Checked ?? true; // по умолчанию — дискретный
        }

        // Текущие шаги перемещения в µm
        private bool TryGetMoveSteps(out uint pitchXum, out uint pitchYum)
        {
            pitchXum = pitchYum = 0;

            if (UseDiscreteStep())
                return TryGetPitchUm(out pitchXum, out pitchYum);

            const uint defaultStepUm = 100;
            pitchXum = defaultStepUm;
            pitchYum = defaultStepUm;
            return true;
        }

        // Pitch из полей ввода (µm)
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
                    MessageBox.Show("COM-порт не подключён.");
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
                MessageBox.Show("Ошибка отправки. Проверьте COM-порт.");
                return false;
            }
        }
    }
}
