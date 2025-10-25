using System;

namespace CrystalTable.Models
{
    public enum MapState
    {
        NoMap,          // Нет активной карты
        Creating,       // Создание новой карты
        Running,        // Выполняется работа с пластиной
        Completed,      // Работа с пластиной завершена
        Archived        // Карта архивирована
    }
}