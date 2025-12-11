#include <Arduino.h>

// ========================================================================
// Прошивка для управления шаговыми двигателями через ОПТРОНЫ
// (инверсная логика - пины Arduino управляют оптопарами)
// 
// Моя плата: Arduino Nano/Uno + плата с оптопарами + драйверы
// Схема: Arduino (LOW) -> оптрон открывается -> драйвер видит HIGH
// Логика: активный уровень LOW (инверсия через оптрон)
// ========================================================================

// -------------------- Назначение пинов --------------------
const uint8_t PIN_STEP_X = 2;  // Импульсы шагов для оси X (через оптрон)
const uint8_t PIN_STEP_Y = 3;  // Импульсы шагов для оси Y (через оптрон)

const uint8_t PIN_DIR_X  = 4;  // Направление движения оси X (через оптрон)
const uint8_t PIN_DIR_Y  = 5;  // Направление движения оси Y (через оптрон)

const uint8_t PIN_ENA    = 6;  // Включение/выключение моторов (через оптрон)

const uint8_t PIN_EDGE   = 7;  // Датчик края пластины (оптический/индуктивный)

// Как работает мой датчик: true = пластина обнаружена
const bool EDGE_ACTIVE_HIGH = true;

// -------------------- Настройки профиля движения --------------------
// Оптрону хватает импульса 2-4 мкс, драйверу тоже
const uint16_t STEP_PULSE_US = 4;   // Длительность импульса STEP (LOW-импульс)

// Параметры трапецеидального профиля (можно менять через ПК командой SetProfile)
uint16_t minDelay = 200;      // мкс между шагами на максимальной скорости
uint16_t maxDelay = 800;      // мкс между шагами при старте/остановке (медленно)
byte accelPercent  = 30;      // Сколько процентов шагов тратим на разгон
byte cruisePercent = 40;      // Сколько процентов едем на постоянной скорости
byte decelPercent  = 30;      // Сколько процентов тратим на торможение

// -------------------- Команды от компьютера --------------------
enum Commands : byte {
  cmdStepXL     = 1,   // Двигать X влево
  cmdStepXR     = 2,   // Двигать X вправо
  cmdStepYup    = 3,   // Двигать Y вверх
  cmdStepYdown  = 4,   // Двигать Y вниз
  cmdLock       = 5,   // Зафиксировать стол (включить моторы)
  cmdUnlock     = 6,   // Расфиксировать стол (выключить моторы)
  cmdSetProfile = 7,   // Изменить параметры профиля движения
  cmdGetProfile = 8,   // Запросить текущие параметры профиля
  cmdSensorQ    = 11,  // Спросить состояние датчика прямо сейчас
  cmdEdgeTouchT = 12   // Резерв под алгоритм поиска края (пока не используется)
};

// -------------------- Состояние системы --------------------
volatile bool g_isLocked = false;  // Зафиксирован ли стол (моторы включены)

// -------------------- Обработка датчика края --------------------
const uint32_t EDGE_DEBOUNCE_US = 300;  // Антидребезг 300 мкс
volatile bool     g_edgeRaw = false;    // Текущее состояние датчика
volatile uint32_t g_edgeLastUs = 0;     // Когда последний раз менялось
volatile bool     g_edgeChanged = false; // Надо отправить событие в ПК

// Читаю датчик с учётом его логики (прямой/инверсной)
inline bool readEdgePin() {
  bool lvl = digitalRead(PIN_EDGE);
  return EDGE_ACTIVE_HIGH ? lvl : !lvl;
}

// Прерывание при изменении датчика (срабатывает автоматически)
void ISR_edgeChange() {
  uint32_t now = micros();
  bool s = readEdgePin();
  
  // Фильтрую дребезг - игнорирую слишком частые изменения
  if (now - g_edgeLastUs >= EDGE_DEBOUNCE_US) {
    g_edgeRaw = s;
    g_edgeLastUs = now;
    g_edgeChanged = true;  // Отмечаю что надо сообщить в ПК
  }
}

