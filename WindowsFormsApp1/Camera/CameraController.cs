using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;

namespace WindowsFormsApp1.Camera
{
    /// <summary>
    /// Manages the USB camera feed and computer vision operations.
    /// </summary>
    public class CameraController : IDisposable
    {
        private VideoCaptureDevice videoSource;
        private Bitmap currentFrame;
        private readonly object frameLock = new object();
        private bool isRunning = false;
        private bool isFrozen = false;
        private Bitmap frozenFrame;

        // Настройки камеры
        private CameraSettings settings;

        // События
        public event EventHandler<Bitmap> FrameCaptured;
        public event EventHandler<string> StatusChanged;

        public CameraController()
        {
            settings = new CameraSettings();
        }

        /// <summary>
        /// Получить список доступных камер
        /// </summary>
        public FilterInfoCollection GetAvailableCameras()
        {
            try
            {
                return new FilterInfoCollection(FilterCategory.VideoInputDevice);
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка получения списка камер: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Инициализация камеры по индексу
        /// </summary>
        public bool Initialize(int cameraIndex)
        {
            try
            {
                var cameras = GetAvailableCameras();
                if (cameras == null || cameraIndex < 0 || cameraIndex >= cameras.Count)
                {
                    OnStatusChanged("Камера не найдена");
                    return false;
                }

                return Initialize(cameras[cameraIndex].MonikerString);
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка инициализации: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Инициализация камеры по MonikerString
        /// </summary>
        public bool Initialize(string monikerString)
        {
            try
            {
                Stop();

                videoSource = new VideoCaptureDevice(monikerString);
                
                // Применяем настройки разрешения
                ApplyResolution();

                videoSource.NewFrame += VideoSource_NewFrame;
                
                OnStatusChanged("Камера инициализирована");
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка инициализации: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Запуск захвата видео
        /// </summary>
        public bool Start()
        {
            try
            {
                if (videoSource == null)
                {
                    OnStatusChanged("Камера не инициализирована");
                    return false;
                }

                if (isRunning)
                {
                    OnStatusChanged("Камера уже работает");
                    return true;
                }

                videoSource.Start();
                isRunning = true;
                OnStatusChanged("Камера запущена");
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка запуска: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Остановка захвата видео
        /// </summary>
        public void Stop()
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                }

                isRunning = false;
                OnStatusChanged("Камера остановлена");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка остановки: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработка нового кадра
        /// </summary>
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                lock (frameLock)
                {
                    // Клонируем кадр
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)eventArgs.Frame.Clone();
                }

                // Уведомляем подписчиков
                FrameCaptured?.Invoke(this, currentFrame);
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка обработки кадра: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить текущий кадр
        /// </summary>
        public Bitmap GetCurrentFrame()
        {
            if (isFrozen && frozenFrame != null)
            {
                return (Bitmap)frozenFrame.Clone();
            }

            lock (frameLock)
            {
                return currentFrame != null ? (Bitmap)currentFrame.Clone() : null;
            }
        }

        /// <summary>
        /// Заморозить изображение
        /// </summary>
        public void FreezeFrame(bool freeze)
        {
            isFrozen = freeze;
            
            if (freeze)
            {
                lock (frameLock)
                {
                    frozenFrame?.Dispose();
                    frozenFrame = currentFrame != null ? (Bitmap)currentFrame.Clone() : null;
                }
                OnStatusChanged("Изображение заморожено");
            }
            else
            {
                frozenFrame?.Dispose();
                frozenFrame = null;
                OnStatusChanged("Изображение разморожено");
            }
        }

        /// <summary>
        /// Сохранить снимок
        /// </summary>
        public bool SaveSnapshot(string filePath)
        {
            try
            {
                var frame = GetCurrentFrame();
                if (frame == null)
                {
                    OnStatusChanged("Нет кадра для сохранения");
                    return false;
                }

                frame.Save(filePath, ImageFormat.Png);
                frame.Dispose();
                
                OnStatusChanged($"Снимок сохранен: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка сохранения: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Применить разрешение
        /// </summary>
        private void ApplyResolution()
        {
            if (videoSource == null) return;

            try
            {
                var capabilities = videoSource.VideoCapabilities;
                if (capabilities == null || capabilities.Length == 0) return;

                // Ищем подходящее разрешение
                VideoCapabilities selectedCap = null;
                
                foreach (var cap in capabilities)
                {
                    if (cap.FrameSize.Width == settings.Resolution.Width &&
                        cap.FrameSize.Height == settings.Resolution.Height)
                    {
                        selectedCap = cap;
                        break;
                    }
                }

                // Если не нашли точное - берем ближайшее
                if (selectedCap == null && capabilities.Length > 0)
                {
                    selectedCap = capabilities[0];
                    foreach (var cap in capabilities)
                    {
                        if (cap.FrameSize.Width * cap.FrameSize.Height > 
                            selectedCap.FrameSize.Width * selectedCap.FrameSize.Height)
                        {
                            selectedCap = cap;
                        }
                    }
                }

                if (selectedCap != null)
                {
                    videoSource.VideoResolution = selectedCap;
                    OnStatusChanged($"Разрешение установлено: {selectedCap.FrameSize.Width}x{selectedCap.FrameSize.Height}");
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка установки разрешения: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить/установить настройки
        /// </summary>
        public CameraSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                ApplySettings();
            }
        }

        /// <summary>
        /// Применить все настройки
        /// </summary>
        public void ApplySettings()
        {
            if (videoSource == null)
            {
                OnStatusChanged("❌ Камера не инициализирована");
                return;
            }

            if (!isRunning)
            {
                OnStatusChanged("❌ Камера не запущена. Запустите камеру для применения настроек.");
                return;
            }

            try
            {
                // Применяем разрешение
                ApplyResolution();

                // Применяем настройки изображения через DirectShow
                var appliedSettings = new System.Collections.Generic.List<string>();
                var failedSettings = new System.Collections.Generic.List<string>();

                // Яркость
                if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.Brightness, settings.Brightness))
                    appliedSettings.Add("Яркость");
                else
                    failedSettings.Add("Яркость");

                // Контраст
                if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.Contrast, settings.Contrast))
                    appliedSettings.Add("Контраст");
                else
                    failedSettings.Add("Контраст");

                // Насыщенность
                if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.Saturation, settings.Saturation))
                    appliedSettings.Add("Насыщенность");
                else
                    failedSettings.Add("Насыщенность");

                // Резкость
                if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.Sharpness, settings.Sharpness))
                    appliedSettings.Add("Резкость");
                else
                    failedSettings.Add("Резкость");

                // Усиление (Gain)
                if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.Gain, settings.Gain))
                    appliedSettings.Add("Усиление");
                else
                    failedSettings.Add("Усиление");

                // Баланс белого
                if (!settings.AutoWhiteBalance)
                {
                    if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.WhiteBalance, settings.WhiteBalance, VideoProcAmpFlags.Manual))
                        appliedSettings.Add("Баланс белого");
                    else
                        failedSettings.Add("Баланс белого");
                }
                else
                {
                    if (TrySetVideoProcAmpProperty(VideoProcAmpProperty.WhiteBalance, 0, VideoProcAmpFlags.Auto))
                        appliedSettings.Add("Баланс белого (авто)");
                    else
                        failedSettings.Add("Баланс белого (авто)");
                }

