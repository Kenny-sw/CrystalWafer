#include <Arduino.h>

// -------------------- Пины  --------------------
const uint8_t PIN_STEP_X = 2; 
const uint8_t PIN_STEP_Y = 3;

const uint8_t PIN_DIR_X  = 4;
const uint8_t PIN_DIR_Y  = 5;

const uint8_t PIN_ENA    = 6;   // Сброс/Фиксация
  
const uint8_t PIN_EDGE   = 7;   // Датчик края (оптика/индуктивный и т.п.)

// Логика датчика: true = «пластина под датчиком»
const bool EDGE_ACTIVE_HIGH = true;

// -------------------- Профиль движения (стартовые значения) --------------------
// Реальные драйверы любят >=3–5 мкс импульс и десятки–сотни мкс между импульсами.
const uint16_t STEP_PULSE_US = 4;   // длительность импульса STEP
uint16_t minDelay = 200;            // мкс, «крейсерская» пауза (скорость)
uint16_t maxDelay = 800;            // мкс, начало/конец (медленнее)
byte accelPercent  = 30;
byte cruisePercent = 40;
byte decelPercent  = 30;

// -------------------- Команды протокола --------------------
enum Commands : byte {
  cmdStepXL     = 1,   // X влево
  cmdStepXR     = 2,   // X вправо
  cmdStepYup    = 3,   // Y вверх
  cmdStepYdown  = 4,   // Y вниз
  cmdLock       = 5,   // Фиксация (HIGH на PIN_ENA = моторы включены, стол зафиксирован)
  cmdUnlock     = 6,   // Сброс (LOW на PIN_ENA = моторы выключены, стол расфиксирован)
  cmdSetProfile = 7,   // (можно оставить закомментированным)
  cmdSensorQ    = 11,  // опрос датчика (мгновенный ответ S:1/S:0)
  cmdEdgeTouchT = 12   // шаблон «ощупать край по Z» (пока заглушка)
};

// -------------------- Состояние фиксации --------------------
volatile bool g_isLocked = false;

// -------------------- Датчик края: прерывание + событие в порт --------------------
const uint32_t EDGE_DEBOUNCE_US = 300; // фильтр дребезга
volatile bool     g_edgeRaw = false;         // текущее сырое состояние
volatile uint32_t g_edgeLastUs = 0;          // время последнего изменения
volatile bool     g_edgeChanged = false;     // «нужно отправить событие»

inline bool readEdgePin() {
  bool lvl = digitalRead(PIN_EDGE);
  return EDGE_ACTIVE_HIGH ? lvl : !lvl;
}

void ISR_edgeChange() {
  uint32_t now = micros();
  bool s = readEdgePin();
  // простой антидребезг: не чаще, чем раз в EDGE_DEBOUNCE_US
  if (now - g_edgeLastUs >= EDGE_DEBOUNCE_US) {
    g_edgeRaw = s;
    g_edgeLastUs = now;
    g_edgeChanged = true; // сообщим о событии из loop()/во время движений
  }
}

// Отправка события (только при изменении). Вызывать часто и из циклов движения.
inline void pumpEdgeEvent() {
  if (g_edgeChanged) {
    noInterrupts();
    bool s = g_edgeRaw;
    g_edgeChanged = false;
    interrupts();
    Serial.print(F("EV S:"));
    Serial.println(s ? 1 : 0);   // формат события: EV S:1 или EV S:0
  }
}

// Текущее стабильное состояние (без события)
inline bool edgeActive() {
  noInterrupts();
  bool s = g_edgeRaw;
  interrupts();
  return s;
}

// -------------------- Управление фиксацией через PIN_ENA --------------------
void setLock(bool locked) {
  digitalWrite(PIN_ENA, locked ? HIGH : LOW);
  g_isLocked = locked;
  // Отправка события о смене состояния фиксации
  Serial.print(F("EV L:"));
  Serial.println(locked ? 1 : 0);
  Serial.println(F("OK"));
}

// -------------------- Утилиты протокола --------------------
byte xorChecksum(const byte* data, int len) {
  byte c = 0; for (int i=0;i<len;i++) c ^= data[i]; return c;
}
uint32_t bytesToU32(const byte* b) {
  return (uint32_t)b[0] | ((uint32_t)b[1]<<8) | ((uint32_t)b[2]<<16) | ((uint32_t)b[3]<<24);
}

// -------------------- Класс оси --------------------
class Axis {
  uint8_t dirPin, stepPin;
public:
  Axis(uint8_t d, uint8_t s): dirPin(d), stepPin(s) {
    pinMode(dirPin, OUTPUT);
    pinMode(stepPin, OUTPUT);
    digitalWrite(dirPin, LOW);
    digitalWrite(stepPin, LOW);
  }

  inline void setDir(bool dirHigh) {
    digitalWrite(dirPin, dirHigh ? HIGH : LOW);
  }
  inline void pulseStep() {
    digitalWrite(stepPin, HIGH);
    delayMicroseconds(STEP_PULSE_US);
    digitalWrite(stepPin, LOW);
  }

