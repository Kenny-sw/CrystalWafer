using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Состояние автообхода
    /// </summary>
    public enum ScanState
    {
        Idle,       // Ожидание
        Running,    // Выполняется
        Paused,     // Пауза
        Completed   // Завершён
    }

    /// <summary>
    /// Паттерн обхода кристаллов
    /// </summary>
    public enum ScanPattern
    {
        /// <summary>Змейка по строкам (↔)</summary>
        Serpentine,
        /// <summary>Построчно (→)</summary>
        Raster,
        /// <summary>По выделенным кристаллам</summary>
        SelectedOnly
    }

    /// <summary>
    /// Угол начала обхода
    /// </summary>
    public enum StartCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Настройки автообхода
    /// </summary>
    public class ScanSettings
    {
        /// <summary>Паттерн обхода</summary>
        public ScanPattern Pattern { get; set; } = ScanPattern.Serpentine;

        /// <summary>Угол начала</summary>
        public StartCorner StartFrom { get; set; } = StartCorner.TopLeft;

        /// <summary>Время паузы на кристалле (мс)</summary>
        public int DwellTimeMs { get; set; } = 500;

        /// <summary>Пропускать уже проверенные кристаллы</summary>
        public bool SkipInspected { get; set; } = false;

        /// <summary>Автоматически помечать как "Годен" после паузы</summary>
        public bool AutoMarkGood { get; set; } = false;
    }

    /// <summary>
    /// Аргументы события прогресса
    /// </summary>
    public class ScanProgressEventArgs : EventArgs
    {
        public int Current { get; }
        public int Total { get; }
        public Crystal CurrentCrystal { get; }
        public TimeSpan Elapsed { get; }
        public TimeSpan? Remaining { get; }

        public ScanProgressEventArgs(int current, int total, Crystal crystal, TimeSpan elapsed, TimeSpan? remaining)
        {
            Current = current;
            Total = total;
            CurrentCrystal = crystal;
            Elapsed = elapsed;
            Remaining = remaining;
        }
    }

    /// <summary>
    /// Контроллер автоматического обхода кристаллов
    /// </summary>
    public class ScanController
    {
        private readonly Form1 form;
        private List<Crystal> route;
        private int currentIndex;
        private CancellationTokenSource cts;
        private DateTime startTime;
        private ScanState state = ScanState.Idle;
        private readonly object stateLock = new object();

        /// <summary>Текущие настройки обхода</summary>
        public ScanSettings Settings { get; } = new ScanSettings();

        /// <summary>Текущее состояние</summary>
        public ScanState State
        {
            get { lock (stateLock) return state; }
            private set { lock (stateLock) state = value; }
        }

        /// <summary>Текущий индекс в маршруте</summary>
        public int CurrentIndex => currentIndex;

        /// <summary>Общее количество кристаллов в маршруте</summary>
        public int TotalCount => route?.Count ?? 0;

        /// <summary>Текущий кристалл</summary>
        public Crystal CurrentCrystal => route != null && currentIndex < route.Count ? route[currentIndex] : null;

        /// <summary>Событие: прогресс обхода</summary>
        public event EventHandler<ScanProgressEventArgs> ProgressChanged;

        /// <summary>Событие: достигнут кристалл</summary>
        public event EventHandler<Crystal> CrystalReached;

        /// <summary>Событие: обход завершён</summary>
        public event EventHandler ScanCompleted;

        /// <summary>Событие: состояние изменилось</summary>
        public event EventHandler<ScanState> StateChanged;

        public ScanController(Form1 form)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form));
        }

        /// <summary>
        /// Построить маршрут обхода
        /// </summary>
        public List<Crystal> BuildRoute(HashSet<int> selectedIndices = null)
        {
            var crystals = CrystalManager.Instance.Crystals;
            if (crystals == null || crystals.Count == 0)
                return new List<Crystal>();

            IEnumerable<Crystal> source = crystals;

            // Фильтр по выделению
            if (Settings.Pattern == ScanPattern.SelectedOnly && selectedIndices != null && selectedIndices.Count > 0)
            {
                source = crystals.Where(c => selectedIndices.Contains(c.Index));
            }

            // Фильтр по проверенным
            if (Settings.SkipInspected)
            {
                source = source.Where(c => c.Bin == BinCategory.NotInspected);
            }

            // Группировка по строкам
            var rows = source
                .GroupBy(c => Math.Round(c.RealY, 1))
                .OrderBy(g => g.Key)
                .ToList();

            // Направление начала
            bool reverseRows = Settings.StartFrom == StartCorner.BottomLeft || Settings.StartFrom == StartCorner.BottomRight;
            bool reverseFirstRow = Settings.StartFrom == StartCorner.TopRight || Settings.StartFrom == StartCorner.BottomRight;

            if (reverseRows)
            {
                rows.Reverse();
            }

            var result = new List<Crystal>();
            bool reverseCurrentRow = reverseFirstRow;

            foreach (var row in rows)
            {
                var rowCrystals = row.OrderBy(c => c.RealX).ToList();
                
                if (reverseCurrentRow)
                {
                    rowCrystals.Reverse();
                }

                result.AddRange(rowCrystals);

                // Змейка: чередуем направление
                if (Settings.Pattern == ScanPattern.Serpentine)
                {
                    reverseCurrentRow = !reverseCurrentRow;
                }
            }

            return result;
        }

        /// <summary>
        /// Начать автообход
        /// </summary>
        public async Task StartAsync(HashSet<int> selectedIndices = null)
        {
            if (State == ScanState.Running)
                return;

            route = BuildRoute(selectedIndices);
            if (route.Count == 0)
            {
                AppLogger.Warning("Автообход: маршрут пуст");
                return;
            }

            currentIndex = 0;
            startTime = DateTime.Now;
            cts = new CancellationTokenSource();
            State = ScanState.Running;
            OnStateChanged(State);

            AppLogger.Info($"Автообход запущен: {route.Count} кристаллов, паттерн: {Settings.Pattern}");

            try
            {
                await RunScanLoopAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info("Автообход отменён");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка автообхода", ex);
            }
            finally
            {
                if (State == ScanState.Running)
                {
                    State = ScanState.Completed;
                    OnStateChanged(State);
                    ScanCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Приостановить обход
        /// </summary>
        public void Pause()
        {
            if (State == ScanState.Running)
            {
                State = ScanState.Paused;
                OnStateChanged(State);
                AppLogger.Info("Автообход приостановлен");
            }
        }

        /// <summary>
        /// Продолжить обход
        /// </summary>
        public void Resume()
        {
            if (State == ScanState.Paused)
            {
                State = ScanState.Running;
                OnStateChanged(State);
                AppLogger.Info("Автообход продолжен");
            }
        }

        /// <summary>
        /// Остановить обход
        /// </summary>
        public void Stop()
        {
            cts?.Cancel();
            State = ScanState.Idle;
            OnStateChanged(State);
            AppLogger.Info("Автообход остановлен");
        }

        /// <summary>
        /// Перейти к следующему кристаллу вручную
        /// </summary>
        public void GoToNext()
        {
            if (route == null || currentIndex >= route.Count - 1)
                return;

            currentIndex++;
            OnCrystalReached(CurrentCrystal);
            OnProgressChanged();
        }

        /// <summary>
        /// Перейти к предыдущему кристаллу вручную
        /// </summary>
        public void GoToPrevious()
        {
            if (route == null || currentIndex <= 0)
                return;

            currentIndex--;
            OnCrystalReached(CurrentCrystal);
            OnProgressChanged();
        }

        private async Task RunScanLoopAsync(CancellationToken token)
        {
            while (currentIndex < route.Count)
            {
                token.ThrowIfCancellationRequested();

                // Ожидание если на паузе
                while (State == ScanState.Paused)
                {
                    await Task.Delay(100, token);
                }

                var crystal = route[currentIndex];

                // Перемещение к кристаллу
                bool moved = await form.MovePointerToAsync(crystal.RealX, crystal.RealY);
                if (!moved && !form.DebugModeWithoutComPort)
                {
                    AppLogger.Warning($"Не удалось переместиться к кристаллу #{crystal.Index}");
                }

                // Уведомление о достижении кристалла
                OnCrystalReached(crystal);
                OnProgressChanged();

                // Пауза на кристалле
                if (Settings.DwellTimeMs > 0)
                {
                    await Task.Delay(Settings.DwellTimeMs, token);
                }

                // Автоматическая пометка
                if (Settings.AutoMarkGood && crystal.Bin == BinCategory.NotInspected)
                {
                    crystal.SetBin(BinCategory.Good);
                }

                currentIndex++;
            }
        }

        private void OnProgressChanged()
        {
            var elapsed = DateTime.Now - startTime;
            TimeSpan? remaining = null;

            if (currentIndex > 0)
            {
                var avgPerCrystal = elapsed.TotalMilliseconds / currentIndex;
                var remainingMs = avgPerCrystal * (route.Count - currentIndex);
                remaining = TimeSpan.FromMilliseconds(remainingMs);
            }

            ProgressChanged?.Invoke(this, new ScanProgressEventArgs(
                currentIndex + 1,
                route.Count,
                CurrentCrystal,
                elapsed,
                remaining));
        }

        private void OnCrystalReached(Crystal crystal)
        {
            CrystalReached?.Invoke(this, crystal);
        }

        private void OnStateChanged(ScanState newState)
        {
            StateChanged?.Invoke(this, newState);
        }
    }
}