// Отправляю событие датчика в ПК (вызывается из loop)
inline void pumpEdgeEvent() {
  if (g_edgeChanged) {
    noInterrupts();
    bool s = g_edgeRaw;
    g_edgeChanged = false;
    interrupts();
    
    // Шлю событие в формате "EV S:1" или "EV S:0"
    Serial.print(F("EV S:"));
    Serial.println(s ? 1 : 0);
  }
}

// Узнать текущее состояние датчика (без отправки события)
inline bool edgeActive() {
  noInterrupts();
  bool s = g_edgeRaw;
  interrupts();
  return s;
}

// -------------------- Управление фиксацией стола --------------------
// ВАЖНО: Через оптрон логика инвертируется!
// LOW на Arduino -> светодиод оптрона горит -> драйвер видит HIGH -> моторы ВКЛЮЧЕНЫ
// HIGH на Arduino -> светодиод оптрона не горит -> драйвер видит LOW -> моторы ВЫКЛЮЧЕНЫ
void setLock(bool locked) {
  // locked=true -> хочу зафиксировать -> ставлю LOW (оптрон откроется)
  // locked=false -> хочу отпустить -> ставлю HIGH (оптрон закроется)
  digitalWrite(PIN_ENA, locked ? LOW : HIGH);
  g_isLocked = locked;
  
  // Сообщаю в ПК о смене состояния
  Serial.print(F("EV L:"));
  Serial.println(locked ? 1 : 0);
  Serial.println(F("OK"));
}

// -------------------- Вспомогательные функции --------------------

// Контрольная сумма XOR (чтобы понять что пакет не испортился)
byte xorChecksum(const byte* data, int len) {
  byte c = 0;
  for (int i = 0; i < len; i++) {
    c ^= data[i];
  }
  return c;
}

// Собираю 4 байта в одно число uint32_t (младший байт первым)
uint32_t bytesToU32(const byte* b) {
  return (uint32_t)b[0] 
       | ((uint32_t)b[1] << 8) 
       | ((uint32_t)b[2] << 16) 
       | ((uint32_t)b[3] << 24);
}

// -------------------- Класс для управления одной осью --------------------
class Axis {
  uint8_t dirPin, stepPin;

public:
  // Инициализация при создании объекта
  Axis(uint8_t d, uint8_t s) : dirPin(d), stepPin(s) {
    pinMode(dirPin, OUTPUT);
    pinMode(stepPin, OUTPUT);
    
    // ВАЖНО: Через оптрон начальные уровни наоборот!
    digitalWrite(dirPin, LOW);    // DIR - любой начальный уровень
    digitalWrite(stepPin, HIGH);  // STEP в покое HIGH (оптрон закрыт)
  }

  // Установить направление движения
  // ВАЖНО: Через оптрон логика инвертируется!
  // dirHigh=true -> ставлю LOW -> оптрон откроется -> драйвер видит HIGH
  // dirHigh=false -> ставлю HIGH -> оптрон закроется -> драйвер видит LOW
  inline void setDir(bool dirHigh) {
    digitalWrite(dirPin, dirHigh ? LOW : HIGH);
  }

  // Сделать один шаг мотора
  // ВАЖНО: Импульс через оптрон = короткий провал в LOW
  inline void pulseStep() {
    digitalWrite(stepPin, LOW);   // Импульс вниз (оптрон откроется)
    delayMicroseconds(STEP_PULSE_US);
    digitalWrite(stepPin, HIGH);  // Возврат вверх (оптрон закроется)
  }