  // Движение по профилю (трапеция). ПК указывает «куда» и «сколько».
  void moveProfile(uint32_t totalSteps, bool dirHigh) {
    setDir(dirHigh);
    if (totalSteps == 0) return;

    uint32_t accel = (totalSteps * accelPercent) / 100UL;
    uint32_t cruise= (totalSteps * cruisePercent) / 100UL;
    uint32_t decel = totalSteps - accel - cruise;

    auto lerp = [](uint16_t a, uint16_t b, uint32_t i, uint32_t n)->uint16_t {
      if (n==0) return b; // защита
      // линейная интерполяция целыми: a + (b-a)*i/n
      int32_t diff = (int32_t)b - (int32_t)a;
      return (uint16_t)(a + (diff * (int32_t)i) / (int32_t)n);
    };

    // Разгон: от maxDelay к minDelay
    for (uint32_t i=1; i<=accel; ++i) {
      pulseStep();
      delayMicroseconds( lerp(maxDelay, minDelay, i, accel) );
      pumpEdgeEvent();
    }
    // Крейсер
    for (uint32_t i=0; i<cruise; ++i) {
      pulseStep();
      delayMicroseconds(minDelay);
      pumpEdgeEvent();
    }
    // Торможение: от minDelay к maxDelay
    for (uint32_t i=1; i<=decel; ++i) {
      pulseStep();
      delayMicroseconds( lerp(minDelay, maxDelay, i, decel) );
      pumpEdgeEvent();
    }
  }
};

Axis X(PIN_DIR_X, PIN_STEP_X);
Axis Y(PIN_DIR_Y, PIN_STEP_Y);

// -------------------- Обработчики команд --------------------
void doMoveCmd(byte cmd, uint32_t steps) {
  // Проверка: можно двигаться только если стол зафиксирован (PIN_ENA = HIGH)
  if (!g_isLocked) {
    Serial.println(F("ERR:UNLOCKED"));
    return;
  }
  
  switch (cmd) {
    case cmdStepXL:    X.moveProfile(steps, /*dirHigh=*/false); break;
    case cmdStepXR:    X.moveProfile(steps, /*dirHigh=*/true ); break;
    case cmdStepYup:   Y.moveProfile(steps, /*dirHigh=*/true ); break;
    case cmdStepYdown: Y.moveProfile(steps, /*dirHigh=*/false); break;
  }
  Serial.println(F("OK"));
}

// -------------------- Приём и разбор команд --------------------
bool read5(byte* buf5) {
  // читаем 4 байта данных + 1 байт XOR
  for (byte i=0;i<5;i++) {
    uint32_t t0 = millis();
    while (!Serial.available()) { pumpEdgeEvent(); if (millis()-t0>100) return false; }
    buf5[i] = Serial.read();
  }
  return true;
}

void setup() {
  Serial.begin(115200);

  pinMode(PIN_ENA, OUTPUT);
  digitalWrite(PIN_ENA, LOW); // стартовое состояние: сброс (моторы выключены)
  g_isLocked = false;

  pinMode(PIN_EDGE, INPUT_PULLUP); // подстрой под свой датчик (внешняя подтяжка/инверсия)
  // Сразу читаем стартовое состояние
  g_edgeRaw = readEdgePin();
  g_edgeLastUs = micros();
  g_edgeChanged = true; // чтобы отправить первое «EV S:x» при старте

  // Прерывание на изменение уровня датчика
  attachInterrupt(digitalPinToInterrupt(PIN_EDGE), ISR_edgeChange, CHANGE);

  Serial.println(F("READY"));
}

void loop() {
  pumpEdgeEvent(); // доставим события даже когда нет команд

  if (!Serial.available()) return;

  byte cmd = Serial.read();

  // Команды без полезной нагрузки
  if (cmd == cmdSensorQ) {
    Serial.print(F("S:"));
    Serial.println(edgeActive() ? 1 : 0);
    return;
  }

  if (cmd == cmdLock) {
    setLock(true);  // HIGH = моторы включены, стол зафиксирован
    return;
  }

  if (cmd == cmdUnlock) {
    setLock(false);  // LOW = моторы выключены, стол расфиксирован
    return;
  }

  // Команды с 4 байтами данных + 1 байтом XOR
  if (cmd==cmdStepXL || cmd==cmdStepXR || cmd==cmdStepYup || cmd==cmdStepYdown ||
      cmd==cmdSetProfile || cmd==cmdEdgeTouchT) {

    byte buf[5];
    if (!read5(buf)) { Serial.println(F("ERR:TIMEOUT")); return; }
    byte rxCS = buf[4];
    byte calc = xorChecksum(buf,4);
    if (rxCS != calc) { Serial.println(F("ERR:CS")); return; }

    if (cmd == cmdSetProfile) {
      Serial.println(F("NA")); // сейчас не используем
      return;
    }

    uint32_t val = bytesToU32(buf);

    if (cmd == cmdEdgeTouchT) {
      Serial.println(F("NA")); // Not Available
      return;
    }

    // Обычное движение (требует фиксации)
    doMoveCmd(cmd, val);
    return;
  }

  // неизвестная команда
  Serial.println(F("ERR:CMD"));
}
