using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    /// <summary>
    /// Тип профиля движения
  /// </summary>
    public enum ProfileType : byte
 {
        /// <summary>Трапециевидный (ACCEL → RUN → DECEL)</summary>
        Trapezoid = 0,
   
        /// <summary>Треугольный (ACCEL → DECEL, без RUN)</summary>
Triangle = 1,
        
  /// <summary>S-образный (плавные углы)</summary>
        SCurve = 2,
        
  /// <summary>Автоматический выбор на основе расстояния</summary>
    Auto = 3
  }

    /// <summary>
    /// Профиль движения шагового двигателя
    /// </summary>
    [Serializable]
    public class MotionProfile
    {
        // ===== Идентификация =====
        
        [XmlElement]
        public Guid Id { get; set; } = Guid.NewGuid();
  
        [XmlElement]
   public string Name { get; set; } = "Новый профиль";
        
      [XmlElement]
        public string Description { get; set; } = "";
      
        [XmlElement]
        public bool IsDefault { get; set; }
        
    [XmlElement]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ===== Тип профиля =====
        
 [XmlElement]
        public ProfileType Type { get; set; } = ProfileType.Trapezoid;
        
   /// <summary>
      /// Порог переключения для Auto профиля (шаги)
        /// Если расстояние < threshold → Triangle, иначе → Trapezoid
     /// </summary>
        [XmlElement]
 public uint AutoThreshold { get; set; } = 20;

// ===== Параметры скорости =====
        
        /// <summary>Минимальная пауза между шагами (крейсерская скорость), мкс</summary>
        [XmlElement]
        public ushort MinDelayUs { get; set; } = 200;
        
        /// <summary>Максимальная пауза между шагами (старт/финиш), мкс</summary>
        [XmlElement]
        public ushort MaxDelayUs { get; set; } = 800;
        
        /// <summary>Ширина импульса STEP, мкс</summary>
        [XmlElement]
        public byte PulseWidthUs { get; set; } = 5;

        // ===== Распределение фаз (для трапеции) =====
        
        /// <summary>Процент шагов на разгон (10-50%)</summary>
        [XmlElement]
        public byte AccelPercent { get; set; } = 30;
        
        /// <summary>Процент шагов на крейсерскую скорость (0-80%)</summary>
        [XmlElement]
        public byte CruisePercent { get; set; } = 40;
        
      /// <summary>Процент шагов на торможение (10-50%)</summary>
        [XmlElement]
      public byte DecelPercent { get; set; } = 30;

        // ===== Дополнительные параметры =====
        
        /// <summary>Синхронизация движения осей X/Y</summary>
        [XmlElement]
    public bool AxisSync { get; set; } = true;
        
        /// <summary>Запас безопасности (множитель для ускорения), 0.5 - 1.0</summary>
        [XmlIgnore]
        public float SafetyMargin { get; set; } = 0.8f;

    // ===== Вычисляемые свойства =====
        
   /// <summary>Максимальная скорость (шагов/сек)</summary>
     [XmlIgnore]
        public float MaxSpeedStepsPerSec =>
            MinDelayUs > 0 ? 1_000_000f / MinDelayUs : 0f;
     
        /// <summary>Минимальная скорость (шагов/сек)</summary>
        [XmlIgnore]
        public float MinSpeedStepsPerSec =>
      MaxDelayUs > 0 ? 1_000_000f / MaxDelayUs : 0f;
        
        /// <summary>Максимальное ускорение (шагов/сек²)</summary>
        [XmlIgnore]
        public float MaxAccelerationStepsPerSec2
        {
            get
     {
     if (MinDelayUs >= MaxDelayUs || AccelPercent == 0)
       return 0f;
       
      float deltaSpeed = MaxSpeedStepsPerSec - MinSpeedStepsPerSec;
      
   // Для 1000 шагов, 30% разгон = 300 шагов
      // Время = сумма задержек ≈ (MaxDelay + MinDelay) / 2 * steps / 1_000_000
                float avgDelay = (MaxDelayUs + MinDelayUs) / 2f;
  float timePerStep = avgDelay / 1_000_000f;
         float accelSteps = 100f; // для примера
  float accelTime = accelSteps * timePerStep;
          
        return accelTime > 0 ? deltaSpeed / accelTime : 0f;
     }
        }

        // ===== Валидация =====
   
        /// <summary>Проверка валидности профиля</summary>
        public bool IsValid() => Validate().Count == 0;
    
        /// <summary>Получить список ошибок валидации</summary>
      public List<string> Validate()
        {
   var errors = new List<string>();

  // 1. Скорости
     if (MinDelayUs < 100)
errors.Add("Минимальная пауза должна быть ≥ 100 мкс (макс. скорость ≈ 10000 шаг/с)");
            
     if (MaxDelayUs > 5000)
                errors.Add("Максимальная пауза должна быть ≤ 5000 мкс (мин. скорость ≈ 200 шаг/с)");
            
            if (MinDelayUs >= MaxDelayUs)
                errors.Add("Минимальная пауза должна быть меньше максимальной");

     // 2. Проценты фаз
    int totalPercent = AccelPercent + CruisePercent + DecelPercent;
            if (totalPercent != 100)
                errors.Add($"Сумма процентов должна быть 100% (сейчас: {totalPercent}%)");
   
            if (AccelPercent < 10)
    errors.Add("Разгон должен быть ≥ 10%");
       
 if (DecelPercent < 10)
           errors.Add("Торможение должно быть ≥ 10%");
   
            if (Type == ProfileType.Triangle && CruisePercent != 0)
       errors.Add("Треугольный профиль: крейсерская фаза должна быть 0%");
  
            // ✅ ИСПРАВЛЕНО: Trapezoid может работать и без крейсерской фазы (будет похож на Triangle)
            // Убрана жёсткая проверка, оставлено предупреждение в UI

// 3. Тип профиля
         if (Type == ProfileType.Auto && AutoThreshold == 0)
        errors.Add("Автоматический профиль: порог переключения должен быть > 0");

  // 4. Физические ограничения
        if (PulseWidthUs < 3)
                errors.Add("Ширина импульса должна быть ≥ 3 мкс (требование драйвера)");
 
if (PulseWidthUs > 20)
        errors.Add("Ширина импульса должна быть ≤ 20 мкс");

         return errors;
        }

     // ===== Статистика профиля =====
 
        /// <summary>Рассчитать статистику для заданного расстояния</summary>
        public ProfileStatistics CalculateStats(uint totalSteps)
        {
            if (totalSteps == 0)
      return new ProfileStatistics();

            ProfileType effectiveType = Type;
            
            // Для Auto - выбираем тип
    if (Type == ProfileType.Auto)
   {
                effectiveType = totalSteps < AutoThreshold 
         ? ProfileType.Triangle 
        : ProfileType.Trapezoid;
  }

            uint accelSteps, cruiseSteps, decelSteps;
         
            if (effectiveType == ProfileType.Triangle)
    {
  // Треугольный: 50% разгон, 50% торможение
        accelSteps = totalSteps / 2;
     decelSteps = totalSteps - accelSteps;
                cruiseSteps = 0;
            }
          else
     {
         // Трапециевидный: используем проценты
   accelSteps = (totalSteps * AccelPercent) / 100;
       cruiseSteps = (totalSteps * CruisePercent) / 100;
   decelSteps = totalSteps - accelSteps - cruiseSteps;
  }

         // Расчёт времени (приблизительный)
         float avgAccelDelay = (MaxDelayUs + MinDelayUs) / 2f;
            float timeAccel = accelSteps * avgAccelDelay / 1_000_000f;
     float timeCruise = cruiseSteps * MinDelayUs / 1_000_000f;
        float timeDecel = decelSteps * avgAccelDelay / 1_000_000f;
  float totalTime = timeAccel + timeCruise + timeDecel;

            return new ProfileStatistics
          {
     EffectiveType = effectiveType,
      TotalSteps = totalSteps,
      AccelSteps = accelSteps,
   CruiseSteps = cruiseSteps,
DecelSteps = decelSteps,
                TotalTimeSec = totalTime,
          MaxSpeedStepsPerSec = MaxSpeedStepsPerSec,
                MaxAcceleration = MaxAccelerationStepsPerSec2
            };
        }

        // ===== Упаковка в бинарный пакет для Arduino =====
        
        /// <summary>Упаковать профиль в байтовый массив для отправки в Arduino</summary>
        public byte[] ToBinary()
   {
            // Формат: [minDelay:2][maxDelay:2][accel:1][cruise:1][decel:1][type:1]
      var data = new byte[8];
            
            data[0] = (byte)(MinDelayUs & 0xFF);
       data[1] = (byte)((MinDelayUs >> 8) & 0xFF);
            data[2] = (byte)(MaxDelayUs & 0xFF);
   data[3] = (byte)((MaxDelayUs >> 8) & 0xFF);
            data[4] = AccelPercent;
         data[5] = CruisePercent;
            data[6] = DecelPercent;
            data[7] = (byte)Type;
            
            return data;
        }

        /// <summary>Распаковать профиль из байтового массива (от Arduino)</summary>
     public static MotionProfile FromBinary(byte[] data)
        {
  if (data == null || data.Length < 8)
    throw new ArgumentException("Invalid profile data");

     return new MotionProfile
         {
    MinDelayUs = (ushort)(data[0] | (data[1] << 8)),
    MaxDelayUs = (ushort)(data[2] | (data[3] << 8)),
  AccelPercent = data[4],
       CruisePercent = data[5],
        DecelPercent = data[6],
      Type = (ProfileType)data[7]
        };
        }

        // ===== Предустановленные профили =====
        
   public static MotionProfile GetDefaultProfile() =>
            new MotionProfile
 {
                Name = "⭐ Стандартный",
              Description = "Универсальный профиль для большинства задач",
          IsDefault = true,
         MinDelayUs = 200,
                MaxDelayUs = 800,
    AccelPercent = 30,
  CruisePercent = 40,
    DecelPercent = 30,
        Type = ProfileType.Trapezoid
            };

        public static MotionProfile GetFastProfile() =>
            new MotionProfile
            {
              Name = "⚡ Быстрый",
              Description = "Для длинных расстояний и малого веса",
        MinDelayUs = 150,
   MaxDelayUs = 600,
   AccelPercent = 25,
     CruisePercent = 50,
                DecelPercent = 25,
             Type = ProfileType.Trapezoid
            };

        public static MotionProfile GetSmoothProfile() =>
    new MotionProfile
      {
         Name = "🌊 Плавный",
  Description = "Для точной работы, минимум вибраций",
                MinDelayUs = 300,
      MaxDelayUs = 1200,
    AccelPercent = 35,
 CruisePercent = 30,
              DecelPercent = 35,
        Type = ProfileType.Trapezoid
         };

        public static MotionProfile GetPreciseProfile() =>
            new MotionProfile
       {
    Name = "🎯 Точный",
       Description = "Высокая точность позиционирования",
  MinDelayUs = 400,
          MaxDelayUs = 1500,
 AccelPercent = 40,
    CruisePercent = 20,
     DecelPercent = 40,
                Type = ProfileType.Trapezoid
      };

        public static MotionProfile GetHeavyLoadProfile() =>
   new MotionProfile
    {
     Name = "🏋️ Тяжёлый груз",
     Description = "Для большой инерции нагрузки",
    MinDelayUs = 500,
              MaxDelayUs = 2000,
         AccelPercent = 45,
                CruisePercent = 10,
           DecelPercent = 45,
     Type = ProfileType.Trapezoid
         };

      public static MotionProfile GetMicroStepProfile() =>
            new MotionProfile
     {
          Name = "🔬 Микрошаг",
    Description = "Для коротких расстояний",
            MinDelayUs = 800,
            MaxDelayUs = 3000,
        AccelPercent = 40,
        CruisePercent = 20,
        DecelPercent = 40,
    Type = ProfileType.Triangle
        };

        /// <summary>Получить все предустановленные профили</summary>
     public static List<MotionProfile> GetPresetProfiles() =>
       new List<MotionProfile>
            {
         GetDefaultProfile(),
 GetFastProfile(),
           GetSmoothProfile(),
 GetPreciseProfile(),
     GetHeavyLoadProfile(),
          GetMicroStepProfile()
   };

        // ===== Клонирование =====
   
        public MotionProfile Clone() =>
         new MotionProfile
            {
    Id = Guid.NewGuid(), // Новый ID для клона
    Name = Name + " (копия)",
    Description = Description,
        IsDefault = false,
      Type = Type,
     AutoThreshold = AutoThreshold,
     MinDelayUs = MinDelayUs,
  MaxDelayUs = MaxDelayUs,
    PulseWidthUs = PulseWidthUs,
           AccelPercent = AccelPercent,
   CruisePercent = CruisePercent,
  DecelPercent = DecelPercent,
    AxisSync = AxisSync,
                SafetyMargin = SafetyMargin
            };

      public override string ToString() => Name;
    }

    /// <summary>
    /// Статистика профиля для заданного расстояния
    /// </summary>
    public class ProfileStatistics
    {
    public ProfileType EffectiveType { get; set; }
        public uint TotalSteps { get; set; }
     public uint AccelSteps { get; set; }
        public uint CruiseSteps { get; set; }
        public uint DecelSteps { get; set; }
        public float TotalTimeSec { get; set; }
        public float MaxSpeedStepsPerSec { get; set; }
        public float MaxAcceleration { get; set; }

 public string GetSummary() =>
   $"Время: {TotalTimeSec:F2} сек | " +
   $"Макс. скорость: {MaxSpeedStepsPerSec:F0} шаг/с | " +
          $"Ускорение: {MaxAcceleration:F0} шаг/с²";
    }
}
