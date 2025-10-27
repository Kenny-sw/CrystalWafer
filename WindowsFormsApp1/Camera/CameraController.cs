using System;
using System.Drawing;
using System.Drawing.Imaging;
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
            if (videoSource == null) return;

            try
            {
                // Применяем разрешение
                ApplyResolution();

                // Применяем настройки изображения через SetCameraProperty
                // Примечание: не все камеры поддерживают все свойства
                
                OnStatusChanged("Настройки применены");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка применения настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Сброс настроек к значениям по умолчанию
        /// </summary>
        public void ResetSettings()
        {
            settings = new CameraSettings();
            ApplySettings();
            OnStatusChanged("Настройки сброшены");
        }

        /// <summary>
        /// Получить возможности камеры
        /// </summary>
        public VideoCapabilities[] GetVideoCapabilities()
        {
            return videoSource?.VideoCapabilities;
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
}