                // Экспозиция
                if (!settings.AutoExposure)
                {
                    if (TrySetCameraControlProperty(CameraControlProperty.Exposure, settings.Exposure, CameraControlFlags.Manual))
                        appliedSettings.Add("Экспозиция");
                    else
                        failedSettings.Add("Экспозиция");
                }
                else
                {
                    if (TrySetCameraControlProperty(CameraControlProperty.Exposure, 0, CameraControlFlags.Auto))
                        appliedSettings.Add("Экспозиция (авто)");
                    else
                        failedSettings.Add("Экспозиция (авто)");
                }

                // Формируем сообщение о результате
                if (appliedSettings.Count == 0 && failedSettings.Count > 0)
                {
                    OnStatusChanged("❌ Настройки не применены: камера не поддерживает DirectShow API");
                }
                else
                {
                    string message = "✓ Настройки применены";
                    if (appliedSettings.Count > 0)
                    {
                        message += $"\n✓ Применено ({appliedSettings.Count}): " + string.Join(", ", appliedSettings);
                    }
                    if (failedSettings.Count > 0)
                    {
                        message += $"\n✗ Не поддерживается ({failedSettings.Count}): " + string.Join(", ", failedSettings);
                    }
                    OnStatusChanged(message);
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged($"❌ Ошибка применения настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Попытка установить свойство VideoProcAmp
        /// </summary>
        private bool TrySetVideoProcAmpProperty(VideoProcAmpProperty property, int value, VideoProcAmpFlags flags = VideoProcAmpFlags.Manual)
        {
            try
            {
                if (videoSource?.SourceObject == null) return false;

                var sourceObject = videoSource.SourceObject as IAMVideoProcAmp;
                if (sourceObject == null) return false;

                // Получаем диапазон значений
                int min, max, step, def;
                VideoProcAmpFlags capsFlags;
                int hr = sourceObject.GetRange(property, out min, out max, out step, out def, out capsFlags);
                
                if (hr != 0) return false;

                // Проверяем поддержку режима
                if (flags == VideoProcAmpFlags.Auto && (capsFlags & VideoProcAmpFlags.Auto) == 0)
                    return false;

                // Нормализуем значение в диапазон
                int normalizedValue = Math.Max(min, Math.Min(max, min + (value * (max - min) / 100)));

                // Устанавливаем значение
                hr = sourceObject.Set(property, normalizedValue, flags);
                return hr == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Попытка установить свойство CameraControl
        /// </summary>
        private bool TrySetCameraControlProperty(CameraControlProperty property, int value, CameraControlFlags flags = CameraControlFlags.Manual)
        {
            try
            {
                if (videoSource?.SourceObject == null) return false;

                var sourceObject = videoSource.SourceObject as IAMCameraControl;
                if (sourceObject == null) return false;

                // Получаем диапазон значений
                int min, max, step, def;
                CameraControlFlags capsFlags;
                int hr = sourceObject.GetRange(property, out min, out max, out step, out def, out capsFlags);
                
                if (hr != 0) return false;

                // Проверяем поддержку режима
                if (flags == CameraControlFlags.Auto && (capsFlags & CameraControlFlags.Auto) == 0)
                    return false;

                // Для экспозиции значение в логарифмической шкале (секунды * 10000)
                int normalizedValue = flags == CameraControlFlags.Manual ? 
                    Math.Max(min, Math.Min(max, -value)) : 0;

                // Устанавливаем значение
                hr = sourceObject.Set(property, normalizedValue, flags);
                return hr == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверить поддержку настроек камерой
        /// </summary>
        public CameraCapabilities GetCameraCapabilities()
        {
            var caps = new CameraCapabilities();

            if (videoSource?.SourceObject == null)
                return caps;

            try
            {
                var videoProcAmp = videoSource.SourceObject as IAMVideoProcAmp;
                var cameraControl = videoSource.SourceObject as IAMCameraControl;

                if (videoProcAmp != null)
                {
                    caps.SupportsBrightness = CheckVideoProcAmpSupport(videoProcAmp, VideoProcAmpProperty.Brightness);
                    caps.SupportsContrast = CheckVideoProcAmpSupport(videoProcAmp, VideoProcAmpProperty.Contrast);
                    caps.SupportsSaturation = CheckVideoProcAmpSupport(videoProcAmp, VideoProcAmpProperty.Saturation);
                    caps.SupportsSharpness = CheckVideoProcAmpSupport(videoProcAmp, VideoProcAmpProperty.Sharpness);
                    caps.SupportsGain = CheckVideoProcAmpSupport(videoProcAmp, VideoProcAmpProperty.Gain);
                    caps.SupportsWhiteBalance = CheckVideoProcAmpSupport(videoProcAmp, VideoProcAmpProperty.WhiteBalance);
                }

                if (cameraControl != null)
                {
                    caps.SupportsExposure = CheckCameraControlSupport(cameraControl, CameraControlProperty.Exposure);
                }
            }
            catch { }

            return caps;
        }

        private bool CheckVideoProcAmpSupport(IAMVideoProcAmp procAmp, VideoProcAmpProperty property)
        {
            try
            {
                int min, max, step, def;
                VideoProcAmpFlags flags;
                return procAmp.GetRange(property, out min, out max, out step, out def, out flags) == 0;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckCameraControlSupport(IAMCameraControl control, CameraControlProperty property)
        {
            try
            {
                int min, max, step, def;
                CameraControlFlags flags;
                return control.GetRange(property, out min, out max, out step, out def, out flags) == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Статус работы камеры
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// Статус заморозки кадра
        /// </summary>
        public bool IsFrozen => isFrozen;

        /// <summary>
        /// Уведомление об изменении статуса
        /// </summary>
        private void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            Stop();
            
            currentFrame?.Dispose();
            frozenFrame?.Dispose();
            
            if (videoSource != null)
            {
                videoSource.NewFrame -= VideoSource_NewFrame;
                videoSource = null;
            }
        }
    }

    /// <summary>
    /// Настройки камеры
    /// </summary>
    public class CameraSettings
    {
        // Базовые параметры изображения
        public int Brightness { get; set; } = 50;       // 0-100
        public int Contrast { get; set; } = 50;         // 0-100
        public int Saturation { get; set; } = 50;       // 0-100
        public int Sharpness { get; set; } = 5;         // 0-10
        public int Gain { get; set; } = 0;              // 0-100

        // Экспозиция
        public bool AutoExposure { get; set; } = true;
        public int Exposure { get; set; } = 100;        // 1-1000 мс

        // Баланс белого
        public bool AutoWhiteBalance { get; set; } = true;
        public int WhiteBalance { get; set; } = 5000;   // Температура цвета

        // Разрешение и FPS
        public Size Resolution { get; set; } = new Size(1280, 720);
        public int FrameRate { get; set; } = 30;        // FPS

        // ROI (Region of Interest)
        public bool UseROI { get; set; } = false;
        public Rectangle ROI { get; set; } = Rectangle.Empty;

        /// <summary>
        /// Клонирование настроек
        /// </summary>
        public CameraSettings Clone()
        {
            return (CameraSettings)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Возможности камеры
    /// </summary>
    public class CameraCapabilities
    {
        public bool SupportsBrightness { get; set; }
        public bool SupportsContrast { get; set; }
        public bool SupportsSaturation { get; set; }
        public bool SupportsSharpness { get; set; }
        public bool SupportsGain { get; set; }
        public bool SupportsWhiteBalance { get; set; }
        public bool SupportsExposure { get; set; }
    }

    // DirectShow COM интерфейсы
    [ComImport, Guid("C6E13370-30AC-11d0-A18C-00A0C9118956"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAMVideoProcAmp
    {
        [PreserveSig]
        int GetRange(
            [In] VideoProcAmpProperty Property,
            [Out] out int pMin,
            [Out] out int pMax,
            [Out] out int pSteppingDelta,
            [Out] out int pDefault,
            [Out] out VideoProcAmpFlags pCapsFlags);

        [PreserveSig]
        int Set(
            [In] VideoProcAmpProperty Property,
            [In] int lValue,
            [In] VideoProcAmpFlags Flags);

        [PreserveSig]
        int Get(
            [In] VideoProcAmpProperty Property,
            [Out] out int lValue,
            [Out] out VideoProcAmpFlags Flags);
    }

    [ComImport, Guid("C6E13370-30AC-11d0-A18C-00A0C9118956"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAMCameraControl
    {
        [PreserveSig]
        int GetRange(
            [In] CameraControlProperty Property,
            [Out] out int pMin,
            [Out] out int pMax,
            [Out] out int pSteppingDelta,
            [Out] out int pDefault,
            [Out] out CameraControlFlags pCapsFlags);

        [PreserveSig]
        int Set(
            [In] CameraControlProperty Property,
            [In] int lValue,
            [In] CameraControlFlags Flags);

        [PreserveSig]
        int Get(
            [In] CameraControlProperty Property,
            [Out] out int lValue,
            [Out] out CameraControlFlags Flags);
    }

    internal enum VideoProcAmpProperty
    {
        Brightness,
        Contrast,
        Hue,
        Saturation,
        Sharpness,
        Gamma,
        ColorEnable,
        WhiteBalance,
        BacklightCompensation,
        Gain
    }

    [Flags]
    internal enum VideoProcAmpFlags
    {
        Auto = 0x0001,
        Manual = 0x0002
    }

    internal enum CameraControlProperty
    {
        Pan,
        Tilt,
        Roll,
        Zoom,
        Exposure,
        Iris,
        Focus
    }

    [Flags]
    internal enum CameraControlFlags
    {
        Auto = 0x0001,
        Manual = 0x0002
    }

}
