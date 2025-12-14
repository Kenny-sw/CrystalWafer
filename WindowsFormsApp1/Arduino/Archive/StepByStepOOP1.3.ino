#include <Arduino.h>

// Глобальные переменные для профиля движения (начальные значения)
uint16_t minDelay = 1;    // Минимальная задержка между шагами (мкс)
uint16_t maxDelay = 10;   // Максимальная задержка между шагами (мкс)
byte accelPercent = 30;   // Процент шагов для этапа разгона
byte cruisePercent = 40;  // Процент шагов для основного участка
byte decelPercent = 30;   // Процент шагов для этапа торможения

class Stol {
private:
  byte pinDirection;  // Пин для управления направлением двигателя
  byte pinStep;       // Пин для управления шагами двигателя
public:
  // Конструктор класса, принимающий пины для направления и шага
  Stol(byte directionPin, byte stepPin) {
    pinDirection = directionPin;
    pinStep = stepPin;
    pinMode(pinDirection, OUTPUT);
    pinMode(pinStep, OUTPUT);
    digitalWrite(pinDirection, LOW);
    digitalWrite(pinStep, LOW);
  }

  // Функция движения: выполняет движение на заданное количество шагов в указанном направлении
  void Move(unsigned int totalSteps, byte direction) {
    // Устанавливаем направление движения
    digitalWrite(pinDirection, direction ? HIGH : LOW);

    // Рассчитываем количество шагов для этапов разгона, равномерного движения и торможения
    unsigned int accelSteps = (totalSteps * accelPercent) / 100;
    unsigned int cruiseSteps = (totalSteps * cruisePercent) / 100;
    unsigned int decelSteps = totalSteps - accelSteps - cruiseSteps;

    // Лямбда-функция для расчёта задержки в этапе разгона (ускорение)
    auto accelDelay = [](int step, int total) -> uint32_t {
      float t = (float)step / total;
      return maxDelay - (maxDelay - minDelay) * t * t;
    };

    // Лямбда-функция для расчёта задержки в этапе торможения (замедление)
    auto decelDelay = [](int step, int total) -> uint32_t {
      float t = (float)step / total;
      return minDelay + (maxDelay - minDelay) * t * t;
    };

    // Этап разгона
    for (unsigned int i = 1; i <= accelSteps; i++) {
      digitalWrite(pinStep, HIGH);
      delayMicroseconds(2);  // Короткий импульс для шага
      digitalWrite(pinStep, LOW);
      delayMicroseconds(accelDelay(i, accelSteps));
    }

    // Этап равномерного движения (крейс)
    for (unsigned int i = 0; i < cruiseSteps; i++) {
      digitalWrite(pinStep, HIGH);
      delayMicroseconds(2);
      digitalWrite(pinStep, LOW);
      delayMicroseconds(minDelay);
    }

    // Этап торможения
    for (unsigned int i = 1; i <= decelSteps; i++) {
      digitalWrite(pinStep, HIGH);
      delayMicroseconds(2);
      digitalWrite(pinStep, LOW);
      delayMicroseconds(decelDelay(i, decelSteps));
    }
  }
};

// Инициализация объектов для управления двигателями по осям X и Y
Stol StepX(2, 5);  // Ось X: пин направления — 2, пин шага — 5
Stol StepY(3, 4);  // Ось Y: пин направления — 3, пин шага — 4

// Перечисление возможных команд
enum Commands {
  cmdStepXL = 1,     // Движение влево по оси X
  cmdStepXR = 2,     // Движение вправо по оси X
  cmdStepYup = 3,    // Движение вверх по оси Y
  cmdStepYdown = 4,  // Движение вниз по оси Y
  cmdScan = 5,       // Команда сканирования
  cmdSetProfile = 7  // Команда установки параметров профиля движения
};

// Функция для вычисления контрольной суммы (XOR всех байтов в массиве)
byte calculateChecksum(byte* data, int length) {
  byte checksum = 0;
  for (int i = 0; i < length; i++) {
    checksum ^= data[i];
  }
  return checksum;
}

// Функция для безопасного преобразования 4 байт из массива в число типа uint32_t.
// Используем побитовые сдвиги, чтобы собрать число из 4 байтов.
uint32_t bytesToUint32(byte* buffer) {
  return ((uint32_t)buffer[0]) |        // Младший байт (без сдвига)
         ((uint32_t)buffer[1] << 8) |   // Второй байт, сдвигаем на 8 бит
         ((uint32_t)buffer[2] << 16) |  // Третий байт, сдвигаем на 16 бит
         ((uint32_t)buffer[3] << 24);   // Старший байт, сдвигаем на 24 бита
}

