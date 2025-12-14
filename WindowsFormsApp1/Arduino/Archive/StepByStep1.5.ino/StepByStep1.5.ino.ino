#include <Arduino.h>

// ========================================
//
// 1) Входы драйвера (DIR / STEP / ENA) сидят на +4 В через светодиод оптрона и резистор,
//    а пины Arduino просто стягивают эту точку на GND.
//    Для себя: логика инверсная.
//      - LOW на пине Arduino -> ток через LED -> оптрон открыт -> для платы сигнал "активен".
//      - HIGH на пине Arduino -> LED не светит -> оптрон закрыт -> сигнал "неактивен".
//
// 2) Хочу вести себя как родная плата:
//      - STEP: в покое держу HIGH, шаг — это короткий провал в LOW.
//      - ENA (FIX): LOW = драйвер включен, стол зажат; HIGH = драйвер выключен, стол свободен.
//      - DIR: из-за оптрона тоже получается "активный низ". Какое направление даёт реальное
//             движение "влево/вправо" — смотреть по месту и при необходимости
//             просто поменять местами направления в doMoveCmd().
//
// ============================================================================


// -------------------- Пины (моя разводка) --------------------
const uint8_t PIN_STEP_X = 2;
const uint8_t PIN_STEP_Y = 3;

const uint8_t PIN_DIR_X  = 4;
const uint8_t PIN_DIR_Y  = 5;

const uint8_t PIN_ENA    = 6;   // Вход ENA драйвера, через оптрон. Для платы активный LOW.

const uint8_t PIN_EDGE   = 7;   // Вход датчика края (оптика, индуктивный и т.п.)

// Как трактую сигнал датчика: true = «пластина под датчиком»
const bool EDGE_ACTIVE_HIGH = true;

// -------------------- Профиль движения (стартовые настройки) --------------------
// Драйверу нужны короткие, но не слишком, импульсы: обычно >= 3–5 мкс и паузы десятки–сотни мкс.
const uint16_t STEP_PULSE_US = 4;   // длительность импульса STEP (LOW-импульс)
uint16_t minDelay = 200;            // мкс, минимальная пауза между шагами (быстрая часть)
uint16_t maxDelay = 800;            // мкс, начало/конец трапеции (медленно)
byte accelPercent  = 30;            // доля шагов на разгон
byte cruisePercent = 40;            // доля шагов на постоянной скорости
byte decelPercent  = 30;            // доля шагов на торможение

// -------------------- Команды протокола --------------------
enum Commands : byte {
  cmdStepXL     = 1,   // X влево
  cmdStepXR     = 2,   // X вправо
  cmdStepYup    = 3,   // Y вверх
  cmdStepYdown  = 4,   // Y вниз
  cmdLock       = 5,   // Фиксация: ENA = LOW -> драйвер включен, стол зажат
  cmdUnlock     = 6,   // Разблокировка: ENA = HIGH -> драйвер отключен, стол свободен
  cmdSetProfile = 7,   // Зарезервировано под настройку профиля (пока не используется)
  cmdSensorQ    = 11,  // Опрос датчика края: мгновенный ответ S:1 / S:0
  cmdEdgeTouchT = 12   // Шаблон «ощупать край по Z» (пока просто заглушка)
};

// -------------------- Состояние фиксации стола --------------------
volatile bool g_isLocked = false;

// -------------------- Датчик края: прерывание + событие в порт --------------------
const uint32_t EDGE_DEBOUNCE_US = 300; // антидребезг по времени, мкс
volatile bool     g_edgeRaw = false;   // последнее зафиксированное состояние датчика
volatile uint32_t g_edgeLastUs = 0;    // время последнего изменения (микросекунды)
volatile bool     g_edgeChanged = false; // флаг: было изменение, нужно отправить событие

// Приводим уровень с пина к логике "true = пластина под датчиком"
inline bool readEdgePin() {
  bool lvl = digitalRead(PIN_EDGE);
  return EDGE_ACTIVE_HIGH ? lvl : !lvl;
}

void ISR_edgeChange() {
  uint32_t now = micros();
  bool s = readEdgePin();
  // Простой антидребезг: не принимать новые изменения чаще, чем раз в EDGE_DEBOUNCE_US
  if (now - g_edgeLastUs >= EDGE_DEBOUNCE_US) {
    g_edgeRaw = s;
    g_edgeLastUs = now;
    g_edgeChanged = true; // дальше loop()/движение отправят событие
  }
}

// Отправка события о датчике (только при изменении).
// Нужно вызывать часто, в том числе внутри циклов движения.
inline void pumpEdgeEvent() {
  if (g_edgeChanged) {
    noInterrupts();
    bool s = g_edgeRaw;
    g_edgeChanged = false;
    interrupts();
    Serial.print(F("EV S:"));
    Serial.println(s ? 1 : 0);   // формат события: "EV S:1" или "EV S:0"
  }
}

