using System;
using System.Collections.Generic;

namespace CrystalTable.Logic
{
    /// <summary>Интерфейс для команд, поддерживающих отмену/повтор</summary>
    public interface ICommand
    {
        void Execute();   // Выполнить команду
        void Undo();      // Отменить команду
        string Description { get; } // Описание команды для UI
    }

    /// <summary>Менеджер истории команд для Undo/Redo</summary>
    public class CommandHistory
    {
        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

        private const int MaxHistorySize = 100;

        /// <summary>Событие при изменении состояния истории</summary>
        public event EventHandler HistoryChanged;

        /// <summary>Выполняет команду и добавляет её в историю</summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null) return;

            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();

            // Ограничение размера истории
            if (undoStack.Count > MaxHistorySize)
            {
                var temp = new Stack<ICommand>();
                for (int i = 0; i < MaxHistorySize - 1; i++)
                    temp.Push(undoStack.Pop());
                undoStack.Clear();
                while (temp.Count > 0)
                    undoStack.Push(temp.Pop());
            }

            OnHistoryChanged();
        }

        /// <summary>Отменяет последнюю выполненную команду</summary>
        public void Undo()
        {
            if (CanUndo())
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
                OnHistoryChanged();
            }
        }

        /// <summary>Повторяет последнюю отменённую команду</summary>
        public void Redo()
        {
            if (CanRedo())
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
                OnHistoryChanged();
            }
        }

        public bool CanUndo() => undoStack.Count > 0;
        public bool CanRedo() => redoStack.Count > 0;

        public string GetUndoDescription() => CanUndo() ? undoStack.Peek().Description : string.Empty;
        public string GetRedoDescription() => CanRedo() ? redoStack.Peek().Description : string.Empty;

        /// <summary>Очищает всю историю</summary>
        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            OnHistoryChanged();
        }

        private void OnHistoryChanged() => HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Команда: выбор одного кристалла</summary>
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

    /// <summary>Команда: групповое выделение кристаллов</summary>
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

        public string Description => $"Выделение {newSelection.Count} кристаллов";

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

    /// <summary>Команда: изменение параметров пластины</summary>
    public class ChangeWaferParametersCommand : ICommand
    {
        private readonly float oldSizeX, oldSizeY, oldDiameter;
        private readonly float newSizeX, newSizeY, newDiameter;
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

    /// <summary>Составная команда: несколько команд как одна</summary>
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> commands = new List<ICommand>();
        private readonly string description;

        public CompositeCommand(string description) => this.description = description;

        public string Description => description;

        public void AddCommand(ICommand command) => commands.Add(command);

        public void Execute()
        {
            foreach (var cmd in commands)
                cmd.Execute();
        }

        public void Undo()
        {
            for (int i = commands.Count - 1; i >= 0; i--)
                commands[i].Undo();
        }
    }
}
