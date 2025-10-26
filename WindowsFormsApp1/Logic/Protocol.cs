using System;

namespace CrystalTable.Logic
{
    public static class Protocol
    {
        public static class Commands
        {
            public const byte MoveLeft = 0x01;
            public const byte MoveRight = 0x02;
            public const byte MoveUp = 0x03;
            public const byte MoveDown = 0x04;
            public const byte Lock = 0x05;      // Фиксация (HIGH на пине)
            public const byte Unlock = 0x06;    // Сброс (LOW на пине)
        }

        public static class Events
        {
            public const string Prefix = "EV";

            public static class Sensor
            {
                public const string On = "EV S:1";
                public const string Off = "EV S:0";
            }
            
            public static class Lock
            {
                public const string Locked = "EV L:1";
                public const string Unlocked = "EV L:0";
            }

            public static bool IsEvent(string message) =>
                !string.IsNullOrWhiteSpace(message) &&
                message.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);

            public static bool TryParseSensorEvent(string message, out bool isOn)
            {
                isOn = false;
                if (string.IsNullOrWhiteSpace(message))
                {
                    return false;
                }

                if (string.Equals(message, Sensor.On, StringComparison.OrdinalIgnoreCase))
                {
                    isOn = true;
                    return true;
                }

                if (string.Equals(message, Sensor.Off, StringComparison.OrdinalIgnoreCase))
                {
                    isOn = false;
                    return true;
                }

                return false;
            }
            
            public static bool TryParseLockEvent(string message, out bool isLocked)
            {
                isLocked = false;
                if (string.IsNullOrWhiteSpace(message))
                {
                    return false;
                }

                if (string.Equals(message, Lock.Locked, StringComparison.OrdinalIgnoreCase))
                {
                    isLocked = true;
                    return true;
                }

                if (string.Equals(message, Lock.Unlocked, StringComparison.OrdinalIgnoreCase))
                {
                    isLocked = false;
                    return true;
                }

                return false;
            }
        }

        public static class Responses
        {
            public const string Ok = "OK";
            public const string ErrorPrefix = "ERR";

            public static bool IsOk(string message) =>
                string.Equals(message, Ok, StringComparison.OrdinalIgnoreCase);

            public static bool IsError(string message) =>
                !string.IsNullOrWhiteSpace(message) &&
                message.StartsWith(ErrorPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static class Timeouts
        {
            public static readonly TimeSpan Command = TimeSpan.FromSeconds(3);
        }
        
        /// <summary>
        /// Проверка направления движения для команды
        /// </summary>
        public static class DirectionCheck
        {
            /// <summary>
            /// Возвращает true если команда движения влево (требует HIGH на пине направления)
            /// </summary>
            public static bool IsLeftDirection(byte command)
            {
                return command == Commands.MoveLeft;
            }
            
            /// <summary>
            /// Возвращает true если команда движения вправо (требует LOW на пине направления)
            /// </summary>
            public static bool IsRightDirection(byte command)
            {
                return command == Commands.MoveRight;
            }
        }
    }
}
