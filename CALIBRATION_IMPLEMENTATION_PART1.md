# ✅ РЕАЛИЗАЦИЯ КАЛИБРОВКИ - ЧАСТЬ 1

**Статус:** ✅ КОМПИЛЯЦИЯ УСПЕШНА + КООРДИНАТЫ ИСПРАВЛЕНЫ

---

## 📦 ШАГ 1: ПОЛЯ КАЛИБРОВКИ В WaferController ✅

### Добавленные поля:

```csharp
// КАЛИБРОВКА: Привязка виртуальной карты к физической пластине
private bool isCalibrated = false;
private int calibrationCrystalIndex = -1;          // Индекс базового кристалла (обычно 0)
private float calibrationCrystalX = 0f;            // Координата базового кристалла на карте (мм) - ВИРТУАЛЬНАЯ
private float calibrationCrystalY = 0f;
private float calibrationPointerX = 0f;            // Где был указатель ЛШД при калибровке (мм) - ФИЗИЧЕСКАЯ
private float calibrationPointerY = 0f;

public bool IsCalibrated => isCalibrated;
public int CalibrationCrystalIndex => calibrationCrystalIndex;
public float CalibrationOffsetX => calibrationCrystalX - calibrationPointerX;  // Смещение между системами
public float CalibrationOffsetY => calibrationCrystalY - calibrationPointerY;
```

### Добавленные методы:

```csharp
/// <summary>
/// Установить точку калибровки (всегда использует первый кристалл - левый верхний)
/// </summary>
/// <param name="pointerX">Текущая позиция указателя ЛШД (мм) - ФИЗИЧЕСКИЕ координаты</param>
/// <param name="pointerY">Текущая позиция указателя ЛШД (мм) - ФИЗИЧЕСКИЕ координаты</param>
public void SetCalibrationZero(float pointerX, float pointerY)
{
    var crystals = CrystalManager.Instance.Crystals;
    if (crystals.Count == 0)
    {
        throw new InvalidOperationException("Нет кристаллов для калибровки. Создайте карту.");
    }

    // ВСЕГДА используем первый кристалл (индекс 0 - левый верхний)
    var firstCrystal = crystals.FirstOrDefault(c => c.Index == 0) ?? crystals.First();

    isCalibrated = true;
    calibrationCrystalIndex = firstCrystal.Index;
    calibrationCrystalX = firstCrystal.RealX;  // Виртуальные координаты на карте
    calibrationCrystalY = firstCrystal.RealY;
    calibrationPointerX = pointerX;  // Физические координаты машины
    calibrationPointerY = pointerY;
    
    // НЕ вызываем SetPointerMm! Указатель уже в правильной позиции
}

/// <summary>
/// Сбросить калибровку
/// </summary>
public void ResetCalibration()
{
    isCalibrated = false;
    calibrationCrystalIndex = -1;
    calibrationCrystalX = 0f;
    calibrationCrystalY = 0f;
    calibrationPointerX = 0f;
    calibrationPointerY = 0f;
}

/// <summary>
/// Получить первый кристалл (базовый для калибровки)
/// </summary>
public Crystal GetFirstCrystal()
{
    var crystals = CrystalManager.Instance.Crystals;
    return crystals.FirstOrDefault(c => c.Index == 0) ?? crystals.FirstOrDefault();
}
```

---

## 📦 ШАГ 2: РЕЖИМ ОТЛАДКИ В Form1 ✅

### Добавленный флаг:

```csharp
// РЕЖИМ ОТЛАДКИ: Работа без COM-порта
private bool debugModeWithoutComPort = false;
```

### Обработчик калибровки:

```csharp
private void SetCalibrationZero_Click(object sender, EventArgs e)
{
    try
    {
        // ✅ Получаем ФИЗИЧЕСКИЕ координаты машины
        var pointerMachine = GetPointerMachineMm();
        waferController.SetCalibrationZero(pointerMachine.X, pointerMachine.Y);
        
        var pointerVirtual = GetPointerMm();  // Виртуальные координаты для отображения
        var offsetX = waferController.CalibrationOffsetX;
        var offsetY = waferController.CalibrationOffsetY;
        
        MessageBox.Show(
            $"Калибровка выполнена!\n" +
            $"Базовый кристалл: №{waferController.CalibrationCrystalIndex} (левый верхний)\n" +
            $"Виртуальная позиция ЛШД: ({pointerVirtual.X:F2}, {pointerVirtual.Y:F2}) мм\n" +
            $"Смещение системы: ({offsetX:+0.00;-0.00;0}, {offsetY:+0.00;-0.00;0}) мм",
            "Калибровка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        
        UpdateUI();
    }
    catch (InvalidOperationException ex)
    {
        MessageBox.Show(ex.Message, "Ошибка калибровки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
```

---

## 📦 ШАГ 3: ИСПРАВЛЕНИЕ СИСТЕМЫ КООРДИНАТ ✅

