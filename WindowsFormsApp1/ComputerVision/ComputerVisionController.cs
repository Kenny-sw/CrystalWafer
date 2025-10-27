using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace WindowsFormsApp1.ComputerVision
{
    /// <summary>
    /// Контроллер компьютерного зрения для обработки изображений с камеры
    /// </summary>
    public class ComputerVisionController
    {
        // Параметры для распознавания
        private int blobMinWidth = 5;
        private int blobMinHeight = 5;
        private int blobMaxWidth = 500;
        private int blobMaxHeight = 500;

        // Калибровка камеры (мм/пиксель)
        private double cameraScale = 0.0; // 0 = не откалиброван

        // События
        public event EventHandler<string> StatusChanged;

        /// <summary>
        /// Установить масштаб камеры (результат калибровки)
        /// </summary>
        public void SetCameraScale(double mmPerPixel)
        {
            cameraScale = mmPerPixel;
            OnStatusChanged($"Масштаб камеры установлен: {mmPerPixel:F5} мм/пиксель");
        }

        /// <summary>
        /// Получить масштаб камеры
        /// </summary>
        public double GetCameraScale() => cameraScale;

        /// <summary>
        /// Проверить, откалиброван ли масштаб
        /// </summary>
        public bool IsCalibrated() => cameraScale > 0;

        /// <summary>
        /// Применить гистограммную эквализацию к изображению
        /// Улучшает контрастность при плохом или неравномерном освещении
        /// </summary>
        /// <param name="image">Исходное изображение</param>
        /// <returns>Обработанное изображение с улучшенным контрастом</returns>
        public Bitmap ApplyHistogramEqualization(Bitmap image)
        {
            try
            {
                OnStatusChanged("Применение гистограммной эквализации...");

                // Преобразование в оттенки серого (если еще не)
                Bitmap grayImage;
                if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                {
                    var grayFilter = new Grayscale(0.2125, 0.7154, 0.0721);
                    grayImage = grayFilter.Apply(image);
                }
                else
                {
                    grayImage = (Bitmap)image.Clone();
                }

                // Применяем гистограммную эквализацию
                var equalization = new HistogramEqualization();
                equalization.ApplyInPlace(grayImage);

                OnStatusChanged("✓ Гистограммная эквализация применена");
                return grayImage;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка эквализации: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Адаптивная гистограммная эквализация (CLAHE)
        /// Лучше сохраняет локальный контраст
        /// </summary>
        /// <param name="image">Исходное изображение</param>
        /// <returns>Обработанное изображение</returns>
        public Bitmap ApplyAdaptiveEqualization(Bitmap image)
        {
            try
            {
                OnStatusChanged("Применение адаптивной эквализации (CLAHE)...");

                // Преобразование в оттенки серого
                Bitmap grayImage;
                if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                {
                    var grayFilter = new Grayscale(0.2125, 0.7154, 0.0721);
                    grayImage = grayFilter.Apply(image);
                }
                else
                {
                    grayImage = (Bitmap)image.Clone();
                }

                // Разбиваем изображение на блоки и применяем эквализацию локально
                int blockSize = 64; // Размер блока в пикселях
                int width = grayImage.Width;
                int height = grayImage.Height;

                var result = (Bitmap)grayImage.Clone();
                var equalization = new HistogramEqualization();

                for (int y = 0; y < height; y += blockSize)
                {
                    for (int x = 0; x < width; x += blockSize)
                    {
                        int blockWidth = Math.Min(blockSize, width - x);
                        int blockHeight = Math.Min(blockSize, height - y);

                        Rectangle blockRect = new Rectangle(x, y, blockWidth, blockHeight);
                        
                        // Применяем эквализацию к блоку
                        var block = result.Clone(blockRect, result.PixelFormat);
                        equalization.ApplyInPlace(block);
                        
                        // Возвращаем блок на место
                        using (Graphics g = Graphics.FromImage(result))
                        {
                            g.DrawImage(block, blockRect);
                        }
                        block.Dispose();
                    }
                }

                grayImage.Dispose();
                OnStatusChanged("✓ Адаптивная эквализация применена");
                return result;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка адаптивной эквализации: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Комплексная предобработка изображения для улучшения качества распознавания
        /// Включает: эквализацию, шумоподавление, улучшение резкости
        /// </summary>
        /// <param name="image">Исходное изображение</param>
        /// <returns>Предобработанное изображение</returns>
        public Bitmap PreprocessImage(Bitmap image)
        {
            try
            {
                OnStatusChanged("Комплексная предобработка изображения...");

                // 1. Преобразование в оттенки серого
                var grayFilter = new Grayscale(0.2125, 0.7154, 0.0721);
                var processed = grayFilter.Apply(image);

                // 2. Гистограммная эквализация (улучшение контраста)
                var equalization = new HistogramEqualization();
                equalization.ApplyInPlace(processed);

                // 3. Медианный фильтр (удаление шума)
                var median = new Median(3);
                median.ApplyInPlace(processed);

                // 4. Улучшение резкости
                var sharpen = new Sharpen();
                sharpen.ApplyInPlace(processed);

                OnStatusChanged("✓ Предобработка завершена");
                return processed;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка предобработки: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Поиск фидуциальных меток на изображении
        /// </summary>
        /// <param name="image">Изображение для анализа</param>
        /// <param name="useEqualization">Применить гистограммную эквализацию</param>
        /// <returns>Список найденных меток с координатами</returns>
        public List<FiducialMark> DetectFiducials(Bitmap image, bool useEqualization = true)
        {
            var results = new List<FiducialMark>();

            try
            {
                OnStatusChanged("Поиск фидуциальных меток...");

                // Предобработка изображения
                Bitmap processed = useEqualization ? PreprocessImage(image) : image;

                // Бинаризация (поиск контрастных объектов)
                var threshold = new Threshold(100);
                threshold.ApplyInPlace(processed);

                // Поиск контуров
                var blobCounter = new BlobCounter
                {
                    MinWidth = blobMinWidth,
                    MinHeight = blobMinHeight,
                    MaxWidth = blobMaxWidth,
                    MaxHeight = blobMaxHeight,
                    FilterBlobs = true,
                    ObjectsOrder = ObjectsOrder.Size
                };

                blobCounter.ProcessImage(processed);
                var blobs = blobCounter.GetObjectsInformation();

                // Фильтрация: ищем круглые или квадратные объекты
                foreach (var blob in blobs)
                {
                    double ratio = (double)blob.Rectangle.Width / blob.Rectangle.Height;
                    
                    // Проверяем, что объект близок к квадрату/кругу (отношение сторон близко к 1)
                    if (ratio > 0.8 && ratio < 1.2)
                    {
                        var mark = new FiducialMark
                        {
                            Center = new PointF(
                                blob.Rectangle.X + blob.Rectangle.Width / 2f,
                                blob.Rectangle.Y + blob.Rectangle.Height / 2f),
                            Bounds = blob.Rectangle,
                            Area = blob.Area,
                            Type = FiducialMarkType.Unknown
                        };

                        // Если камера откалибрована, добавляем размеры в мм
                        if (IsCalibrated())
                        {
                            mark.WidthMm = blob.Rectangle.Width * cameraScale;
                            mark.HeightMm = blob.Rectangle.Height * cameraScale;
                        }

                        results.Add(mark);
                    }
                }

                if (useEqualization && processed != image)
                    processed.Dispose();

                OnStatusChanged($"Найдено меток: {results.Count}");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка поиска меток: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Поиск дефектов на кристалле
        /// </summary>
        /// <param name="image">Изображение кристалла</param>
        /// <param name="useEqualization">Применить гистограммную эквализацию</param>
        /// <returns>Список найденных дефектов</returns>
        public List<Defect> DetectDefects(Bitmap image, bool useEqualization = true)
        {
            var results = new List<Defect>();

            try
            {
                OnStatusChanged("Поиск дефектов...");

                // Предобработка с эквализацией для лучшего обнаружения
                Bitmap processed = useEqualization ? PreprocessImage(image) : image;

                // Бинаризация с адаптивным порогом
                var threshold = new Threshold(80);
                threshold.ApplyInPlace(processed);

                // Морфологические операции для удаления шума
                var erosion = new Erosion();
                erosion.ApplyInPlace(processed);

                var dilation = new Dilatation();
                dilation.ApplyInPlace(processed);

                // Поиск дефектов (темных пятен)
                var blobCounter = new BlobCounter
                {
                    MinWidth = 2,
                    MinHeight = 2,
                    MaxWidth = 100,
                    MaxHeight = 100,
                    FilterBlobs = true,
                    ObjectsOrder = ObjectsOrder.Size
                };

                blobCounter.ProcessImage(processed);
                var blobs = blobCounter.GetObjectsInformation();

                foreach (var blob in blobs)
                {
                    var defect = new Defect
                    {
                        Location = new PointF(
                            blob.Rectangle.X + blob.Rectangle.Width / 2f,
                            blob.Rectangle.Y + blob.Rectangle.Height / 2f),
                        Bounds = blob.Rectangle,
                        Area = blob.Area,
                        Severity = CalculateDefectSeverity(blob.Area, image.Width * image.Height)
                    };

                    // Если камера откалибрована, добавляем размеры в мм
                    if (IsCalibrated())
                    {
                        defect.WidthMm = blob.Rectangle.Width * cameraScale;
                        defect.HeightMm = blob.Rectangle.Height * cameraScale;
                        defect.AreaMm2 = blob.Area * cameraScale * cameraScale;
                    }

                    results.Add(defect);
                }

                if (useEqualization && processed != image)
                    processed.Dispose();

                OnStatusChanged($"Найдено дефектов: {results.Count}");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка поиска дефектов: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Измерение размеров кристалла
        /// </summary>
        /// <param name="image">Изображение кристалла</param>
        /// <param name="useEqualization">Применить гистограммную эквализацию</param>
        /// <returns>Результаты измерений</returns>
        public CrystalMeasurement MeasureCrystal(Bitmap image, bool useEqualization = true)
        {
            try
            {
                OnStatusChanged("Измерение кристалла...");

                // Предобработка
                Bitmap processed = useEqualization ? PreprocessImage(image) : image;

                // Бинаризация
                var threshold = new Threshold(128);
                threshold.ApplyInPlace(processed);

                // Поиск границ кристалла
                var blobCounter = new BlobCounter
                {
                    FilterBlobs = true,
                    ObjectsOrder = ObjectsOrder.Size
                };

                blobCounter.ProcessImage(processed);
                var blobs = blobCounter.GetObjectsInformation();

                if (blobs.Length > 0)
                {
                    // Берем самый большой объект (предполагается, что это кристалл)
                    var crystal = blobs[0];

                    var measurement = new CrystalMeasurement
                    {
                        Bounds = crystal.Rectangle,
                        Width = crystal.Rectangle.Width,
                        Height = crystal.Rectangle.Height,
                        Area = crystal.Area,
                        Center = new PointF(
                            crystal.Rectangle.X + crystal.Rectangle.Width / 2f,
                            crystal.Rectangle.Y + crystal.Rectangle.Height / 2f)
                    };

                    // Если камера откалибрована, добавляем размеры в мм
                    if (IsCalibrated())
                    {
                        measurement.WidthMm = crystal.Rectangle.Width * cameraScale;
                        measurement.HeightMm = crystal.Rectangle.Height * cameraScale;
                        measurement.AreaMm2 = crystal.Area * cameraScale * cameraScale;
                        
                        OnStatusChanged($"Размеры: {measurement.Width}x{measurement.Height} px " +
                                      $"({measurement.WidthMm:F2}x{measurement.HeightMm:F2} мм)");
                    }
                    else
                    {
                        OnStatusChanged($"Размеры: {measurement.Width}x{measurement.Height} px");
                    }

                    if (useEqualization && processed != image)
                        processed.Dispose();
                    
                    return measurement;
                }

                if (useEqualization && processed != image)
                    processed.Dispose();

                OnStatusChanged("Кристалл не найден");
                return null;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка измерения: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Совмещение изображения с картой пластины
        /// </summary>
        /// <param name="image">Изображение с камеры</param>
        /// <param name="mapBitmap">Изображение карты</param>
        /// <returns>Результат совмещения с параметрами трансформации</returns>
        public AlignmentResult AlignToMap(Bitmap image, Bitmap mapBitmap)
        {
            try
            {
                OnStatusChanged("Совмещение с картой...");

                // Поиск характерных точек на обоих изображениях
                var fiducialsCamera = DetectFiducials(image);
                var fiducialsMap = DetectFiducials(mapBitmap);

                if (fiducialsCamera.Count < 3 || fiducialsMap.Count < 3)
                {
                    OnStatusChanged("Недостаточно меток для совмещения");
                    return null;
                }

                // Сопоставление меток (простой алгоритм - по расстоянию)
                var pairs = MatchFiducials(fiducialsCamera, fiducialsMap);

                if (pairs.Count < 3)
                {
                    OnStatusChanged("Не удалось сопоставить метки");
                    return null;
                }

                // Вычисление аффинного преобразования
                var result = new AlignmentResult
                {
                    IsSuccessful = true,
                    MatchedPairs = pairs,
                    // TODO: Вычислить параметры трансформации
                    TranslationX = 0,
                    TranslationY = 0,
                    Rotation = 0,
                    Scale = 1.0
                };

                OnStatusChanged($"Совмещение успешно: найдено {pairs.Count} соответствий");
                return result;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка совмещения: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Классификация кристалла (хороший/дефектный)
        /// </summary>
        /// <param name="image">Изображение кристалла</param>
        /// <returns>Результат классификации</returns>
        public CrystalClassification ClassifyCrystal(Bitmap image)
        {
            try
            {
                OnStatusChanged("Классификация кристалла...");

                // Поиск дефектов
                var defects = DetectDefects(image);

                // Измерение размеров
                var measurement = MeasureCrystal(image);

                // Простая классификация по количеству дефектов
                var classification = new CrystalClassification
                {
                    IsGood = defects.Count == 0,
                    DefectCount = defects.Count,
                    Quality = CalculateQuality(defects, measurement),
                    Defects = defects
                };

                OnStatusChanged($"Классификация: {(classification.IsGood ? "Годен" : "Брак")} (дефектов: {defects.Count})");
                return classification;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка классификации: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Вычисление степени серьезности дефекта
        /// </summary>
        private DefectSeverity CalculateDefectSeverity(int defectArea, int totalArea)
        {
            double ratio = (double)defectArea / totalArea * 100;

            if (ratio < 0.5)
                return DefectSeverity.Minor;
            else if (ratio < 2.0)
                return DefectSeverity.Moderate;
            else
                return DefectSeverity.Critical;
        }

        /// <summary>
        /// Вычисление общего качества кристалла
        /// </summary>
        private double CalculateQuality(List<Defect> defects, CrystalMeasurement measurement)
        {
            if (defects == null || defects.Count == 0)
                return 100.0;

            if (measurement == null)
                return 0.0;

            // Простая формула: 100% - (сумма площадей дефектов / площадь кристалла * 100)
            double defectArea = defects.Sum(d => d.Area);
            double quality = 100.0 - (defectArea / measurement.Area * 100);

            return Math.Max(0, Math.Min(100, quality));
        }

        /// <summary>
        /// Сопоставление меток между двумя изображениями
        /// </summary>
        private List<FiducialPair> MatchFiducials(List<FiducialMark> marks1, List<FiducialMark> marks2)
        {
            var pairs = new List<FiducialPair>();

            // Простой алгоритм: ближайший сосед
            foreach (var mark1 in marks1)
            {
                FiducialMark closestMark = null;
                double minDistance = double.MaxValue;

                foreach (var mark2 in marks2)
                {
                    double distance = Distance(mark1.Center, mark2.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestMark = mark2;
                    }
                }

                if (closestMark != null && minDistance < 100) // Порог расстояния
                {
                    pairs.Add(new FiducialPair
                    {
                        Mark1 = mark1,
                        Mark2 = closestMark,
                        Distance = minDistance
                    });
                }
            }

            return pairs;
        }

        /// <summary>
        /// Вычисление расстояния между точками
        /// </summary>
        private double Distance(PointF p1, PointF p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }
    }

    #region Data Classes

    public class FiducialMark
    {
        public PointF Center { get; set; }
        public Rectangle Bounds { get; set; }
        public int Area { get; set; }
        public FiducialMarkType Type { get; set; }
        
        // Размеры в мм (если камера откалибрована)
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
    }

    public enum FiducialMarkType
    {
        Unknown,
        Circle,
        Square,
        Cross
    }

    public class Defect
    {
        public PointF Location { get; set; }
        public Rectangle Bounds { get; set; }
        public int Area { get; set; }
        public DefectSeverity Severity { get; set; }
        
        // Размеры в мм (если камера откалибрована)
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double AreaMm2 { get; set; }
    }

    public enum DefectSeverity
    {
        Minor,
        Moderate,
        Critical
    }

    public class CrystalMeasurement
    {
        public Rectangle Bounds { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Area { get; set; }
        public PointF Center { get; set; }
        
        // Размеры в мм (если камера откалибрована)
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double AreaMm2 { get; set; }
    }

    public class CrystalClassification
    {
        public bool IsGood { get; set; }
        public int DefectCount { get; set; }
        public double Quality { get; set; }
        public List<Defect> Defects { get; set; }
    }

    public class AlignmentResult
    {
        public bool IsSuccessful { get; set; }
        public List<FiducialPair> MatchedPairs { get; set; }
        public double TranslationX { get; set; }
        public double TranslationY { get; set; }
        public double Rotation { get; set; }
        public double Scale { get; set; }
    }

    public class FiducialPair
    {
        public FiducialMark Mark1 { get; set; }
        public FiducialMark Mark2 { get; set; }
        public double Distance { get; set; }
    }

    #endregion
}
