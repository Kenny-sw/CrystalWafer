using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Менеджер профилей движения: загрузка, сохранение, применение
    /// </summary>
    public class MotionProfileManager
    {
        // ✅ ИСПРАВЛЕНО: Thread-safe lazy singleton
        private static readonly Lazy<MotionProfileManager> _lazy = 
            new Lazy<MotionProfileManager>(() => new MotionProfileManager());
        public static MotionProfileManager Instance => _lazy.Value;

        private readonly string _profilesDirectory;
        private readonly string _profilesFilePath;
        private readonly string _activeProfilePath;
        private readonly string _useProfileSettingPath;

        private List<MotionProfile> _profiles;
        private MotionProfile _activeProfile;
        
        /// <summary>
        /// Флаг использования профиля. Если false - Arduino работает со штатными настройками 30/40/30
        /// </summary>
        private bool _useProfile = true;

        public event EventHandler<MotionProfile> ActiveProfileChanged;
        public event EventHandler ProfilesListChanged;
        public event EventHandler<bool> UseProfileChanged;

        private MotionProfileManager()
        {
            var appDataPath = Path.Combine(
     Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
             "CrystalTable");

      _profilesDirectory = Path.Combine(appDataPath, "Profiles");
          _profilesFilePath = Path.Combine(_profilesDirectory, "profiles.xml");
 _activeProfilePath = Path.Combine(appDataPath, "Config", "active_profile.txt");
            _useProfileSettingPath = Path.Combine(appDataPath, "Config", "use_profile.txt");

      Directory.CreateDirectory(_profilesDirectory);
    Directory.CreateDirectory(Path.GetDirectoryName(_activeProfilePath));

    LoadProfiles();
            LoadUseProfileSetting();
    }

        // ===== Свойства =====

        /// <summary>
        /// Использовать профиль движения. Если false - Arduino работает со штатными настройками 30/40/30
        /// </summary>
        public bool UseProfile
        {
            get => _useProfile;
            set
            {
                if (_useProfile != value)
                {
                    _useProfile = value;
                    SaveUseProfileSetting();
                    UseProfileChanged?.Invoke(this, _useProfile);
                    AppLogger.Info($"Режим профилей: {(_useProfile ? "ВКЛЮЧЁН" : "ВЫКЛЮЧЁН (штатный 30/40/30)")}");
                }
            }
        }

        public MotionProfile ActiveProfile
        {
         get => _activeProfile;
        private set
    {
       if (_activeProfile != value)
      {
              _activeProfile = value;
  SaveActiveProfileId();
     ActiveProfileChanged?.Invoke(this, _activeProfile);
           AppLogger.Info($"Активный профиль изменён: {_activeProfile?.Name}");
             }
            }
        }

   public IReadOnlyList<MotionProfile> Profiles => _profiles?.AsReadOnly();

        // ===== Загрузка/Сохранение =====

        private void LoadProfiles()
        {
    try
            {
        if (File.Exists(_profilesFilePath))
   {
       var serializer = new XmlSerializer(typeof(List<MotionProfile>));
         using (var reader = new StreamReader(_profilesFilePath, Encoding.UTF8))
 {
        _profiles = (List<MotionProfile>)serializer.Deserialize(reader);
       AppLogger.Info($"Загружено {_profiles.Count} профилей из {_profilesFilePath}");
     }
    }
    else
     {
         // Первый запуск - создаём предустановленные профили
          _profiles = MotionProfile.GetPresetProfiles();
        SaveProfiles();
          AppLogger.Info("Созданы предустановленные профили");
      }

   // Загружаем активный профиль
      LoadActiveProfile();
          }
            catch (Exception ex)
     {
            AppLogger.Error("Ошибка загрузки профилей", ex);
     _profiles = MotionProfile.GetPresetProfiles();
       ActiveProfile = _profiles.FirstOrDefault(p => p.IsDefault) ?? _profiles[0];
       }
        }

        private void SaveProfiles()
        {
            try
        {
          var serializer = new XmlSerializer(typeof(List<MotionProfile>));
                using (var writer = new StreamWriter(_profilesFilePath, false, Encoding.UTF8))
     {
             serializer.Serialize(writer, _profiles);
       }
    AppLogger.Debug($"Профили сохранены: {_profiles.Count} шт.");
            }
            catch (Exception ex)
            {
    AppLogger.Error("Ошибка сохранения профилей", ex);
         }
   }

        private void LoadActiveProfile()
  {
            try
   {
       if (File.Exists(_activeProfilePath))
        {
    var idStr = File.ReadAllText(_activeProfilePath).Trim();
          if (Guid.TryParse(idStr, out Guid id))
      {
     _activeProfile = _profiles.FirstOrDefault(p => p.Id == id);
      }
       }

            // Если не загрузили - используем профиль по умолчанию
           _activeProfile ??= _profiles.FirstOrDefault(p => p.IsDefault) ?? _profiles[0];
                
  AppLogger.Info($"Активный профиль: {_activeProfile.Name}");
         }
         catch (Exception ex)
       {
     AppLogger.Warning("Ошибка загрузки активного профиля", ex);
    _activeProfile = _profiles.FirstOrDefault(p => p.IsDefault) ?? _profiles[0];
            }
 }

      private void SaveActiveProfileId()
        {
   try
            {
      if (_activeProfile != null)
            {
    File.WriteAllText(_activeProfilePath, _activeProfile.Id.ToString());
       }
            }
            catch (Exception ex)
    {
       AppLogger.Warning("Ошибка сохранения ID активного профиля", ex);
   }
        }

        private void LoadUseProfileSetting()
        {
            try
            {
                if (File.Exists(_useProfileSettingPath))
                {
                    var value = File.ReadAllText(_useProfileSettingPath).Trim();
                    _useProfile = !value.Equals("false", StringComparison.OrdinalIgnoreCase) 
                               && !value.Equals("0", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    _useProfile = true; // По умолчанию используем профили
                }
                AppLogger.Info($"Режим профилей: {(_useProfile ? "ВКЛЮЧЁН" : "ВЫКЛЮЧЁН (штатный 30/40/30)")}");
            }
            catch (Exception ex)
            {
                AppLogger.Warning("Ошибка загрузки настройки UseProfile", ex);
                _useProfile = true;
            }
        }

        private void SaveUseProfileSetting()
        {
            try
            {
                File.WriteAllText(_useProfileSettingPath, _useProfile ? "true" : "false");
            }
            catch (Exception ex)
            {
                AppLogger.Warning("Ошибка сохранения настройки UseProfile", ex);
            }
        }

        /// <summary>
        /// Получить профиль для отправки в Arduino.
        /// Если UseProfile=false, возвращает штатный профиль 30/40/30
        /// </summary>
        public MotionProfile GetEffectiveProfile()
        {
            if (!UseProfile)
            {
                // Штатный профиль 30/40/30 как в старой прошивке StepByStep1.5.ino
                return new MotionProfile
                {
                    Name = "Штатный (30/40/30)",
                    Description = "Штатный профиль из прошивки Arduino",
                    MinDelayUs = 200,
                    MaxDelayUs = 800,
                    AccelPercent = 30,
                    CruisePercent = 40,
                    DecelPercent = 30,
                    Type = ProfileType.Trapezoid
                };
            }
            return ActiveProfile;
        }

        // ===== Управление профилями =====

 public void SetActiveProfile(MotionProfile profile)
        {
    if (profile == null)
          throw new ArgumentNullException(nameof(profile));

            if (!_profiles.Contains(profile))
                throw new ArgumentException("Профиль не найден в списке");

            ActiveProfile = profile;
  }

        public void SetActiveProfile(Guid profileId)
        {
     var profile = _profiles.FirstOrDefault(p => p.Id == profileId);
   if (profile == null)
       throw new ArgumentException($"Профиль с ID {profileId} не найден");

          SetActiveProfile(profile);
  }

        public void SetActiveProfile(string profileName)
        {
var profile = _profiles.FirstOrDefault(p => 
         p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
       
            if (profile == null)
        throw new ArgumentException($"Профиль '{profileName}' не найден");

            SetActiveProfile(profile);
      }

        public void AddProfile(MotionProfile profile)
        {
     if (profile == null)
          throw new ArgumentNullException(nameof(profile));

     // Проверка валидности
       if (!profile.IsValid())
    {
          var errors = string.Join("\n", profile.Validate());
    throw new InvalidOperationException($"Профиль невалиден:\n{errors}");
    }

         // Проверка уникальности имени
  if (_profiles.Any(p => p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase)))
            {
     throw new InvalidOperationException($"Профиль с именем '{profile.Name}' уже существует");
       }

     _profiles.Add(profile);
            SaveProfiles();
      ProfilesListChanged?.Invoke(this, EventArgs.Empty);
            AppLogger.Info($"Профиль добавлен: {profile.Name}");
}

      public void UpdateProfile(MotionProfile profile)
        {
        if (profile == null)
     throw new ArgumentNullException(nameof(profile));

            var existing = _profiles.FirstOrDefault(p => p.Id == profile.Id);
        if (existing == null)
           throw new ArgumentException("Профиль не найден в списке");

            // Проверка валидности
         if (!profile.IsValid())
            {
       var errors = string.Join("\n", profile.Validate());
      throw new InvalidOperationException($"Профиль невалиден:\n{errors}");
  }

            // Копируем данные
          int index = _profiles.IndexOf(existing);
    _profiles[index] = profile;

  SaveProfiles();
    
  // Если это активный профиль - обновляем ссылку
            if (_activeProfile?.Id == profile.Id)
            {
                _activeProfile = profile;
           ActiveProfileChanged?.Invoke(this, _activeProfile);
       }

     ProfilesListChanged?.Invoke(this, EventArgs.Empty);
   AppLogger.Info($"Профиль обновлён: {profile.Name}");
        }

      public void DeleteProfile(Guid profileId)
        {
   var profile = _profiles.FirstOrDefault(p => p.Id == profileId);
 if (profile == null)
    throw new ArgumentException("Профиль не найден");

        if (profile.IsDefault)
        throw new InvalidOperationException("Нельзя удалить профиль по умолчанию");

        if (_activeProfile?.Id == profileId)
            {
   // Переключаемся на профиль по умолчанию
          var defaultProfile = _profiles.FirstOrDefault(p => p.IsDefault) 
      ?? _profiles.FirstOrDefault(p => p.Id != profileId);

       if (defaultProfile != null)
                {
  SetActiveProfile(defaultProfile);
          }
   }

   _profiles.Remove(profile);
            SaveProfiles();
            ProfilesListChanged?.Invoke(this, EventArgs.Empty);
      AppLogger.Info($"Профиль удалён: {profile.Name}");
        }

        // ===== Экспорт/Импорт =====

        public void ExportProfile(MotionProfile profile, string filePath)
        {
            if (profile == null)
         throw new ArgumentNullException(nameof(profile));

 try
        {
var serializer = new XmlSerializer(typeof(MotionProfile));
       using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
          serializer.Serialize(writer, profile);
     }
        AppLogger.Info($"Профиль экспортирован: {filePath}");
     }
     catch (Exception ex)
       {
     AppLogger.Error($"Ошибка экспорта профиля в {filePath}", ex);
          throw;
            }
        }

        public MotionProfile ImportProfile(string filePath)
     {
            try
         {
    var serializer = new XmlSerializer(typeof(MotionProfile));
using (var reader = new StreamReader(filePath, Encoding.UTF8))
         {
               var profile = (MotionProfile)serializer.Deserialize(reader);
 
        // Генерируем новый ID
     profile.Id = Guid.NewGuid();
              profile.IsDefault = false;
           
        // Проверяем уникальность имени
        if (_profiles.Any(p => p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase)))
       {
   profile.Name += " (импорт)";
           }

       AddProfile(profile);
    AppLogger.Info($"Профиль импортирован: {filePath}");
   return profile;
     }
        }
       catch (Exception ex)
            {
   AppLogger.Error($"Ошибка импорта профиля из {filePath}", ex);
    throw;
  }
        }

     // ===== Сброс к умолчаниям =====

        public void ResetToDefaults()
        {
            _profiles = MotionProfile.GetPresetProfiles();
            SaveProfiles();
    ActiveProfile = _profiles.FirstOrDefault(p => p.IsDefault) ?? _profiles[0];
            ProfilesListChanged?.Invoke(this, EventArgs.Empty);
            AppLogger.Info("Профили сброшены к умолчаниям");
        }

        // ===== Применение профиля в Arduino =====

     public async System.Threading.Tasks.Task<bool> ApplyProfileToArduino(
   SerialPortController serialController, 
            MotionProfile profile = null)
        {
            // Если профили отключены - не отправляем ничего в Arduino
            if (!UseProfile && profile == null)
            {
                AppLogger.Info("Профили отключены - Arduino использует штатные настройки 30/40/30");
                return true;
            }

            profile ??= GetEffectiveProfile();
   
            if (profile == null)
            {
           AppLogger.Warning("Нет активного профиля для отправки");
     return false;
 }

            if (serialController == null)
            {
         AppLogger.Warning("SerialPortController == null");
       return false;
      }

       try
{
    AppLogger.Info($"Отправка профиля в Arduino: {profile.Name}");
         
  // Упаковываем профиль в uint32 для отправки
        // Формат: [minDelay:16][maxDelay:16] в первом пакете
   ushort minDelay = profile.MinDelayUs;
          ushort maxDelay = profile.MaxDelayUs;
       uint data1 = (uint)minDelay | ((uint)maxDelay << 16);

    bool success = await serialController.SendCommandAsync(
Protocol.Commands.SetProfile, 
             data1);

     if (success)
    {
   AppLogger.Info($"Профиль успешно применён: {profile.Name}");
            return true;
     }
             else
    {
             AppLogger.Warning("Arduino не подтвердил установку профиля");
         return false;
         }
     }
        catch (Exception ex)
   {
      AppLogger.Error("Ошибка применения профиля", ex);
     return false;
  }
        }

    // ===== Получение профиля от Arduino =====

        /// <summary>
        /// Парсит строку ответа "PROFILE:minDelay,maxDelay,accel,cruise,decel" от Arduino
        /// </summary>
        public static MotionProfile ParseProfileFromArduino(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return null;

            // Формат: "PROFILE:200,800,30,40,30"
            const string prefix = "PROFILE:";
            if (!response.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                string data = response.Substring(prefix.Length);
                string[] parts = data.Split(',');

                if (parts.Length < 2)
                {
                    AppLogger.Warning($"Недостаточно данных в ответе профиля: {response}");
                    return null;
                }

                var profile = new MotionProfile
                {
                    Name = "Arduino Profile",
                    Description = "Профиль, полученный от Arduino"
                };

                // Парсим minDelay и maxDelay (обязательные)
                if (ushort.TryParse(parts[0].Trim(), out ushort minDelay))
                    profile.MinDelayUs = minDelay;
                else
                    return null;

                if (ushort.TryParse(parts[1].Trim(), out ushort maxDelay))
                    profile.MaxDelayUs = maxDelay;
                else
                    return null;

                // Парсим проценты (опциональные, если есть)
                if (parts.Length >= 5)
                {
                    if (byte.TryParse(parts[2].Trim(), out byte accel))
                        profile.AccelPercent = accel;
                    if (byte.TryParse(parts[3].Trim(), out byte cruise))
                        profile.CruisePercent = cruise;
                    if (byte.TryParse(parts[4].Trim(), out byte decel))
                        profile.DecelPercent = decel;
                }

                AppLogger.Info($"Профиль от Arduino: minDelay={profile.MinDelayUs}, maxDelay={profile.MaxDelayUs}");
                return profile;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Ошибка парсинга профиля от Arduino: {response}", ex);
                return null;
            }
        }

        public async System.Threading.Tasks.Task<MotionProfile> GetProfileFromArduino(
            SerialPortController serialController)
        {
            if (serialController == null)
            {
                AppLogger.Warning("SerialPortController == null");
                return null;
            }

            try
            {
                AppLogger.Debug("Запрос текущего профиля от Arduino...");
      
                // Отправляем команду GetProfile
                bool success = await serialController.SendCommandAsync(
                    Protocol.Commands.GetProfile, 
                    0);

                if (!success)
                {
                    AppLogger.Warning("Не удалось отправить команду GetProfile");
                    return null;
                }

                // Ответ будет обработан через событие ProfileDataReceived
                // Возвращаем null - профиль будет доступен через событие
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка получения профиля от Arduino", ex);
                return null;
            }
        }
    }
}
