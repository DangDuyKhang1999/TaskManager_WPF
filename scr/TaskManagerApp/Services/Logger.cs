using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using TaskManagerApp.Common;

namespace TaskManagerApp.Services
{
    /// <summary>
    /// Provides thread-safe logging functionality with background file writing.
    /// Implements a singleton pattern.
    /// </summary>
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.AppText.FolderLog);
        private readonly string _logFilePath;
        private readonly BlockingCollection<string> _logQueue = new();
        private readonly Thread _logThread;

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// Creates log directory and file, starts background thread for log writing.
        /// </summary>
        private Logger()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);

                var logFiles = new DirectoryInfo(_logDirectory)
                    .GetFiles("*.log")
                    .OrderBy(f => f.CreationTime)
                    .ToList();

                while (logFiles.Count >= 5)
                {
                    logFiles[0].Delete();
                    logFiles.RemoveAt(0);
                }

                string fileName = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + AppConstants.Logging.Ext_Log;
                _logFilePath = Path.Combine(_logDirectory, fileName);
            }
            catch
            {
                string fallbackFileName = AppConstants.Logging.FilePrefix_Fallback + DateTime.Now.ToString("yyyyMMdd_HHmmss") + AppConstants.Logging.Ext_Log;
                _logFilePath = Path.Combine(_logDirectory, fallbackFileName);
            }

            _logThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "LoggerThread"
            };
            _logThread.Start();
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public void Information(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.Logging.Information, message, GetClassName(callerFilePath));

        /// <summary>
        /// Logs a success message.
        /// </summary>
        public void Success(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.Logging.Success, message, GetClassName(callerFilePath));

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void Warning(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.Logging.Warning, message, GetClassName(callerFilePath));

        /// <summary>
        /// Logs an error message with optional member name.
        /// </summary>
        public void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            EnqueueLog(AppConstants.Logging.Error, message, className);
        }

        /// <summary>
        /// Logs an exception with stack trace.
        /// </summary>
        public void Error(Exception ex, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            string message = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            EnqueueLog(AppConstants.Logging.Error, message, className);
        }

        /// <summary>
        /// Adds a formatted log entry to the queue.
        /// </summary>
        private void EnqueueLog(string level, string message, string className)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logLine = $"[{timestamp}][{className}]{level}: {message}";
            _logQueue.Add(logLine);
        }

        /// <summary>
        /// Background thread method to write logs from queue to file.
        /// </summary>
        private void ProcessLogQueue()
        {
            foreach (var logLine in _logQueue.GetConsumingEnumerable())
            {
                try
                {
                    File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                }
                catch
                {
                    // Suppress exceptions during logging to avoid recursive errors
                }
            }
        }

        /// <summary>
        /// Extracts the class name from the caller file path.
        /// </summary>
        private string GetClassName(string? callerFilePath)
        {
            if (string.IsNullOrWhiteSpace(callerFilePath))
                return "Unknown";

            var fileName = Path.GetFileName(callerFilePath);
            return fileName
                .Replace(AppConstants.Logging.Ext_XamlCs, string.Empty)
                .Replace(AppConstants.Logging.Ext_Cs, string.Empty);
        }

        /// <summary>
        /// Gracefully shuts down the logger and flushes remaining logs.
        /// Should be called on application exit.
        /// </summary>
        public void Shutdown()
        {
            _logQueue.CompleteAdding();
            _logThread.Join();
        }
    }
}