// Функция для обработки команд движения
void processMoveCommand(byte cmd, uint32_t steps) {
  switch (cmd) {
    case cmdStepXL:
      StepX.Move(steps, 0);  // Движение влево по оси X
      break;
    case cmdStepXR:
      StepX.Move(steps, 1);  // Движение вправо по оси X
      break;
    case cmdStepYup:
      StepY.Move(steps, 1);  // Движение вверх по оси Y
      break;
    case cmdStepYdown:
      StepY.Move(steps, 0);  // Движение вниз по оси Y
      break;
  }
}

// Функция для обработки команды сканирования
void processScanCommand(uint32_t steps) {
  // Пример сканирования: последовательное движение по осям
  StepY.Move(steps, 1);  // Движение вверх по оси Y
  StepX.Move(steps, 0);  // Движение влево по оси X
  delay(300);            // Пауза перед началом цикла
  for (byte i = 0; i < 10; i++) {
    StepX.Move(50000, 0);  // Движение влево на 50000 шагов
    delay(200);
    StepX.Move(50000, 1);  // Движение вправо на 50000 шагов
    delay(200);
  }
}

/*// Функция для обработки команды установки профиля движения
void processSetProfileCommand(byte* buffer) {
  // Извлечение параметров из буфера:
  // Извлекаем 4 байта для newMinDelay
  uint32_t newMinDelay = bytesToUint32(&buffer[0]);
  // Извлекаем 4 байта для newMaxDelay
  uint32_t newMaxDelay = bytesToUint32(&buffer[4]);
  // Извлекаем оставшиеся 3 байта для процентного распределения этапов
  byte newAccelPercent = buffer[8];   // 1 байт для процента разгона
  byte newCruisePercent = buffer[9];    // 1 байт для процента основного участка
  byte newDecelPercent = buffer[10];    // 1 байт для процента торможения
  
  // Проверка корректности параметров:
  // newMinDelay должен быть >= 1 и меньше newMaxDelay,
  // newMaxDelay не должен превышать 1000,
  // Сумма процентов для разгона, крейса и торможения должна равняться 100.
  if (newMinDelay >= 1 && newMinDelay < newMaxDelay && newMaxDelay <= 1000 &&
      newAccelPercent + newCruisePercent + newDecelPercent == 100) {
    // Обновляем глобальные параметры профиля
    minDelay = newMinDelay;
    maxDelay = newMaxDelay;
    accelPercent = newAccelPercent;
    cruisePercent = newCruisePercent;
    decelPercent = newDecelPercent;
    Serial.println("Профиль успешно обновлен");
  } else {
    Serial.println("Ошибка: некорректные параметры профиля");
  }
}
*/

void setup() {
  Serial.begin(9600);
  // Инициализация пинов производится в конструкторах объектов StepX и StepY
}

void loop() {
  if (Serial.available() >= 6) {  // Проверяем наличие достаточного количества байтов
    byte commandType = Serial.read();

    // Обработка команд движения или сканирования
    if (commandType >= cmdStepXL && commandType <= cmdScan) {
      // Формат команды движения: 1 байт типа + 4 байта данных (steps) + 1 байт контрольной суммы
      byte buffer[5];
      for (byte i = 0; i < 5; i++) {
        buffer[i] = Serial.read();
      }
      byte receivedChecksum = buffer[4];
      byte calculatedChecksum = calculateChecksum(buffer, 4);

      if (receivedChecksum == calculatedChecksum) {
        // Безопасное преобразование 4 байтов из buffer в число типа uint32_t
        uint32_t steps = bytesToUint32(buffer);
        if (commandType == cmdScan) {
          processScanCommand(steps);
        } else {
          processMoveCommand(commandType, steps);
        }
      } else {
        Serial.println("Ошибка: неверная контрольная сумма для команды движения");
      }
    }
    // Обработка команды установки профиля движения
    /* else if (commandType == cmdSetProfile) {
      // Формат команды установки профиля: 1 байт типа + 12 байтов данных + 1 байт контрольной суммы
      if (Serial.available() >= 13) {
        byte buffer[12];
        for (int i = 0; i < 12; i++) {
          buffer[i] = Serial.read();
        }
        byte receivedChecksum = Serial.read();
        byte calculatedChecksum = calculateChecksum(buffer, 12);
  
        if (receivedChecksum == calculatedChecksum) {
          processSetProfileCommand(buffer);
        } else {
          Serial.println("Ошибка: неверная контрольная сумма для команды установки профиля");
        }
      }
    }
    else {
      Serial.println("Ошибка: неизвестный тип команды");
    }*/
  }
}
