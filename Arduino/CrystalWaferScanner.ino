/*
 * Crystal Wafer Scanner - Arduino Control Sketch
 * 
 * ПРОТОКОЛ КОМАНД:
 * 0x01 - Move Left  (направление = HIGH, движение влево)
 * 0x02 - Move Right (направление = LOW, движение вправо)
 * 0x03 - Move Up
 * 0x04 - Move Down
 * 0x05 - Lock   (фиксация = HIGH)
 * 0x06 - Unlock (сброс = LOW)
 * 
 * ПИНЫ ARDUINO:
 * LOCK_PIN (9)      - Управление фиксацией (HIGH = фиксация, LOW = сброс)
 * DIR_X_PIN (2)     - Направление X (HIGH = влево, LOW = вправо)
 * DIR_Y_PIN (3)     - Направление Y (HIGH = вверх, LOW = вниз)
 * STEP_X_PIN (4)    - Шаг X
 * STEP_Y_PIN (5)    - Шаг Y
 * SENSOR_PIN (A0)   - Датчик присутствия кристалла
 */

// ===== КОНФИГУРАЦИЯ ПИНОВ =====
const int LOCK_PIN = 9;       // Пин фиксации/сброса
const int DIR_X_PIN = 2;      // Направление по X
const int DIR_Y_PIN = 3;      // Направление по Y
const int STEP_X_PIN = 4;     // Шаг по X
const int STEP_Y_PIN = 5;     // Шаг по Y
const int SENSOR_PIN = A0;    // Датчик (аналоговый)

// ===== КОНСТАНТЫ ПРОТОКОЛА =====
const byte CMD_MOVE_LEFT = 0x01;
const byte CMD_MOVE_RIGHT = 0x02;
const byte CMD_MOVE_UP = 0x03;
const byte CMD_MOVE_DOWN = 0x04;
const byte CMD_LOCK = 0x05;
const byte CMD_UNLOCK = 0x06;

// ===== НАСТРОЙКИ ДВИЖЕНИЯ =====
const unsigned long STEP_DELAY_US = 500;  // Задержка между шагами (мкс)
const unsigned long STEPS_PER_MM = 100;   // Шагов на миллиметр (калибровка)

// ===== СОСТОЯНИЕ СИСТЕМЫ =====
bool isLocked = false;
int lastSensorState = -1;

// ===== СТРУКТУРА КОМАНДЫ =====
struct Command {
    byte cmd;
    uint32_t steps;
};

void setup() {
    Serial.begin(115200);
    
    // Настройка пинов
    pinMode(LOCK_PIN, OUTPUT);
    pinMode(DIR_X_PIN, OUTPUT);
    pinMode(DIR_Y_PIN, OUTPUT);
    pinMode(STEP_X_PIN, OUTPUT);
    pinMode(STEP_Y_PIN, OUTPUT);
    pinMode(SENSOR_PIN, INPUT);
    
    // Начальное состояние
    digitalWrite(LOCK_PIN, LOW);   // Сброс
    digitalWrite(STEP_X_PIN, LOW);
    digitalWrite(STEP_Y_PIN, LOW);
    
    Serial.println("Crystal Wafer Scanner v1.0");
    Serial.println("Ready");
    
    // Отправляем начальное состояние
    sendLockEvent(false);
}

void loop() {
    // Проверка датчика
    checkSensor();
    
    // Обработка команд
    if (Serial.available() >= 5) {  // Команда: 1 байт + 4 байта данных
        Command cmd = readCommand();
        
        if (cmd.cmd != 0x00) {
            executeCommand(cmd);
        }
    }
}

// ===== ЧТЕНИЕ КОМАНДЫ =====
Command readCommand() {
    Command cmd;
    cmd.cmd = Serial.read();
    
    // Читаем 4 байта шагов (Little Endian)
    cmd.steps = 0;
    cmd.steps |= (uint32_t)Serial.read();
    cmd.steps |= ((uint32_t)Serial.read()) << 8;
    cmd.steps |= ((uint32_t)Serial.read()) << 16;
    cmd.steps |= ((uint32_t)Serial.read()) << 24;
    
    return cmd;
}

// ===== ВЫПОЛНЕНИЕ КОМАНДЫ =====
void executeCommand(Command cmd) {
    switch (cmd.cmd) {
        case CMD_MOVE_LEFT:
            // ✅ ВЛЕВО: Направление = HIGH
            digitalWrite(DIR_X_PIN, HIGH);
            moveSteps(STEP_X_PIN, cmd.steps);
            sendOk();
            break;
            
        case CMD_MOVE_RIGHT:
            // ✅ ВПРАВО: Направление = LOW
            digitalWrite(DIR_X_PIN, LOW);
            moveSteps(STEP_X_PIN, cmd.steps);
            sendOk();
            break;
            
        case CMD_MOVE_UP:
            // ВВЕРХ: Направление = HIGH
            digitalWrite(DIR_Y_PIN, HIGH);
            moveSteps(STEP_Y_PIN, cmd.steps);
            sendOk();
            break;
            
        case CMD_MOVE_DOWN:
            // ВНИЗ: Направление = LOW
            digitalWrite(DIR_Y_PIN, LOW);
            moveSteps(STEP_Y_PIN, cmd.steps);
            sendOk();
            break;
            
        case CMD_LOCK:
            // ✅ ФИКСАЦИЯ: Пин = HIGH
            digitalWrite(LOCK_PIN, HIGH);
            isLocked = true;
            sendLockEvent(true);
            sendOk();
            break;
            
        case CMD_UNLOCK:
            // ✅ СБРОС: Пин = LOW
            digitalWrite(LOCK_PIN, LOW);
            isLocked = false;
            sendLockEvent(false);
            sendOk();
            break;
            
        default:
            sendError("Unknown command");
            break;
    }
}

// ===== ВЫПОЛНЕНИЕ ШАГОВ =====
void moveSteps(int stepPin, uint32_t steps) {
    for (uint32_t i = 0; i < steps; i++) {
        digitalWrite(stepPin, HIGH);
        delayMicroseconds(STEP_DELAY_US);
        digitalWrite(stepPin, LOW);
        delayMicroseconds(STEP_DELAY_US);
        
        // Проверка датчика каждые 100 шагов
        if (i % 100 == 0) {
            checkSensor();
        }
    }
}

// ===== ПРОВЕРКА ДАТЧИКА =====
void checkSensor() {
    int sensorValue = analogRead(SENSOR_PIN);
    bool sensorActive = (sensorValue > 512);  // Порог: >2.5V = активен
    
    if (sensorActive != (lastSensorState == 1)) {
        lastSensorState = sensorActive ? 1 : 0;
        sendSensorEvent(sensorActive);
    }
}

// ===== ОТПРАВКА СОБЫТИЙ =====
void sendSensorEvent(bool active) {
    if (active) {
        Serial.println("EV S:1");
    } else {
        Serial.println("EV S:0");
    }
}

void sendLockEvent(bool locked) {
    if (locked) {
        Serial.println("EV L:1");
    } else {
        Serial.println("EV L:0");
    }
}

// ===== ОТПРАВКА ОТВЕТОВ =====
void sendOk() {
    Serial.println("OK");
}

void sendError(const char* message) {
    Serial.print("ERR: ");
    Serial.println(message);
}

// ===== ДИАГНОСТИКА =====
void printStatus() {
    Serial.print("Lock: ");
    Serial.print(isLocked ? "LOCKED" : "UNLOCKED");
    Serial.print(" | Sensor: ");
    Serial.println(lastSensorState == 1 ? "ACTIVE" : "INACTIVE");
}