// Получить текущее стабильное состояние датчика (без генерации события)
inline bool edgeActive() {
  noInterrupts();
  bool s = g_edgeRaw;
  interrupts();
  return s;
}

// -------------------- Управление фиксацией через PIN_ENA --------------------
//
// Для памяти: ENA управляет НЕ мотором напрямую, а входом платы через оптрон (ACTIVE LOW).
//   - LOW  на PIN_ENA  -> диод оптрона светит -> плата видит «драйвер ВКЛ / стол зажат».
//   - HIGH на PIN_ENA  -> диод не светит     -> плата видит «драйвер ВЫКЛ / стол свободен».
void setLock(bool locked) {
  // locked = true  -> хочу зафиксировать стол: ENA тяну в 0 (LOW).
  // locked = false -> хочу отпустить: ENA оставляю в HIGH.
  digitalWrite(PIN_ENA, locked ? LOW : HIGH);
  g_isLocked = locked;

  // Шлю событие о смене состояния фиксации
  Serial.print(F("EV L:"));
  Serial.println(locked ? 1 : 0);
  Serial.println(F("OK"));
}

// -------------------- Утилиты протокола --------------------
byte xorChecksum(const byte* data, int len) {
  byte c = 0;
  for (int i = 0; i < len; i++) c ^= data[i];
  return c;
}

// Собираю 4 байта в uint32_t (малый байт первым)
uint32_t bytesToU32(const byte* b) {
  return (uint32_t)b[0]
       | ((uint32_t)b[1] << 8)
       | ((uint32_t)b[2] << 16)
       | ((uint32_t)b[3] << 24);
}

// -------------------- Класс оси --------------------
//
// Обертка над парой DIR/STEP. Тут уже учтено, что для платы "активный" уровень получается через LOW на выходе Arduino.
class Axis {
  uint8_t dirPin, stepPin;

public:
  Axis(uint8_t d, uint8_t s) : dirPin(d), stepPin(s) {
    pinMode(dirPin, OUTPUT);
    pinMode(stepPin, OUTPUT);

    // Состояние "по умолчанию":
    //  - DIR можно держать в любом уровне — это просто базовое направление.
    //  - STEP ДОЛЖЕН быть в HIGH, иначе можно словить "ложный шаг" при старте.
    digitalWrite(dirPin, LOW);
    digitalWrite(stepPin, HIGH);
  }

  // Выставить направление движения.
  //
  // ВАЖНО для себя: реальная логика платы инвертируется оптроном.
  // Здесь договор такой:
  //   dirHigh == true  -> на физический пин даю LOW  -> оптрон открыт  -> "DIR_internal = 1".
  //   dirHigh == false -> на физический пин даю HIGH -> оптрон закрыт  -> "DIR_internal = 0".
  //
  // Если по факту "лево/право" окажется перепутано — правится не здесь, а в doMoveCmd(),
  // просто меняю местами true/false.
  inline void setDir(bool dirHigh) {
    digitalWrite(dirPin, dirHigh ? LOW : HIGH);
  }

  // Один STEP-импульс: короткий провал в LOW (на время импульса светится LED оптрона).
  inline void pulseStep() {
    digitalWrite(stepPin, LOW);    // активный импульс (LED ON)
    delayMicroseconds(STEP_PULSE_US);
    digitalWrite(stepPin, HIGH);   // обратно в "покой" (LED OFF)
  }

  // Движение по трапецеидальному профилю.
  // ПК говорит «сколько шагов» и «в какую сторону» (dirHigh).
  void moveProfile(uint32_t totalSteps, bool dirHigh) {
    setDir(dirHigh);
    if (totalSteps == 0) return;

    uint32_t accel  = (totalSteps * accelPercent)  / 100UL; // шаги разгона
    uint32_t cruise = (totalSteps * cruisePercent) / 100UL; // шаги на крейсерской
    uint32_t decel  = totalSteps - accel - cruise;          // шаги торможения

    // Линейная интерполяция задержки между шагами (от a к b за n шагов)
    auto lerp = [](uint16_t a, uint16_t b, uint32_t i, uint32_t n) -> uint16_t {
      if (n == 0) return b; // защита от деления на 0
      int32_t diff = (int32_t)b - (int32_t)a;
      return (uint16_t)(a + (diff * (int32_t)i) / (int32_t)n);
    };

    // Разгон: от maxDelay к minDelay
    for (uint32_t i = 1; i <= accel; ++i) {
      pulseStep();
      delayMicroseconds(lerp(maxDelay, minDelay, i, accel));
      pumpEdgeEvent(); // датчик продолжаем обслуживать во время движения
    }

    // Крейсерская часть: фиксированная минимальная задержка
    for (uint32_t i = 0; i < cruise; ++i) {
      pulseStep();
      delayMicroseconds(minDelay);
      pumpEdgeEvent();
    }

    // Торможение: от minDelay к maxDelay
    for (uint32_t i = 1; i <= decel; ++i) {
      pulseStep();
      delayMicroseconds(lerp(minDelay, maxDelay, i, decel));
      pumpEdgeEvent();
    }
  }
};

