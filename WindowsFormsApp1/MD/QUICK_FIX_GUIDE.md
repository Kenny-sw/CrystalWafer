# 🔍 ДИАГНОСТИКА ОШИБКИ "Failed to send the command" - КРАТКАЯ ИНСТРУКЦИЯ

## ⚡ Быстрая диагностика

### Шаг 1: Откройте логи приложения

**Через меню:**
```
Помощь → Просмотр логов...
```

**Горячая клавиша:** `Ctrl+L`

**Вручную:**
```
1. Перейдите в папку установки приложения
2. Откройте папку "logs"
3. Найдите файл app_ГГГГММДД.log (сегодняшняя дата)
```

### Шаг 2: Найдите последнюю ошибку

В просмотрщике логов:
- Ошибки выделены **красным цветом** 🔴
- Предупреждения выделены **оранжевым** 🟠
- Прокрутите вниз для просмотра последних записей

Или в текстовом редакторе ищите:
- `[Error]`
- `[Warning]`
- `Failed`
- `ERR:`
- `Timeout`

---

## 🔴 КРИТИЧЕСКАЯ ПРОБЛЕМА В ARDUINO СКЕТЧЕ

### ❌ ПРОБЛЕМА: Несовпадение формата пакета

**C# отправляет:**
```
[CMD][D0][D1][D2][D3][XOR]
где XOR = CMD ^ D0 ^ D1 ^ D2 ^ D3
```

**Старый Arduino читал:**
```
cmd = Serial.read()  // читает CMD
read5(buf)           // читает [D0][D1][D2][D3][XOR]
calc = D0 ^ D1 ^ D2 ^ D3  // ❌ НЕПРАВИЛЬНО! CMD не учитывается
```

### ✅ ИСПРАВЛЕНИЕ в `StepByStep1.5.ino.ino`

**Строка ~220:**
```cpp
// ❌ БЫЛО:
byte calc = xorChecksum(buf,4);

// ✅ ДОЛЖНО БЫТЬ:
byte calc = cmd ^ buf[0] ^ buf[1] ^ buf[2] ^ buf[3];
```

**Полный исправленный блок:**
```cpp
// Команды с 4 байтами данных + 1 байтом XOR
if (cmd==cmdStepXL || cmd==cmdStepXR || cmd==cmdStepYup || cmd==cmdStepYdown ||
    cmd==cmdSetProfile || cmd==cmdEdgeTouchT) {

  byte buf[5];
  if (!read5(buf)) { 
    Serial.println(F("ERR:TIMEOUT")); 
    return; 
  }
  
  // ✅ ИСПРАВЛЕНО: XOR считается от CMD + 4 байта данных
  byte rxCS = buf[4];
  byte calc = cmd ^ buf[0] ^ buf[1] ^ buf[2] ^ buf[3];
  
  if (rxCS != calc) { 
    // Отладочный вывод для диагностики
    Serial.print(F("ERR:CS rx=0x"));
    Serial.print(rxCS, HEX);
    Serial.print(F(" calc=0x"));
    Serial.println(calc, HEX);
    return; 
  }
  
  // ... остальной код
}
```

---

## 📊 Типичные ошибки в логах

### 1. "Listener не запущен или завершился!"

**Лог:**
```
2024-01-15 14:23:45.123 [Error] Listener не запущен или завершился! Попытка перезапуска...
```

**Причина:** Слушатель ответов от Arduino остановился

**Решение:**
1. Отключитесь от COM-порта (кнопка "Disconnect")
2. Подключитесь заново

---

### 2. "Serial command 0x02 timed out after 3 seconds"

**Лог:**
```
2024-01-15 14:23:45.123 [Warning] Serial command 0x02 timed out after 3 seconds.
```

**Причина:** Arduino не ответил

**Решение:**
1. Откройте Arduino IDE → Serial Monitor (115200 baud)
2. Проверьте, что Arduino отправляет "READY" при старте
3. Отправьте команду Lock (0x05) - должен ответить "EV L:1" и "OK"
4. Если нет ответа - перезалейте исправленный скетч

---