  // Двигаться по трапецеидальному профилю (плавный разгон-крейсер-торможение)
  void moveProfile(uint32_t totalSteps, bool dirHigh) {
    setDir(dirHigh);
    if (totalSteps == 0) return;  // Защита от нуля

    // Считаю сколько шагов на каждую фазу
    uint32_t accel  = (totalSteps * accelPercent)  / 100UL;
    uint32_t cruise = (totalSteps * cruisePercent) / 100UL;
    uint32_t decel  = totalSteps - accel - cruise;

    // Линейная интерполяция задержки (плавно меняю от a к b за n шагов)
    auto lerp = [](uint16_t a, uint16_t b, uint32_t i, uint32_t n) -> uint16_t {
      if (n == 0) return b;  // Защита от деления на ноль
      int32_t diff = (int32_t)b - (int32_t)a;
      return (uint16_t)(a + (diff * (int32_t)i) / (int32_t)n);
    };

    // ВАЖНО: Во время движения НЕ вызываю Serial.print - он тормозит на 100-500мкс!
    
    // Фаза 1: Разгон от maxDelay к minDelay
    for (uint32_t i = 1; i <= accel; ++i) {
      pulseStep();
      delayMicroseconds(lerp(maxDelay, minDelay, i, accel));
    }

    // Фаза 2: Крейсерская скорость (постоянная минимальная задержка)
    for (uint32_t i = 0; i < cruise; ++i) {
      pulseStep();
      delayMicroseconds(minDelay);
    }

    // Фаза 3: Торможение от minDelay к maxDelay
    for (uint32_t i = 1; i <= decel; ++i) {
      pulseStep();
      delayMicroseconds(lerp(minDelay, maxDelay, i, decel));
    }
    
    // После движения проверяю датчик и отправляю события если были
    pumpEdgeEvent();
  }
};

// Создаю два объекта для управления осями X и Y
Axis X(PIN_DIR_X, PIN_STEP_X);
Axis Y(PIN_DIR_Y, PIN_STEP_Y);

// -------------------- Обработка команд движения --------------------
void doMoveCmd(byte cmd, uint32_t steps) {
  // Двигаться можно только если стол зафиксирован!
  if (!g_isLocked) {
    Serial.println(F("ERR:UNLOCKED"));
    return;
  }
  
  // Выбираю направление движения в зависимости от команды
  // ПРИМЕЧАНИЕ: Если реальное направление не совпадает с ожидаемым -
  // просто меняю местами true/false в этих строчках
  switch (cmd) {
    case cmdStepXL:    X.moveProfile(steps, false); break;  // X влево
    case cmdStepXR:    X.moveProfile(steps, true);  break;  // X вправо
    case cmdStepYup:   Y.moveProfile(steps, true);  break;  // Y вверх
    case cmdStepYdown: Y.moveProfile(steps, false); break;  // Y вниз
  }
  
  Serial.println(F("OK"));  // Сообщаю что выполнил
}

// -------------------- Приём данных от компьютера --------------------

// Читаю 5 байт (4 байта данных + 1 байт контрольной суммы)
bool read5(byte* buf5) {
  for (byte i = 0; i < 5; i++) {
    uint32_t t0 = millis();
    
    // Жду байт максимум 100мс
    while (!Serial.available()) {
      pumpEdgeEvent();  // Пока жду - обрабатываю датчик
      if (millis() - t0 > 100) {
        return false;  // Тайм-аут
      }
    }
    
    buf5[i] = Serial.read();
  }
  return true;
}

// -------------------- Инициализация при включении --------------------
void setup() {
  Serial.begin(115200);  // Скорость должна совпадать с ПК!

  // Настраиваю пин фиксации
  pinMode(PIN_ENA, OUTPUT);
  digitalWrite(PIN_ENA, HIGH);  // Старт: стол свободен (оптрон закрыт, моторы выключены)
  g_isLocked = false;

  // Настраиваю датчик края
  pinMode(PIN_EDGE, INPUT_PULLUP);
  g_edgeRaw = readEdgePin();
  g_edgeLastUs = micros();
  g_edgeChanged = true;  // Отправлю первое событие при старте

  // Включаю прерывание на любое изменение датчика
  attachInterrupt(digitalPinToInterrupt(PIN_EDGE), ISR_edgeChange, CHANGE);

  Serial.println(F("READY"));  // Сообщаю что готов работать
}

