# 📦 Инструкция по установке NuGet пакетов для камеры

## ⚠️ КРИТИЧНО: Перед компиляцией необходимо установить пакеты!

Система управления камерой требует библиотеки **AForge.NET Framework**.

---

## 🎯 Быстрая установка (Visual Studio)

### Шаг 1: Открыть NuGet Package Manager

1. В **Solution Explorer** найдите проект **CrystalTable**
2. Нажмите **правой кнопкой мыши** на проект
3. Выберите **"Manage NuGet Packages..."**

### Шаг 2: Установить пакеты

Во вкладке **"Browse"** найдите и установите **ПО ОЧЕРЕДИ**:

```
1. AForge                           (версия 2.2.5)
2. AForge.Video                     (версия 2.2.5)
3. AForge.Video.DirectShow          (версия 2.2.5)
4. AForge.Imaging                   (версия 2.2.5)
5. AForge.Math                      (версия 2.2.5)
```

**Важно:** Устанавливайте именно версию **2.2.5** для совместимости с .NET Framework 4.7.2

### Шаг 3: Проверка

После установки проверьте, что в **References** проекта появились:
- AForge
- AForge.Video
- AForge.Video.DirectShow
- AForge.Imaging
- AForge.Math

---

## 🔧 Альтернатива: Package Manager Console

Откройте **Tools → NuGet Package Manager → Package Manager Console** и выполните:

```powershell
Install-Package AForge -Version 2.2.5
Install-Package AForge.Video -Version 2.2.5
Install-Package AForge.Video.DirectShow -Version 2.2.5
Install-Package AForge.Imaging -Version 2.2.5
Install-Package AForge.Math -Version 2.2.5
```

---

## ✅ После установки

1. **Перезапустите Visual Studio** (рекомендуется)
2. **Build → Rebuild Solution**
3. Проверьте отсутствие ошибок компиляции

---

## 🚀 Проверка работоспособности

После успешной компиляции:

1. Запустите приложение
2. Подключите USB-камеру
3. Нажмите кнопку **"📷 Камера"** в тулбаре
4. Превью должно появиться в правом верхнем углу

---

## ❌ Если не работает

### Ошибка: "Не удалось найти тип или имя пространства имен AForge"

**Решение:**
1. Проверьте, что все 5 пакетов установлены
2. Очистите кэш NuGet: **Tools → Options → NuGet Package Manager → Clear All NuGet Cache(s)**
3. Восстановите пакеты: **Tools → NuGet Package Manager → Restore NuGet Packages**
4. Перезапустите Visual Studio

### Ошибка при установке пакетов

**Решение:**
1. Убедитесь, что у вас есть интернет-соединение
2. Проверьте источники пакетов: **Tools → Options → NuGet Package Manager → Package Sources**
3. Должен быть включен источник **nuget.org**: `https://api.nuget.org/v3/index.json`

### Камера не обнаруживается

**Решение:**
1. Проверьте подключение камеры в Диспетчере устройств
2. Установите драйверы камеры
3. Попробуйте другой USB-порт

---

## 📝 Примечания

- Пакеты устанавливаются **один раз** для проекта
- При переносе проекта на другой компьютер пакеты восстановятся автоматически
- Размер пакетов: ~5 МБ
- Лицензия AForge: **LGPL v3** (бесплатно для коммерческого использования)

---

## 🆘 Поддержка

Если возникли проблемы:
1. Проверьте версию .NET Framework (должна быть 4.7.2)
2. Убедитесь, что Visual Studio 2017 или новее
3. Создайте issue: https://github.com/Kenny-sw/CrystalWafer/issues

---

**ГОТОВО!** После установки пакетов система камеры будет полностью функциональна 🎉
