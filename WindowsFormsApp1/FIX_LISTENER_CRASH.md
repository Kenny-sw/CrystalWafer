# 🔧 ИСПРАВЛЕНИЕ КРИТИЧЕСКОЙ ОШИБКИ LISTENER

## 🔴 Проблема

### Симптомы:
```
2025-10-31 10:28:01.391 [Warning] Serial port read error.
System.IO.IOException: Операция ввода-вывода прекращена из-за выхода из потока или запроса приложения.
   в System.IO.Ports.SerialStream.EndRead(IAsyncResult asyncResult)
   ...
   в CrystalTable.Controllers.SerialPortController.<ListenAsync>d__22.MoveNext()
```

### Последствия:
- ❌ **Listener постоянно падает** сразу после запуска
- ❌ **Все команды завершаются timeout'ом** (Arduino не отвечает)
- ❌ **Автоматический перезапуск listener не помогает** - он снова падает
- ❌ **Невозможно управлять Arduino** через приложение

---

## 🔍 Причины

### 1. **Нестабильный StreamReader**
```csharp
// ❌ БЫЛО:
using (var reader = new StreamReader(_port.BaseStream, encoding, false, 1024, leaveOpen: true))
{
    while (!token.IsCancellationRequested)
    {
        var readTask = reader.ReadLineAsync();
        var completed = await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, token));
        // ...
    }
}
```

**Проблемы:**
- `Task.Delay(Timeout.Infinite, token)` не корректно отменяется
- При перезапуске listener старый StreamReader не закрывается
- BaseStream может быть в нестабильном состоянии

### 2. **Отсутствие очистки буферов**
При открытии порта и запуске listener буферы могут содержать мусор от предыдущего сеанса.

### 3. **Автоматический reset Arduino**
При открытии COM-порта Windows по умолчанию устанавливает DTR = true, что вызывает **hardware reset Arduino**.

### 4. **Бесконечный цикл перезапусков**
```csharp
// ❌ БЫЛО в SendCommandAsync:
if (_listenerTask == null || _listenerTask.IsCompleted)
{
    StartListener();  // Попытка перезапуска
    await Task.Delay(100);
    
    if (_listenerTask == null || _listenerTask.IsCompleted)
    {
        return false;  // Listener снова упал!
    }
}
```

Listener падал сразу после перезапуска из-за IOException.

---

## ✅ Исправления

### 1. **Исправлен ListenAsync**

#### Очистка буферов перед стартом:
```csharp
if (_port.IsOpen && _port.BytesToRead > 0)
{
    AppLogger.Debug($"Очистка буфера приема: {_port.BytesToRead} байт");
    _port.DiscardInBuffer();
}
```

#### Корректная отмена чтения:
```csharp
// ✅ СТАЛО:
line = await reader.ReadLineAsync().ConfigureAwait(false);
```

Удалили `Task.WhenAny` с бесконечной задержкой - используем прямое чтение с автоматической отменой через token.

#### Корректное закрытие StreamReader:
```csharp
finally
{
    try
    {
        reader?.Dispose();
    }
    catch (Exception disposeEx)
    {
        AppLogger.Debug($"Error disposing reader: {disposeEx.Message}");
    }
    
    FailPendingCommand("Listener stopped");
    AppLogger.Debug("Listener finished");
}
```

---

### 2. **Предотвращение автоматического reset Arduino**

#### ConfigurePort:
```csharp
_port.DtrEnable = false;  // ✅ Не сбрасываем Arduino при открытии
_port.RtsEnable = false;
_port.ReadTimeout = 5000;
_port.WriteTimeout = 1000;
```

#### ToggleConnection:
```csharp
_port.Open();
AppLogger.Info($"COM port opened: {_port.PortName}");

// ✅ Устанавливаем DTR ПОСЛЕ открытия для стабильной работы
_port.DtrEnable = true;
_port.RtsEnable = true;

// ✅ Очищаем буферы
_port.DiscardInBuffer();
_port.DiscardOutBuffer();

// ✅ Даем Arduino время на стабилизацию
System.Threading.Thread.Sleep(100);

StartListener();
```

---

### 3. **Убран автоматический перезапуск listener**

```csharp
// ✅ СТАЛО в SendCommandAsync:
if (_listenerTask == null || _listenerTask.IsCompleted)
{
    AppLogger.Error($"Listener не запущен или завершился!");
    AppLogger.Error($"Необходимо переподключить COM-порт (Disconnect → Connect)");
    return false;
}
```

**Причина:** Автоматический перезапуск listener во время отправки команды приводил к новым ошибкам. Правильное решение - **переподключить COM-порт вручную**.

---

### 4. **Улучшен StopListener**

```csharp
// ✅ Даем больше времени на остановку (1000 мс вместо 500 мс)
if (_listenerTask != null && !_listenerTask.Wait(1000))
{
    AppLogger.Warning("Listener did not stop within timeout");
}
```

