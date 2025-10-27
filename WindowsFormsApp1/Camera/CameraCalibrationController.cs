using System;
using System.Drawing;
using System.Threading.Tasks;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace WindowsFormsApp1.Camera
{
    /// <summary>
    /// Контроллер калибровки камеры для определения масштаба (мм/пиксель)
    /// </summary>
    public class CameraCalibrationController
    {
        private readonly CameraController cameraController;
        private readonly CrystalTable.Form1 form;

        // Состояние калибровки
        private CalibrationState state = CalibrationState.NotStarted;
        
        // Данные калибровки
        private Bitmap referenceFrame;
        private PointF referenceMachinePosition;
        private Bitmap currentFrame;
        private PointF currentMachinePosition;
        private PointF measuredDisplacementPx;
        private CameraCalibrationData calibrationResult;

        // События
        public event EventHandler<CalibrationStateChangedEventArgs> StateChanged;
        public event EventHandler<string> StatusMessage;

        public CameraCalibrationController(CameraController camera, CrystalTable.Form1 mainForm)
        {
            cameraController = camera ?? throw new ArgumentNullException(nameof(camera));
            form = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
        }

        /// <summary>
        /// Текущее состояние калибровки
        /// </summary>
        public CalibrationState State => state;

        /// <summary>
        /// ШАГ 1: Захват эталонного кадра
        /// </summary>
        public bool Step1_CaptureReferenceFrame()
        {
            try
            {
                OnStatusMessage("Шаг 1: Захват эталонного кадра...");

                if (!cameraController.IsRunning)
                {
                    OnStatusMessage("ОШИБКА: Камера не запущена!");
                    return false;
                }

                // Захватываем текущий кадр
                referenceFrame?.Dispose();
                referenceFrame = cameraController.GetCurrentFrame();

                if (referenceFrame == null)
                {
                    OnStatusMessage("ОШИБКА: Не удалось получить кадр с камеры!");
                    return false;
                }

                // Получаем текущую позицию машины (физические координаты ЛШД)
                referenceMachinePosition = form.GetPointerMachineMm();

                // Меняем состояние
                ChangeState(CalibrationState.ReferenceFrameCaptured);

                OnStatusMessage(
                    $"✓ Эталонный кадр сохранен\n" +
                    $"  Разрешение: {referenceFrame.Width}×{referenceFrame.Height} пикселей\n" +
                    $"  Позиция ЛШД: ({referenceMachinePosition.X:F2}, {referenceMachinePosition.Y:F2}) мм");

                return true;
            }
            catch (Exception ex)
            {
                OnStatusMessage($"ОШИБКА на шаге 1: {ex.Message}");
                ChangeState(CalibrationState.Error);
                return false;
            }
        }

        /// <summary>
        /// ШАГ 2: Выполнить смещение на точный шаг
        /// </summary>
        public async Task<bool> Step2_ExecuteMovement(float stepX, float stepY)
        {
            try
            {
                if (state != CalibrationState.ReferenceFrameCaptured)
                {
                    OnStatusMessage("ОШИБКА: Сначала выполните шаг 1!");
                    return false;
                }

                OnStatusMessage($"Шаг 2: Смещение на ({stepX:F2}, {stepY:F2}) мм...");
                ChangeState(CalibrationState.MovementInProgress);

                // Выполняем смещение через форму
                bool success = await form.MoveRelativeAsync(stepX, stepY);

                if (!success)
                {
                    OnStatusMessage("ОШИБКА: Не удалось выполнить смещение!");
                    ChangeState(CalibrationState.Error);
                    return false;
                }

                // Небольшая задержка для стабилизации
                await Task.Delay(500);

                // Получаем новую позицию машины
                currentMachinePosition = form.GetPointerMachineMm();

                // Вычисляем фактическое командное смещение
                float actualCommandX = currentMachinePosition.X - referenceMachinePosition.X;
                float actualCommandY = currentMachinePosition.Y - referenceMachinePosition.Y;

                ChangeState(CalibrationState.MovementCompleted);

                OnStatusMessage(
                    $"✓ Смещение выполнено\n" +
                    $"  Командное: ({stepX:F3}, {stepY:F3}) мм\n" +
                    $"  Фактическое (по энкодерам): ({actualCommandX:F3}, {actualCommandY:F3}) мм\n" +
                    $"  Новая позиция: ({currentMachinePosition.X:F2}, {currentMachinePosition.Y:F2}) мм");

                return true;
            }
            catch (Exception ex)
            {
                OnStatusMessage($"ОШИБКА на шаге 2: {ex.Message}");
                ChangeState(CalibrationState.Error);
                return false;
            }
        }

        /// <summary>
        /// ШАГ 3: Измерить смещение по изображению
        /// </summary>
        public bool Step3_MeasureDisplacement()
        {
            try
            {
                if (state != CalibrationState.MovementCompleted)
                {
                    OnStatusMessage("ОШИБКА: Сначала выполните шаги 1 и 2!");
                    return false;
                }

                OnStatusMessage("Шаг 3: Измерение смещения по изображению...");
                ChangeState(CalibrationState.MeasurementInProgress);

                // Захватываем новый кадр ПОСЛЕ смещения
                currentFrame?.Dispose();
                currentFrame = cameraController.GetCurrentFrame();

                if (currentFrame == null)
                {
                    OnStatusMessage("ОШИБКА: Не удалось получить новый кадр!");
                    ChangeState(CalibrationState.Error);
                    return false;
                }

                // Применяем гистограммную эквализацию для улучшения сопоставления
                var refProcessed = ApplyHistogramEqualization(referenceFrame);
                var curProcessed = ApplyHistogramEqualization(currentFrame);

                // Вычисляем смещение между изображениями
                measuredDisplacementPx = CalculateImageDisplacement(refProcessed, curProcessed);

                refProcessed.Dispose();
                curProcessed.Dispose();

                if (measuredDisplacementPx == PointF.Empty)
                {
                    OnStatusMessage("ОШИБКА: Не удалось сопоставить изображения!");
                    ChangeState(CalibrationState.Error);
                    return false;
                }

                ChangeState(CalibrationState.MeasurementCompleted);

                OnStatusMessage(
                    $"✓ Смещение измерено\n" +
                    $"  Δx = {measuredDisplacementPx.X:F1} пикселей\n" +
                    $"  Δy = {measuredDisplacementPx.Y:F1} пикселей");

                return true;
            }
            catch (Exception ex)
            {
                OnStatusMessage($"ОШИБКА на шаге 3: {ex.Message}");
                ChangeState(CalibrationState.Error);
                return false;
            }
        }

        /// <summary>
        /// ШАГ 4: Вычислить масштаб камеры и установить калибровку
        /// </summary>
        public bool Step4_CalculateCalibration()
        {
            try
            {
                if (state != CalibrationState.MeasurementCompleted)
                {
                    OnStatusMessage("ОШИБКА: Сначала выполните шаги 1-3!");
                    return false;
                }

                OnStatusMessage("Шаг 4: Вычисление масштаба камеры...");

                // Вычисляем фактическое смещение машины (по энкодерам)
                float commandedX = currentMachinePosition.X - referenceMachinePosition.X;
                float commandedY = currentMachinePosition.Y - referenceMachinePosition.Y;

                // Вычисляем масштаб (мм/пиксель)
                double scaleX = Math.Abs(commandedX / measuredDisplacementPx.X);
                double scaleY = Math.Abs(commandedY / measuredDisplacementPx.Y);

                // Используем среднее значение
                double mmPerPixel = (scaleX + scaleY) / 2.0;

                // Проверка валидности
                if (double.IsNaN(mmPerPixel) || double.IsInfinity(mmPerPixel) || mmPerPixel <= 0)
                {
                    OnStatusMessage("ОШИБКА: Некорректный результат калибровки!");
                    ChangeState(CalibrationState.Error);
                    return false;
                }

                // Проверка разумности диапазона (0.01 - 0.5 мм/px)
                if (mmPerPixel < 0.001 || mmPerPixel > 1.0)
                {
                    OnStatusMessage(
                        $"ПРЕДУПРЕЖДЕНИЕ: Масштаб {mmPerPixel:F4} мм/px выходит за разумные пределы!\n" +
                        $"Проверьте настройки камеры и расстояние до объекта.");
                }

                // Вычисляем ошибку оценки
                double errorEstimate = Math.Abs(scaleX - scaleY) / mmPerPixel * 100.0;

                // Создаем объект с результатами калибровки
                calibrationResult = new CameraCalibrationData
                {
                    MillimetersPerPixel = mmPerPixel,
                    CalibrationDate = DateTime.Now,
                    ImageResolution = new Size(referenceFrame.Width, referenceFrame.Height),
                    WorkingDistance = 0f, // TODO: Определить из настроек
                    CommandedStepX = commandedX,
                    CommandedStepY = commandedY,
                    MeasuredStepPxX = measuredDisplacementPx.X,
                    MeasuredStepPxY = measuredDisplacementPx.Y,
                    ScaleX = scaleX,
                    ScaleY = scaleY,
                    ErrorEstimatePercent = (float)errorEstimate
                };

                ChangeState(CalibrationState.Completed);

                OnStatusMessage(
                    $"✅ КАЛИБРОВКА ЗАВЕРШЕНА!\n\n" +
                    $"📊 РЕЗУЛЬТАТЫ:\n" +
                    $"────────────────────────────────\n" +
                    $"  Командное смещение:  {commandedX:F3} × {commandedY:F3} мм\n" +
                    $"  Измеренное смещение: {measuredDisplacementPx.X:F1} × {measuredDisplacementPx.Y:F1} пикселей\n\n" +
                    $"  МАСШТАБ КАМЕРЫ:\n" +
                    $"  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━┓\n" +
                    $"  ┃  {mmPerPixel:F5} мм/пиксель  ┃\n" +
                    $"  ┃  {mmPerPixel * 1000:F2} мкм/пиксель   ┃\n" +
                    $"  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━┛\n\n" +
                    $"  Масштаб X: {scaleX:F5} мм/px\n" +
                    $"  Масштаб Y: {scaleY:F5} мм/px\n" +
                    $"  Расхождение: {errorEstimate:F2}%\n" +
                    $"  Точность: {(errorEstimate < 5 ? "✓ ОТЛИЧНО" : errorEstimate < 10 ? "⚠ ПРИЕМЛЕМО" : "❌ ПЛОХО")}\n" +
                    $"────────────────────────────────");

                return true;
            }
            catch (Exception ex)
            {
                OnStatusMessage($"ОШИБКА на шаге 4: {ex.Message}");
                ChangeState(CalibrationState.Error);
                return false;
            }
        }

        /// <summary>
        /// Получить результат калибровки
        /// </summary>
        public CameraCalibrationData GetCalibrationResult()
        {
            return calibrationResult;
        }

        /// <summary>
        /// Сбросить калибровку и начать заново
        /// </summary>
        public void Reset()
        {
            referenceFrame?.Dispose();
            currentFrame?.Dispose();
            referenceFrame = null;
            currentFrame = null;
            calibrationResult = null;
            ChangeState(CalibrationState.NotStarted);
            OnStatusMessage("Калибровка сброшена");
        }

        /// <summary>
        /// Применить гистограммную эквализацию к изображению
        /// </summary>
        private Bitmap ApplyHistogramEqualization(Bitmap source)
        {
            // Преобразуем в оттенки серого
            var grayscale = new Grayscale(0.2125, 0.7154, 0.0721);
            var grayImage = grayscale.Apply(source);

            // Применяем гистограммную эквализацию
            var equalization = new HistogramEqualization();
            equalization.ApplyInPlace(grayImage);

            return grayImage;
        }

        /// <summary>
        /// Вычислить смещение между двумя изображениями методом Template Matching
        /// </summary>
        private PointF CalculateImageDisplacement(Bitmap frame1, Bitmap frame2)
        {
            try
            {
                // Размер шаблона (квадратная область из центра первого кадра)
                int templateSize = Math.Min(frame1.Width, frame1.Height) / 3;
                templateSize = Math.Min(templateSize, 200); // Максимум 200x200

                // Вырезаем шаблон из центра первого кадра
                Rectangle templateRect = new Rectangle(
                    (frame1.Width - templateSize) / 2,
                    (frame1.Height - templateSize) / 2,
                    templateSize,
                    templateSize);

                var template = frame1.Clone(templateRect, frame1.PixelFormat);

                // Ищем шаблон на втором кадре с помощью Exhaustive Template Matching
                var tm = new ExhaustiveTemplateMatching(0.8f); // Минимальное сходство 80%
                TemplateMatch[] matches = tm.ProcessImage(frame2, template);

                template.Dispose();

                if (matches.Length == 0)
                {
                    return PointF.Empty;
                }

                // Берем лучшее совпадение
                var bestMatch = matches[0];

                // Вычисляем смещение
                float centerX1 = templateRect.X + templateRect.Width / 2f;
                float centerY1 = templateRect.Y + templateRect.Height / 2f;

                float centerX2 = bestMatch.Rectangle.X + bestMatch.Rectangle.Width / 2f;
                float centerY2 = bestMatch.Rectangle.Y + bestMatch.Rectangle.Height / 2f;

                float deltaX = centerX2 - centerX1;
                float deltaY = centerY2 - centerY1;

                // Логируем качество совпадения
                OnStatusMessage($"  Качество совпадения: {bestMatch.Similarity:P1}");

                return new PointF(deltaX, deltaY);
            }
            catch (Exception ex)
            {
                OnStatusMessage($"Ошибка сопоставления изображений: {ex.Message}");
                return PointF.Empty;
            }
        }

        private void ChangeState(CalibrationState newState)
        {
            var oldState = state;
            state = newState;
            StateChanged?.Invoke(this, new CalibrationStateChangedEventArgs(oldState, newState));
        }

        private void OnStatusMessage(string message)
        {
            StatusMessage?.Invoke(this, message);
        }

        public void Dispose()
        {
            referenceFrame?.Dispose();
            currentFrame?.Dispose();
        }
    }

    /// <summary>
    /// Состояние калибровки
    /// </summary>
    public enum CalibrationState
    {
        NotStarted,
        ReferenceFrameCaptured,
        MovementInProgress,
        MovementCompleted,
        MeasurementInProgress,
        MeasurementCompleted,
        Completed,
        Error
    }

    /// <summary>
    /// Данные калибровки камеры
    /// </summary>
    public class CameraCalibrationData
    {
        // Основной результат
        public double MillimetersPerPixel { get; set; }

        // Метаданные
        public DateTime CalibrationDate { get; set; }
        public Size ImageResolution { get; set; }
        public float WorkingDistance { get; set; }

        // Исходные данные для проверки
        public float CommandedStepX { get; set; }
        public float CommandedStepY { get; set; }
        public float MeasuredStepPxX { get; set; }
        public float MeasuredStepPxY { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public float ErrorEstimatePercent { get; set; }

        /// <summary>
        /// Преобразовать пиксели в миллиметры
        /// </summary>
        public float PixelsToMillimeters(float pixels)
        {
            return (float)(pixels * MillimetersPerPixel);
        }

        /// <summary>
        /// Преобразовать миллиметры в пиксели
        /// </summary>
        public float MillimetersToPixels(float millimeters)
        {
            return (float)(millimeters / MillimetersPerPixel);
        }

        /// <summary>
        /// Проверить валидность калибровки
        /// </summary>
        public bool IsValid()
        {
            // Масштаб в разумных пределах (0.01 - 0.5 мм/px)
            if (MillimetersPerPixel < 0.001 || MillimetersPerPixel > 1.0)
                return false;

            // Расхождение X и Y не более 10%
            if (ErrorEstimatePercent > 10)
                return false;

            // Разрешение валидно
            if (ImageResolution.Width < 100 || ImageResolution.Height < 100)
                return false;

            return true;
        }

        /// <summary>
        /// Получить строковое представление
        /// </summary>
        public override string ToString()
        {
            return $"{MillimetersPerPixel:F5} мм/пиксель ({MillimetersPerPixel * 1000:F2} мкм/пиксель)";
        }
    }

    /// <summary>
    /// Аргументы события изменения состояния калибровки
    /// </summary>
    public class CalibrationStateChangedEventArgs : EventArgs
    {
        public CalibrationState OldState { get; }
        public CalibrationState NewState { get; }

        public CalibrationStateChangedEventArgs(CalibrationState oldState, CalibrationState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
