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
        // ✅ ВНУТРЕННЕЕ ХРАНЕНИЕ: ФИЗИЧЕСКИЕ координаты (машинная система)
        // pointerMm хранит реальное положение ЛШД в системе координат машины
        // БЕЗ учёта калибровки виртуальной карты
        private PointF pointerMm = new PointF(0, 0);
        private bool isLoadingInProgress;

        // ✅ ПУБЛИЧНЫЙ API: возвращает ВИРТУАЛЬНЫЕ координаты (система карты)
        // Преобразует физическую позицию машины -> виртуальную позицию на карте
        public PointF GetPointerMm() => ToVirtual(pointerMm);

        // ✅ СЛУЖЕБНЫЙ: возвращает ФИЗИЧЕСКИЕ координаты (машинная система)
        // Прямой доступ к реальной позиции ЛШД без преобразования
        public PointF GetPointerMachineMm() => pointerMm;

        // ✅ ПУБЛИЧНЫЙ API: принимает ВИРТУАЛЬНЫЕ координаты карты
        // Преобразует виртуальную позицию на карте -> физическую позицию машины
        // Использование: SetPointerMm(crystalVirtualX, crystalVirtualY)
        public void SetPointerMm(float xMm, float yMm)
        {
            pointerMm = ToPhysical(new PointF(xMm, yMm));
            pictureBox1?.Invalidate();
            UpdateUI();
        }

        // ✅ СЛУЖЕБНЫЙ: устанавливает ФИЗИЧЕСКИЕ координаты напрямую
        // БЕЗ преобразования - для прямого управления позицией машины
        // Использование: SetPointerMachineMm(physicalX, physicalY)
        public void SetPointerMachineMm(float xMm, float yMm)
        {
            pointerMm = new PointF(xMm, yMm);
            pictureBox1?.Invalidate();
            UpdateUI();
        }

        public void CenterPointer()
        {
            // ✅ (0, 0) в ВИРТУАЛЬНЫХ координатах = центр пластины на карте
            // Преобразуется в физические координаты машины с учётом калибровки
            pointerMm = ToPhysical(new PointF(0f, 0f));
            pictureBox1?.Invalidate();
            UpdateUI();
        }

        // ✅ Преобразование координат через WaferController (учитывает калибровку)
        private PointF ToVirtual(PointF physicalPoint) =>
            waferController?.ToVirtualCoordinates(physicalPoint) ?? physicalPoint;

        private PointF ToPhysical(PointF virtualPoint) =>
            waferController?.ToPhysicalCoordinates(virtualPoint) ?? virtualPoint;

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

            // ✅ Проверка в ВИРТУАЛЬНЫХ координатах
            var candidatePhysical = new PointF(pointerMm.X + dxMm, pointerMm.Y - dyMm);
            var candidateVirtual = ToVirtual(candidatePhysical);

            if (!CanMoveTo(candidateVirtual.X, candidateVirtual.Y))
            {
                MessageBox.Show("Нельзя выйти за пределы пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!await TrySendAsync(Protocol.Commands.MoveRight, stepXum)) return;
            if (!await TrySendAsync(Protocol.Commands.MoveUp, stepYum)) return;

            pointerMm = candidatePhysical;
            pictureBox1.Invalidate();
            UpdateUI();
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            // Кнопка "Загрузка" - только построение карты кристаллов без перемещения
            await BuildCrystalMapAsync();
        }

        /// <summary>
        /// Построение карты кристаллов без автоматического перемещения
        /// </summary>
        private async System.Threading.Tasks.Task BuildCrystalMapAsync()
        {
            if (!IsInputValid())
            {
                MessageBox.Show("Проверьте параметры пластины перед загрузкой.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonStart.Enabled = false;
            
            try
            {
                waferController.BuildCrystalsCached();
                UpdateUI();
                
                var count = CrystalManager.Instance.Crystals.Count;
                if (count > 0)
                {
                    AppLogger.Info($"Карта кристаллов загружена: {count} кристаллов");
                    statusLabel.Text = $"Загружено {count} кристаллов";
                }
                else
                {
                    MessageBox.Show("Карта кристаллов пустая. Проверьте параметры.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                buttonStart.Enabled = true;
            }
            
            await System.Threading.Tasks.Task.CompletedTask; // async совместимость
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
                cmd = negative ? Protocol.Commands.MoveLeft : Protocol.Commands.MoveRight;
                stepUm = stepXum;
                deltaMm = (negative ? -1f : 1f) * (stepXum / 1000f);

                // ✅ Проверка в ВИРТУАЛЬНЫХ координатах
                var candidatePhysical = new PointF(pointerMm.X + deltaMm, pointerMm.Y);
                var candidateVirtual = ToVirtual(candidatePhysical);
                if (!CanMoveTo(candidateVirtual.X, candidateVirtual.Y))
                {
                    MessageBox.Show("Нельзя выйти за пределы пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!await TrySendAsync(cmd, stepUm)) return;
                pointerMm = candidatePhysical;
            }
            else
            {
                cmd = negative ? Protocol.Commands.MoveUp : Protocol.Commands.MoveDown;
                stepUm = stepYum;
                deltaMm = (negative ? -1f : 1f) * (stepYum / 1000f);

                // ✅ Проверка в ВИРТУАЛЬНЫХ координатах
                var candidatePhysical = new PointF(pointerMm.X, pointerMm.Y + deltaMm);
                var candidateVirtual = ToVirtual(candidatePhysical);
                if (!CanMoveTo(candidateVirtual.X, candidateVirtual.Y))
                {
                    MessageBox.Show("Нельзя выйти за пределы пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!await TrySendAsync(cmd, stepUm)) return;
                pointerMm = candidatePhysical;
            }

            pictureBox1.Invalidate();
            UpdateUI();
        }

        private async Task<bool> MoveToCenterAsync()
        {
            // ✅ (0, 0) - это виртуальный центр
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

            // ✅ Crystal.RealX/Y - это ВИРТУАЛЬНЫЕ координаты на карте
            await MovePointerToAsync(target.RealX, target.RealY);
        }

        /// <summary>
        /// ✅ Переместить указатель в заданные ВИРТУАЛЬНЫЕ координаты
        /// </summary>
        private async Task<bool> MovePointerToAsync(float targetXmm, float targetYmm)
        {
            // ✅ Проверка в виртуальных координатах
            if (!CanMoveTo(targetXmm, targetYmm))
            {
                MessageBox.Show("Цель вне пределов пластины.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // ✅ Преобразуем виртуальную цель в физические координаты
            var physicalTarget = ToPhysical(new PointF(targetXmm, targetYmm));
            
            // ✅ Вычисляем дельту в физической системе
            float dx = physicalTarget.X - pointerMm.X;
            float dy = physicalTarget.Y - pointerMm.Y;

            uint moveXum = (uint)Math.Round(Math.Abs(dx) * 1000f);
            uint moveYum = (uint)Math.Round(Math.Abs(dy) * 1000f);

            if (moveXum > 0)
            {
                byte command = dx > 0 ? Protocol.Commands.MoveRight : Protocol.Commands.MoveLeft;
                if (!await TrySendAsync(command, moveXum))
                {
                    return false;
                }

                float movedMm = moveXum / 1000f;
                pointerMm = new PointF(pointerMm.X + (dx > 0 ? movedMm : -movedMm), pointerMm.Y);
            }

            if (moveYum > 0)
            {
                byte command = dy > 0 ? Protocol.Commands.MoveDown : Protocol.Commands.MoveUp;
                if (!await TrySendAsync(command, moveYum))
                {
                    return false;
                }

                float movedMm = moveYum / 1000f;
                pointerMm = new PointF(pointerMm.X, pointerMm.Y + (dy > 0 ? movedMm : -movedMm));
            }

            pointerMm = physicalTarget;
            pictureBox1?.Invalidate();
            UpdateUI();
            return true;
        }

        /// <summary>
        /// Переместить ЛШД на относительное смещение (для калибровки камеры)
        /// </summary>
        /// <param name="deltaXmm">Смещение по X в мм (положительно вправо)</param>
        /// <param name="deltaYmm">Смещение по Y в мм (положительно вниз)</param>
        /// <returns>True если успешно</returns>
        public async Task<bool> MoveRelativeAsync(float deltaXmm, float deltaYmm)
        {
            if (debugModeWithoutComPort)
            {
                AppLogger.Debug($"[DEBUG MODE] Относительное движение: ({deltaXmm:F3}, {deltaYmm:F3}) мм - пропущено");
                // В debug режиме просто обновляем координаты
                pointerMm = new PointF(pointerMm.X + deltaXmm, pointerMm.Y + deltaYmm);
                pictureBox1?.Invalidate();
                UpdateUI();
                return true;
            }

            uint moveXum = (uint)Math.Round(Math.Abs(deltaXmm) * 1000f);
            uint moveYum = (uint)Math.Round(Math.Abs(deltaYmm) * 1000f);

            // Движение по X
            if (moveXum > 0)
            {
                byte command = deltaXmm > 0 ? Protocol.Commands.MoveRight : Protocol.Commands.MoveLeft;
                if (!await TrySendAsync(command, moveXum))
                {
                    return false;
                }

                float movedMm = moveXum / 1000f;
                pointerMm = new PointF(pointerMm.X + (deltaXmm > 0 ? movedMm : -movedMm), pointerMm.Y);
            }

            // Движение по Y
            if (moveYum > 0)
            {
                byte command = deltaYmm > 0 ? Protocol.Commands.MoveDown : Protocol.Commands.MoveUp;
                if (!await TrySendAsync(command, moveYum))
                {
                    return false;
                }

                float movedMm = moveYum / 1000f;
                pointerMm = new PointF(pointerMm.X, pointerMm.Y + (deltaYmm > 0 ? movedMm : -movedMm));
            }

            pictureBox1?.Invalidate();
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
            // ✅ Исправлено: берем из WaferController
            pitchXum = waferController.CrystalWidthRaw;
            pitchYum = waferController.CrystalHeightRaw;
            
            if (pitchXum == 0 || pitchYum == 0) return false;
            return true;
        }

        private async Task<bool> TrySendAsync(byte commandByte, uint stepUm)
        {
            if (debugModeWithoutComPort)
            {
                AppLogger.Debug($"[DEBUG MODE] Команда 0x{commandByte:X2}, шаг={stepUm} um – отправка пропущена.");
                return true;
            }

            // ✅ ДОБАВЛЕНО: Поддержка упрощенного протокола
            if (useSimpleProtocol && simpleSerialController != null)
            {
                AppLogger.Debug($"[SIMPLE MODE] Отправка команды 0x{commandByte:X2}, шаг={stepUm} um");
                
                try
                {
                    bool success = simpleSerialController.SendCommand(commandByte, stepUm);
                    
                    if (!success)
                    {
                        AppLogger.Warning($"[SIMPLE MODE] Команда 0x{commandByte:X2} не выполнена.");
                        MessageBox.Show(
                            $"Упрощенный протокол: Команда 0x{commandByte:X2} не выполнена.\n\n" +
                            "Проверьте логи для деталей.",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    else
                    {
                        AppLogger.Debug($"[SIMPLE MODE] Команда 0x{commandByte:X2} успешно выполнена.");
                    }
                    
                    return success;
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"[SIMPLE MODE] Исключение при отправке команды 0x{commandByte:X2}", ex);
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            // Стандартный протокол
            if (serialPortController == null)
            {
                AppLogger.Error($"Попытка отправить 0x{commandByte:X2}: serialPortController == null");
                MessageBox.Show("Контроллер последовательного порта не инициализирован.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (MyserialPort == null)
            {
                AppLogger.Error($"Попытка отправить 0x{commandByte:X2}: MyserialPort == null");
                MessageBox.Show("Последовательный порт не инициализирован.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!MyserialPort.IsOpen)
            {
                AppLogger.Warning($"Попытка отправить 0x{commandByte:X2}: COM порт закрыт");
                MessageBox.Show("COM порт закрыт. Подключитесь к порту и повторите попытку.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            AppLogger.Debug($"UI -> отправка команды 0x{commandByte:X2}, шаг={stepUm} um");
            
            try
            {
                bool success = await serialPortController.SendCommandAsync(commandByte, stepUm);
                
                if (!success)
                {
                    AppLogger.Warning($"Команда 0x{commandByte:X2} не выполнена (SendCommandAsync вернул false).");
                    MessageBox.Show(
                        $"Не удалось выполнить команду 0x{commandByte:X2}.\n\n" +
                        "Возможные причины:\n" +
                        "• Arduino не отвечает (проверьте подключение)\n" +
                        "• Тайм-аут команды (3 секунды)\n" +
                        "• Arduino вернул ошибку (проверьте логи)\n\n" +
                        "Проверьте Serial Monitor Arduino и логи приложения.",
                        "Ошибка отправки команды",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    AppLogger.Debug($"Команда 0x{commandByte:X2} успешно выполнена.");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Исключение при отправке команды 0x{commandByte:X2}", ex);
                MessageBox.Show(
                    $"Исключение при отправке команды:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
    
        /// <summary>
        /// ✅ Проверка в ВИРТУАЛЬНЫХ координатах
        /// </summary>
        private bool CanMoveTo(float xMm, float yMm)
        {
            float r = waferController?.WaferDiameter > 0 ? waferController.WaferDiameter / 2f : 100f;
            return (xMm * xMm + yMm * yMm) <= (r * r) + 1e-6f;
        }
    }
}