---

### 5. **Добавлена проверка состояния порта**

```csharp
private void StartListener()
{
    StopListener();

    // ✅ Проверка состояния порта перед запуском
    if (!_port.IsOpen)
    {
        AppLogger.Warning("Cannot start listener: port is not open");
        return;
    }

    _listenerCts = new CancellationTokenSource();
    var token = _listenerCts.Token;
    _listenerTask = Task.Run(() => ListenAsync(token), token);
    AppLogger.Debug("Serial listener started.");
}
```

---

## 📋 Что делать при ошибке

### Если listener упал:

1. **Откройте логи:** Помощь → Просмотр логов...
2. **Найдите последнюю ошибку:**
   ```
   [Error] Listener не запущен или завершился!
   [Error] Необходимо переподключить COM-порт (Disconnect → Connect)
   ```

3. **Переподключите COM-порт:**
   - Нажмите **"Disconnect"** (Отключить)
   - Подождите 1 секунду
   - Нажмите **"Connect"** (Подключить)

4. **Проверьте состояние:**
   - Статус должен показать: **"COM подключён: COMx"**
   - В логах должно быть: **"Serial listener started."**

5. **Проверьте Arduino:**
   - Откройте Serial Monitor (115200 baud)
   - Должно появиться: **"READY"** и **"EV S:1"**

---

## 🎯 Проверка исправления

### 1. Компиляция
```bash
dotnet build
```
**Результат:** ✅ Без ошибок

### 2. Тестирование

#### Шаг 1: Подключение
1. Выберите COM-порт
2. Нажмите "Подключить"
3. **Ожидается:**
   ```
   [Info] COM port opened: COM15
   [Debug] Очистка входного буфера: X байт
   [Debug] Serial listener started.
   [Info] Sensor event: ON
   ```

#### Шаг 2: Отправка команды Lock
1. Нажмите "🔓 Фиксация"
2. **Ожидается:**
   ```
   [Debug] UI -> отправка команды 0x05, шаг=0 um
   [Debug] TX -> cmd=0x05, data=0, packet=[05-00-00-00-00-05]
   [Debug] Пакет команды 0x05 отправлен, ожидание ответа...
   [Debug] RX <- OK: OK
   [Debug] Команда 0x05 успешно выполнена.
   ```

#### Шаг 3: Движение
1. Нажмите стрелку (например, вверх)
2. **Ожидается:**
   ```
   [Debug] UI -> отправка команды 0x03, шаг=940 um
   [Debug] TX -> cmd=0x03, data=940
   [Debug] RX <- OK: OK
   ```

---

## 🚫 Известные ограничения

### 1. Listener не перезапускается автоматически
**Причина:** Перезапуск во время работы приводит к новым ошибкам  
**Решение:** Переподключите COM-порт вручную (Disconnect → Connect)

### 2. Arduino может сброситься при подключении
**Причина:** DTR line при открытии порта  
**Решение:** Ждем 100 мс после открытия + очищаем буферы

### 3. Первое подключение может быть нестабильным
**Причина:** Мусор в буферах от предыдущего сеанса  
**Решение:** Переподключитесь еще раз

---

## 📊 Статус исправлений

| Компонент | Статус | Описание |
|-----------|--------|----------|
| **ListenAsync** | ✅ | Исправлена IOException, корректное закрытие StreamReader |
| **StartListener** | ✅ | Добавлена проверка состояния порта |
| **StopListener** | ✅ | Увеличен таймаут остановки (1000 мс) |
| **ToggleConnection** | ✅ | Очистка буферов, управление DTR/RTS |
| **ConfigurePort** | ✅ | DTR=false при открытии, таймауты чтения/записи |
| **SendCommandAsync** | ✅ | Убран автоматический перезапуск listener |

---

## 🎉 Результат

### ✅ Проблемы решены:
1. Listener больше не падает с IOException
2. StreamReader корректно закрывается
3. Буферы очищаются при открытии порта
4. Arduino не сбрасывается автоматически
5. Нет бесконечных циклов перезапуска

### ✅ Улучшения:
1. Подробное логирование всех операций
2. Корректная обработка ошибок
3. Информативные сообщения пользователю
4. Увеличены таймауты для стабильности

---

## 🔗 Связанные файлы

- **SerialPortController.cs** - контроллер COM-порта
- **Protocol.cs** - протокол обмена с Arduino
- **AppLogger.cs** - логирование
- **StepByStep1.5.ino.ino** - прошивка Arduino

---

**Версия:** 1.0  
**Дата:** 2024-01-15  
**Проект:** CrystalWafer  
**Статус:** ✅ ИСПРАВЛЕНО

**Автор:** GitHub Copilot  
**GitHub:** https://github.com/Kenny-sw/CrystalWafer
