using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CrystalTable.Logic
{
    public enum AppLogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public sealed class AppLogEntry
    {
        public AppLogEntry(AppLogLevel level, string message, DateTime timestamp, Exception exception)
        {
            Level = level;
            Message = message ?? string.Empty;
            Timestamp = timestamp;
            Exception = exception;
        }

        public AppLogLevel Level { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public Exception Exception { get; }
    }

    public sealed class AppLogEventArgs : EventArgs
    {
        public AppLogEventArgs(AppLogEntry entry)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        public AppLogEntry Entry { get; }
    }

    /// <summary>
    /// Thread-safe application file logger with optional event notifications.
    /// </summary>
    public static class AppLogger
    {
        private static readonly object SyncRoot = new object();
        private static readonly string LogFilePath;

        private static AppLogLevel minimumLevel = AppLogLevel.Debug;

        public static event EventHandler<AppLogEventArgs> LogMessagePublished;

        static AppLogger()
        {
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var logDirectory = Path.Combine(basePath, "logs");
                Directory.CreateDirectory(logDirectory);
                var fileName = $"app_{DateTime.Now:yyyyMMdd}.log";
                LogFilePath = Path.Combine(logDirectory, fileName);
            }
            catch
            {
                LogFilePath = string.Empty;
            }
        }

        public static AppLogLevel MinimumLevel
        {
            get => minimumLevel;
            set => minimumLevel = value;
        }

        public static void Trace(string message) => Write(AppLogLevel.Trace, message);

        public static void Debug(string message) => Write(AppLogLevel.Debug, message);

        public static void Info(string message) => Write(AppLogLevel.Info, message);

        public static void Warning(string message, Exception exception = null) => Write(AppLogLevel.Warning, message, exception);

        public static void Error(string message, Exception exception = null) => Write(AppLogLevel.Error, message, exception);

        private static void Write(AppLogLevel level, string message, Exception exception = null)
        {
            if (level < minimumLevel)
            {
                return;
            }

            var timestamp = DateTime.Now;
            var entry = new AppLogEntry(level, message ?? string.Empty, timestamp, exception);

            if (!string.IsNullOrEmpty(LogFilePath))
            {
                try
                {
                    var builder = new StringBuilder();
                    builder.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}", timestamp, level, entry.Message);
                    builder.AppendLine();
                    if (exception != null)
                    {
                        builder.AppendLine(exception.ToString());
                    }

                    lock (SyncRoot)
                    {
                        File.AppendAllText(LogFilePath, builder.ToString(), Encoding.UTF8);
                    }
                }
                catch
                {
                    // Logging should never break the main workflow.
                }
            }

            try
            {
                System.Diagnostics.Trace.WriteLine($"{timestamp:HH:mm:ss.fff} [{level}] {entry.Message}");
                if (exception != null)
                {
                    System.Diagnostics.Trace.WriteLine(exception.ToString());
                }
            }
            catch
            {
                // Ignore Trace writing failures.
            }

            var handler = LogMessagePublished;
            if (handler != null)
            {
                try
                {
                    handler.Invoke(null, new AppLogEventArgs(entry));
                }
                catch
                {
                    // Observers are not allowed to break logging.
                }
            }
        }
    }
}