// -------------------- Главный цикл --------------------
void loop() {
  // Обрабатываю события датчика даже если нет команд
  pumpEdgeEvent();

  // Если нет данных от ПК - просто жду
  if (!Serial.available()) return;

  byte cmd = Serial.read();

  // ===== Команды без дополнительных данных =====
  
  // Запрос состояния датчика
  if (cmd == cmdSensorQ) {
    Serial.print(F("S:"));
    Serial.println(edgeActive() ? 1 : 0);
    return;
  }

  // Фиксация стола
  if (cmd == cmdLock) {
    setLock(true);
    return;
  }

  // Расфиксация стола
  if (cmd == cmdUnlock) {
    setLock(false);
    return;
  }

  // Запрос текущего профиля
  if (cmd == cmdGetProfile) {
    Serial.print(F("PROFILE:"));
    Serial.print(minDelay);
    Serial.print(F(","));
    Serial.print(maxDelay);
    Serial.print(F(","));
    Serial.print(accelPercent);
    Serial.print(F(","));
    Serial.print(cruisePercent);
    Serial.print(F(","));
    Serial.println(decelPercent);
    return;
  }

  // ===== Команды с данными (4 байта + контрольная сумма) =====
  
  if (cmd == cmdStepXL || cmd == cmdStepXR || 
      cmd == cmdStepYup || cmd == cmdStepYdown ||
      cmd == cmdSetProfile || cmd == cmdEdgeTouchT) {

    byte buf[5];
    if (!read5(buf)) {
      Serial.println(F("ERR:TIMEOUT"));
      return;
    }

    // Проверяю контрольную сумму
    // ПК отправляет: [CMD][D0][D1][D2][D3][XOR(CMD^D0^D1^D2^D3)]
    byte rxCS = buf[4];
    byte calc = cmd ^ buf[0] ^ buf[1] ^ buf[2] ^ buf[3];

    if (rxCS != calc) {
      // Пакет испортился - сообщаю для отладки
      Serial.print(F("ERR:CS rx=0x"));
      Serial.print(rxCS, HEX);
      Serial.print(F(" calc=0x"));
      Serial.println(calc, HEX);
      return;
    }

    // Установка нового профиля
    if (cmd == cmdSetProfile) {
      uint32_t val = bytesToU32(buf);
      
      // ПК передаёт: младшие 16 бит = minDelay, старшие 16 бит = maxDelay
      uint16_t newMinDelay = (uint16_t)(val & 0xFFFF);
      uint16_t newMaxDelay = (uint16_t)((val >> 16) & 0xFFFF);
      
      // Проверяю что значения разумные
      if (newMinDelay < 100 || newMinDelay > 5000) {
        Serial.println(F("ERR:MIN_DELAY"));
        return;
      }
      
      if (newMaxDelay < 100 || newMaxDelay > 5000) {
        Serial.println(F("ERR:MAX_DELAY"));
        return;
      }
      
      if (newMinDelay >= newMaxDelay) {
        Serial.println(F("ERR:DELAY_ORDER"));
        return;
      }
      
      // Применяю новые настройки
      minDelay = newMinDelay;
      maxDelay = newMaxDelay;
      
      // Подтверждаю что принял
      Serial.print(F("PSET "));
      Serial.print(minDelay);
      Serial.print(F(","));
      Serial.println(maxDelay);
      return;
    }

    // Команда поиска края (пока не реализовано)
    if (cmd == cmdEdgeTouchT) {
      Serial.println(F("NA"));
      return;
    }

    // Обычное движение (все остальные команды)
    uint32_t val = bytesToU32(buf);
    doMoveCmd(cmd, val);
    return;
  }

  // Если дошёл сюда - команда неизвестна
  Serial.println(F("ERR:CMD"));
}