// Две оси: X и Y. Порядок: сначала DIR, потом STEP.
Axis X(PIN_DIR_X, PIN_STEP_X);
Axis Y(PIN_DIR_Y, PIN_STEP_Y);

// -------------------- Обработчики команд движения --------------------
void doMoveCmd(byte cmd, uint32_t steps) {
  // Двигаться разрешаю только при зажатом столе (драйвер включён).
  if (!g_isLocked) {
    Serial.println(F("ERR:UNLOCKED"));
    return;
  }

  switch (cmd) {
    // ВАЖНО: если по факту "лево/право" окажутся перепутаны,
    // здесь достаточно поменять местами true/false в вызовах moveProfile().
    case cmdStepXL:    // X влево
      X.moveProfile(steps, /*dirHigh=*/false);
      break;
    case cmdStepXR:    // X вправо
      X.moveProfile(steps, /*dirHigh=*/true);
      break;

    case cmdStepYup:   // Y вверх
      Y.moveProfile(steps, /*dirHigh=*/true);
      break;
    case cmdStepYdown: // Y вниз
      Y.moveProfile(steps, /*dirHigh=*/false);
      break;
  }

  Serial.println(F("OK"));
}

// -------------------- Приём и разбор "хвоста" команды (5 байт) --------------------
bool read5(byte* buf5) {
  // Жду 4 байта данных + 1 байт XOR.
  for (byte i = 0; i < 5; i++) {
    uint32_t t0 = millis();
    while (!Serial.available()) {
      pumpEdgeEvent();             // заодно не теряю события по датчику
      if (millis() - t0 > 100) return false;  // таймаут на байт ~100 мс
    }
    buf5[i] = Serial.read();
  }
  return true;
}

// -------------------- setup / loop --------------------
void setup() {
  Serial.begin(115200);

  // ENA: старт — стол свободен, драйвер выключен (LED оптрона погашен).
  pinMode(PIN_ENA, OUTPUT);
  digitalWrite(PIN_ENA, HIGH);
  g_isLocked = false;

  // Настройка датчика края
  pinMode(PIN_EDGE, INPUT_PULLUP); // при необходимости поменять режим и EDGE_ACTIVE_HIGH под свой датчик
  // Прочитать стартовое состояние, чтобы не было "мусора" при первом EV
  g_edgeRaw = readEdgePin();
  g_edgeLastUs = micros();
  g_edgeChanged = true; // сразу после старта отправлю первое «EV S:x»

  // Прерывание на любое изменение уровня датчика (front + back)
  attachInterrupt(digitalPinToInterrupt(PIN_EDGE), ISR_edgeChange, CHANGE);

  Serial.println(F("READY"));
}

void loop() {
  // Даже если нет команд, всё равно выгружаю события по датчику
  pumpEdgeEvent();

  if (!Serial.available()) return;

  byte cmd = Serial.read();

  // Команды без полезной нагрузки в хвосте
  if (cmd == cmdSensorQ) {
    Serial.print(F("S:"));
    Serial.println(edgeActive() ? 1 : 0);
    return;
  }

  if (cmd == cmdLock) {
    setLock(true);  // LOW на ENA = моторы включены, стол зажат
    return;
  }

  if (cmd == cmdUnlock) {
    setLock(false); // HIGH на ENA = моторы выключены, стол свободен
    return;
  }

  // Команды, которые тянут за собой 4 байта данных + 1 байт XOR
  if (cmd == cmdStepXL || cmd == cmdStepXR ||
      cmd == cmdStepYup || cmd == cmdStepYdown ||
      cmd == cmdSetProfile || cmd == cmdEdgeTouchT) {

    byte buf[5];
    if (!read5(buf)) {
      Serial.println(F("ERR:TIMEOUT"));
      return;
    }

    // XOR считается от CMD + 4 байт данных.
    // C#-сторона шлёт так: [CMD][D0][D1][D2][D3][XOR(CMD^D0^D1^D2^D3)]
    byte rxCS = buf[4];
    byte calc = cmd ^ buf[0] ^ buf[1] ^ buf[2] ^ buf[3];

    if (rxCS != calc) {
      Serial.print(F("ERR:CS rx=0x"));
      Serial.print(rxCS, HEX);
      Serial.print(F(" calc=0x"));
      Serial.println(calc, HEX);
      return;
    }

    if (cmd == cmdSetProfile) {
      // Пока не реализовано изменение профиля с ПК
      Serial.println(F("NA"));
      return;
    }

    uint32_t val = bytesToU32(buf);

    if (cmd == cmdEdgeTouchT) {
      // Заглушка под будущий алгоритм "нащупать край"
      Serial.println(F("NA"));
      return;
    }

    // Обычное движение (требует, чтобы стол был зажат)
    doMoveCmd(cmd, val);
    return;
  }

  // Если дошли сюда — команда неизвестна
  Serial.println(F("ERR:CMD"));
}
