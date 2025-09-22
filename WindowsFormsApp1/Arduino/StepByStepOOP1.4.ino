#include <Arduino.h>

// -------------------- Пины (подстрой под свою схему) --------------------
const uint8_t PIN_DIR_X  = 2;
const uint8_t PIN_STEP_X = 5;
const uint8_t PIN_DIR_Y  = 3;
const uint8_t PIN_STEP_Y = 4;
const uint8_t PIN_ENA    = 6;   // ENABLE драйверов (LOW=включено — проверь свой драйвер)
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

// -------------------- Команды протокола (как у тебя + новые) --------------------
enum Commands : byte {
  cmdStepXL     = 1,   // X влево
  cmdStepXR     = 2,   // X вправо
  cmdStepYup    = 3,   // Y вверх
  cmdStepYdown  = 4,   // Y вниз
  cmdScan       = 5,   // демо-скан
  cmdSetProfile = 7,   // (как у тебя; можно оставить закомментированным)
  cmdSensorQ    = 11,  // новый: опрос датчика (мгновенный ответ S:1/S:0)
  cmdEdgeTouchT = 12   // новый: шаблон «ощупать край по Z» (пока заглушка)
};

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

// -------------------- Утилиты протокола --------------------
byte xorChecksum(const byte* data, int len) {
  byte c = 0; for (int i=0;i<len;i++) c ^= data[i]; return c;
}
uint32_t bytesToU32(const byte* b) {
  return (uint32_t)b[0] | ((uint32_t)b[1]<<8) | ((uint32_t)b[2]<<16) | ((uint32_t)b[3]<<24);
}

// -------------------- Класс оси (без «самостоятельной логики направления») --------------------
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
      pumpEdgeEvent(); // не мешает движению; шлём событие только на реальном изменении
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
  switch (cmd) {
    case cmdStepXL:    X.moveProfile(steps, /*dirHigh=*/false); break;
    case cmdStepXR:    X.moveProfile(steps, /*dirHigh=*/true ); break;
    case cmdStepYup:   Y.moveProfile(steps, /*dirHigh=*/true ); break;
    case cmdStepYdown: Y.moveProfile(steps, /*dirHigh=*/false); break;
  }
  Serial.println(F("OK"));
}

// Демонстрация «сканирования» (оставлено как пример; события датчика будут приходить параллельно)
void doScanDemo(uint32_t steps) {
  Y.moveProfile(steps, true);
  X.moveProfile(steps, false);
  delay(300);
  for (byte i=0;i<4;i++) {
    X.moveProfile(20000, false); delay(100);
    X.moveProfile(20000, true ); delay(100);
  }
  Serial.println(F("OK"));
}

// Заглушка «ощупать край по Z» — сейчас НИЧЕГО не трогает, только сообщает «не реализовано»
void doEdgeTouchTemplate(uint32_t param) {
  // Здесь в будущем: опустить Z на N шагов до касания, зафиксировать точку, поднять Z и т.п.
  // Сейчас — просто сообщение, чтобы не мешать основному коду.
  (void)param;
  Serial.println(F("NA")); // Not Available
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
  digitalWrite(PIN_ENA, LOW); // включить драйверы (если у тебя наоборот — поставь HIGH)

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

  // Команда без полезной нагрузки: мгновенный опрос датчика
  if (cmd == cmdSensorQ) {
    Serial.print(F("S:"));
    Serial.println(edgeActive() ? 1 : 0);
    return;
  }

  // Команды с 4 байтами данных + 1 байтом XOR
  if (cmd==cmdStepXL || cmd==cmdStepXR || cmd==cmdStepYup || cmd==cmdStepYdown ||
      cmd==cmdScan   || cmd==cmdSetProfile || cmd==cmdEdgeTouchT) {

    byte buf[5];
    if (!read5(buf)) { Serial.println(F("ERR:TIMEOUT")); return; }
    byte rxCS = buf[4];
    byte calc = xorChecksum(buf,4);
    if (rxCS != calc) { Serial.println(F("ERR:CS")); return; }

    if (cmd == cmdSetProfile) {
      // формат: [minDelay u32][maxDelay u32][accel% u8][cruise% u8][decel% u8] — у тебя было 12 байт
      // тут оставлено как «совместимость»: сейчас 4 байта. Раскомментируй и переделай формат, если нужно.
      Serial.println(F("NA")); // сейчас не используем
      return;
    }

    uint32_t val = bytesToU32(buf);

    if (cmd == cmdScan) {
      doScanDemo(val);
      return;
    }
    if (cmd == cmdEdgeTouchT) {
      doEdgeTouchTemplate(val);
      return;
    }

    // Обычное движение (ПК указывает направление командой)
    doMoveCmd(cmd, val);
    return;
  }

  // неизвестная команда
  Serial.println(F("ERR:CMD"));
}