### 3. "Failed to write to serial port"

**Лог:**
```
2024-01-15 14:23:45.123 [Error] Failed to write to serial port.
System.IO.IOException: Порт COM3 не существует.
```

**Причина:** Кабель отключен или драйвер упал

**Решение:**
1. Проверьте физическое подключение USB
2. Диспетчер устройств → Порты (COM и LPT)
3. Переподключите USB-кабель

---

### 4. "ERR:CS" от Arduino

**Serial Monitor Arduino:**
```
ERR:CS rx=0x9B calc=0x88
```

**Причина:** Ошибка контрольной суммы (старый скетч)

**Решение:**
1. **Загрузите исправленный скетч** `StepByStep1.5.ino.ino`
2. Проверьте, что XOR считается от CMD + 4 байта данных
3. Перезагрузите Arduino (кнопка RESET)

---

## 🛠️ Быстрое решение (90% случаев)

### Вариант 1: Если "ERR:CS"
```
1. Откройте StepByStep1.5.ino.ino
2. Найдите строку: byte calc = xorChecksum(buf,4);
3. Замените на: byte calc = cmd ^ buf[0] ^ buf[1] ^ buf[2] ^ buf[3];
4. Загрузите скетч в Arduino
5. Нажмите RESET на Arduino
6. Переподключите COM-порт в приложении
```

### Вариант 2: Если "Timeout"
```
1. Откройте Serial Monitor Arduino (115200 baud)
2. Проверьте, что Arduino отвечает "READY"
3. Если нет - перезалейте скетч
4. Если есть - проверьте, что listener запущен (см. логи)
```

### Вариант 3: Если "Listener не запущен"
```
1. Disconnect в приложении
2. Закройте Serial Monitor Arduino IDE
3. Connect в приложении
4. Проверьте статус: "COM подключён: COMx"
```

---

## 📋 Контрольный список исправлений

```
Arduino:
  [ ] Загружен исправленный StepByStep1.5.ino.ino
  [ ] XOR считается от CMD + 4 байта: calc = cmd ^ buf[0] ^ buf[1] ^ buf[2] ^ buf[3]
  [ ] Serial Monitor показывает "READY" (115200 baud)
  [ ] На команду Lock отвечает "EV L:1" + "OK"
  [ ] На движение отвечает "OK" (после Lock)

Application:
  [ ] COM-порт подключен
  [ ] Статус "COM подключён: COMx"
  [ ] Serial Monitor Arduino IDE закрыт
  [ ] Логи проверены (папка logs/)

Test:
  [ ] Lock (кнопка "🔓 Фиксация") работает
  [ ] Движение (стрелки) работает после Lock
  [ ] Нет ошибок "ERR:CS" в Serial Monitor
  [ ] Нет ошибок "Timeout" в логах приложения
```

---

## 📞 Если ничего не помогло

1. **Соберите диагностику:**
   ```
   - Лог приложения (logs/app_ГГГГММДД.log)
   - Скриншот Serial Monitor Arduino
   - Версия скетча (в начале файла)
   ```

2. **Проверьте формат пакета в логах:**
   ```
   TX -> cmd=0x05, data=0, packet=[05-00-00-00-00-05]
                                     └─CMD
                                        └──────DATA───────┘
                                                          └─XOR
   ```

3. **Создайте Issue:**
   https://github.com/Kenny-sw/CrystalWafer/issues

---

## ✅ После исправления

**Ожидаемое поведение:**

1. **Подключение:**
   - Лог: `COM port opened: COM3`
   - Serial Monitor: `READY`

2. **Команда Lock:**
   - Лог: `TX -> cmd=0x05, data=0`
   - Serial Monitor: `EV L:1` + `OK`
   - Лог: `RX <- OK: OK`

3. **Команда движения:**
   - Лог: `TX -> cmd=0x02, data=5000`
   - Serial Monitor: `OK`
   - Лог: `RX <- OK: OK`

---

**Версия:** 1.0  
**Дата:** 2024-01-15  
**Проект:** CrystalWafer  
**Автор:** Диагностика ошибок связи с Arduino
