using System;
using System.Collections.Generic;

namespace CrystalTable.Logic
{
    /// <summary>
    /// Интерфейс для команд, поддерживающих отмену/повтор
    /// </summary>
    public interface ICommand
    {
        void Execute();  // Выполнить команду
        void Undo();     // Отменить команду
        string Description { get; } // Описание команды для отображения в интерфейсе
    }

    /// <summary>
    /// Менеджер истории команд для реализации функциональности Undo/Redo
    /// </summary>
    public class CommandHistory
    {
        // Стек для хранения выполненных команд (для отмены)
        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();

        // Стек для хранения отмененных команд (для повтора)
        private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

        // Максимальный размер истории для экономии памяти
        private const int MaxHistorySize = 100;

        /// <summary>
        /// Событие, возникающее при изменении состояния истории
        /// </summary>
        public event EventHandler HistoryChanged;

        /// <summary>
        /// Выполняет команду и добавляет её в историю
        /// </summary>
        /// <param name="command">Команда для выполнения</param>
        public void ExecuteCommand(ICommand command)
        {
            // Выполняем команду
            command.Execute();

            // Добавляем в стек отмены
            undoStack.Push(command);

            // Очищаем стек повтора (после новой команды нельзя делать redo)
            redoStack.Clear();

            // Ограничиваем размер истории
            if (undoStack.Count > MaxHistorySize)
            {
                // Удаляем самые старые команды
                var tempStack = new Stack<ICommand>();
                for (int i = 0; i < MaxHistorySize - 1; i++)
                {
                    tempStack.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    undoStack.Push(tempStack.Pop());
                }
            }

            // Уведомляем об изменении истории
            OnHistoryChanged();
        }

        /// <summary>
        /// Отменяет последнюю выполненную команду
        /// </summary>
        public void Undo()
        {
            if (CanUndo())
            {
                // Извлекаем последнюю команду
                var command = undoStack.Pop();

                // Отменяем её
                command.Undo();

                // Добавляем в стек повтора
                redoStack.Push(command);

                // Уведомляем об изменении
                OnHistoryChanged();
            }
        }

        /// <summary>
        /// Повторяет последнюю отмененную команду
        /// </summary>
        public void Redo()
        {
            if (CanRedo())
            {
                // Извлекаем команду из стека повтора
                var command = redoStack.Pop();

                // Выполняем её снова
                command.Execute();

                // Возвращаем в стек отмены
                undoStack.Push(command);

                // Уведомляем об изменении
                OnHistoryChanged();
            }
        }

        /// <summary>
        /// Проверяет, можно ли отменить команду
        /// </summary>
        public bool CanUndo() => undoStack.Count > 0;

        /// <summary>
        /// Проверяет, можно ли повторить команду
        /// </summary>
        public bool CanRedo() => redoStack.Count > 0;

        /// <summary>
        /// Получает описание последней команды для отмены
        /// </summary>
        public string GetUndoDescription()
        {
            return CanUndo() ? undoStack.Peek().Description : string.Empty;
        }

        /// <summary>
        /// Получает описание последней команды для повтора
        /// </summary>
        public string GetRedoDescription()
        {
            return CanRedo() ? redoStack.Peek().Description : string.Empty;
        }

        /// <summary>
        /// Очищает всю историю
        /// </summary>
        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            OnHistoryChanged();
        }

        /// <summary>
        /// Вызывает событие изменения истории
        /// </summary>
        private void OnHistoryChanged()
        {
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Команда для изменения выбранного кристалла
    /// </summary>
    public class SelectCrystalCommand : ICommand
    {
        private readonly int newIndex;
        private readonly int oldIndex;
        private readonly Action<int> setSelectedIndex;
        private readonly Action invalidateAction;

        public SelectCrystalCommand(int oldIndex, int newIndex,
            Action<int> setSelectedIndex, Action invalidateAction)
        {
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
            this.setSelectedIndex = setSelectedIndex;
            this.invalidateAction = invalidateAction;
        }

        public string Description => $"Выбор кристалла {newIndex}";

        public void Execute()
        {
            setSelectedIndex(newIndex);
            invalidateAction();
        }

        public void Undo()
        {
            setSelectedIndex(oldIndex);
            invalidateAction();
        }
    }

    /// <summary>
    /// Команда для группового выделения кристаллов
    /// </summary>
    public class MultiSelectCrystalsCommand : ICommand
    {
        private readonly HashSet<int> oldSelection;
        private readonly HashSet<int> newSelection;
        private readonly Action<HashSet<int>> setSelection;
        private readonly Action invalidateAction;

        public MultiSelectCrystalsCommand(HashSet<int> oldSelection,
            HashSet<int> newSelection, Action<HashSet<int>> setSelection,
            Action invalidateAction)
        {
            this.oldSelection = new HashSet<int>(oldSelection);
            this.newSelection = new HashSet<int>(newSelection);
            this.setSelection = setSelection;
            this.invalidateAction = invalidateAction;
        }

        public string Description =>
            $"Выделение {newSelection.Count} кристаллов";

        public void Execute()
        {
            setSelection(new HashSet<int>(newSelection));
            invalidateAction();
        }

        public void Undo()
        {
            setSelection(new HashSet<int>(oldSelection));
            invalidateAction();
        }
    }

    /// <summary>
    /// Команда для изменения параметров пластины
    /// </summary>
    public class ChangeWaferParametersCommand : ICommand
    {
        private readonly float oldSizeX;
        private readonly float oldSizeY;
        private readonly float oldDiameter;
        private readonly float newSizeX;
        private readonly float newSizeY;
        private readonly float newDiameter;
        private readonly Action<float, float, float> applyParameters;
        private readonly Action rebuildAction;

        public ChangeWaferParametersCommand(
            float oldSizeX, float oldSizeY, float oldDiameter,
            float newSizeX, float newSizeY, float newDiameter,
            Action<float, float, float> applyParameters,
            Action rebuildAction)
        {
            this.oldSizeX = oldSizeX;
            this.oldSizeY = oldSizeY;
            this.oldDiameter = oldDiameter;
            this.newSizeX = newSizeX;
            this.newSizeY = newSizeY;
            this.newDiameter = newDiameter;
            this.applyParameters = applyParameters;
            this.rebuildAction = rebuildAction;
        }

        public string Description => "Изменение параметров пластины";

        public void Execute()
        {
            applyParameters(newSizeX, newSizeY, newDiameter);
            rebuildAction();
        }

        public void Undo()
        {
            applyParameters(oldSizeX, oldSizeY, oldDiameter);
            rebuildAction();
        }
    }

    /// <summary>
    /// Составная команда для выполнения нескольких команд как одной
    /// </summary>
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> commands;
        private readonly string description;

        public CompositeCommand(string description)
        {
            this.description = description;
            this.commands = new List<ICommand>();
        }

        public string Description => description;

        /// <summary>
        /// Добавляет команду в составную команду
        /// </summary>
        public void AddCommand(ICommand command)
        {
            commands.Add(command);
        }

        public void Execute()
        {
            // Выполняем все команды в прямом порядке
            foreach (var command in commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            // Отменяем команды в обратном порядке
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                commands[i].Undo();
            }
        }
    }
}