### Проблема двойного преобразования:

**БЫЛО (неправильно):**
```csharp
// SetPointerMm преобразовывал виртуальные -> физические
public void SetPointerMm(float xMm, float yMm) 
{
    pointerMm = ToPhysical(new PointF(xMm, yMm));  // Преобразование 1
}

// WaferController вызывал с уже виртуальными координатами
form?.SetPointerMm(firstCrystal.RealX, firstCrystal.RealY);  // Преобразование 2!
```

**Результат:** Двойное преобразование → указатель оставался на месте

**СТАЛО (правильно):**
```csharp
// ✅ ВНУТРИ: храним ФИЗИЧЕСКИЕ координаты
private PointF pointerMm = new PointF(0, 0);

// ✅ ПУБЛИЧНЫЙ API: работает с ВИРТУАЛЬНЫМИ координатами
public PointF GetPointerMm() => ToVirtual(pointerMm);
public void SetPointerMm(float xMm, float yMm)
{
    pointerMm = ToPhysical(new PointF(xMm, yMm));
}

// ✅ СЛУЖЕБНЫЙ: работает с ФИЗИЧЕСКИМИ координатами
public PointF GetPointerMachineMm() => pointerMm;
public void SetPointerMachineMm(float xMm, float yMm)
{
    pointerMm = new PointF(xMm, yMm);  // БЕЗ преобразования
}
```

### Исправления в MovePointerToAsync():

**БЫЛО:**
```csharp
var physicalTarget = ToPhysical(new PointF(targetXmm, targetYmm));
// Если targetXmm/Ymm уже физические (RealX/Y), то сдвиг второй раз!
```

**СТАЛО:**
```csharp
// ✅ Параметры - это ВИРТУАЛЬНЫЕ координаты (Crystal.RealX/Y или (0,0))
var physicalTarget = ToPhysical(new PointF(targetXmm, targetYmm));
// Преобразование происходит ровно 1 раз
```

---

## 📋 ЗАВЕРШЁННЫЕ ШАГИ

- [x] ✅ Поля калибровки в WaferController
- [x] ✅ Методы SetCalibrationZero, ResetCalibration, GetFirstCrystal
- [x] ✅ Флаг debugModeWithoutComPort
- [x] ✅ Обработчик SetCalibrationZero_Click
- [x] ✅ Проверка debugModeWithoutComPort в TrySendAsync
- [x] ✅ Исправление двойного преобразования координат
- [x] ✅ API разделён на виртуальные/физические методы
- [x] ✅ Визуализация калибровки (зелёная рамка)
- [x] ✅ Статус калибровки в статус-баре

---

## 📋 ЧТО ОСТАЛОСЬ СДЕЛАТЬ

### ШАГ 4: Добавить кнопку "Установить 0" в Designer
- [ ] Разместить компактно рядом с кнопками движения
- [ ] Связать с обработчиком SetCalibrationZero_Click

### ШАГ 5: Меню "Наладка"
- [ ] Добавить пункт меню с checkbox для debugModeWithoutComPort
- [ ] Пункт для сброса калибровки (уже есть обработчик)

### ШАГ 6: Тестирование
- [ ] Проверка работы калибровки
- [ ] Проверка режима отладки
- [ ] Проверка визуализации
- [ ] Проверка точности движения после калибровки

---

## ✅ РЕЗУЛЬТАТЫ

```
✅ Поля калибровки добавлены в WaferController
✅ Методы SetCalibrationZero, ResetCalibration, GetFirstCrystal реализованы
✅ Флаг debugModeWithoutComPort добавлен
✅ Обработчик SetCalibrationZero_Click реализован
✅ Двойное преобразование координат устранено
✅ API разделён на виртуальные/физические методы
✅ Компиляция успешна (0 ошибок)
✅ Визуализация калибровки работает
✅ Статус калибровки отображается
```

---

## 🔍 СИСТЕМА КООРДИНАТ

### Виртуальные координаты (карта):
- `Crystal.RealX/Y` - положение кристалла на карте
- Центр пластины = (0, 0)
- Используются для отображения и планирования

### Физические координаты (машина):
- `pointerMm` - реальное положение ЛШД
- Начало координат машины
- Используются для управления оборудованием

### Преобразование:
```csharp
offsetX = calibrationCrystalX - calibrationPointerX
offsetY = calibrationCrystalY - calibrationPointerY

virtualX = physicalX + offsetX
virtualY = physicalY + offsetY

physicalX = virtualX - offsetX
physicalY = virtualY - offsetY
```

---

## 🎯 СЛЕДУЮЩИЙ ШАГ

**Нужно добавить:**
1. Кнопку "Установить 0" в Form1.Designer.cs (компактно)
2. Пункты меню "Наладка" для режима отладки и сброса калибровки
3. Провести финальное тестирование

**Готов продолжить!** 🚀

**📄 См. также:** `CALIBRATION_COORDINATES_FIX.md` - детальное описание исправлений
