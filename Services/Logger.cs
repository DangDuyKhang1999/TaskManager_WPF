using System.IO;
using System.Runtime.CompilerServices;
using TaskManager.Common;
using TaskManager.Services;

namespace TaskManager.Services
{
    /// <summary>
    /// Singleton Logger class to log messages to daily log files with
    /// retention of max 5 log files. Supports different log levels.
    /// </summary>
    public class Logger
    {
        // Lazy singleton instance to ensure thread-safe, lazy initialization
        private static readonly Lazy<Logger> _instance = new(() => new Logger());

        /// <summary>
        /// Gets the singleton instance of the Logger.
        /// </summary>
        public static Logger Instance => _instance.Value;

        // Directory to store log files
        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.AppText.FolderLog);
        // Current log file path
        private readonly string _logFilePath;
        // Lock object for thread-safe file writing
        private readonly object _lock = new();

        /// <summary>
        /// Private constructor initializes the log directory and log file.
        /// It keeps only the latest 5 log files by deleting older ones.
        /// </summary>
        private Logger()
        {
            // Fallback log file name with timestamp in case of error during initialization
            string fallbackFileName = AppConstants.Logging.FilePrefix_Fallback + DateTime.Now.ToString("yyyyMMdd_HHmmss") + AppConstants.Logging.Ext_Log;
            _logFilePath = Path.Combine(_logDirectory, fallbackFileName);

            try
            {
                // Create log directory if it does not exist
                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);

                // Get all existing log files sorted by creation time (oldest first)
                var logFiles = new DirectoryInfo(_logDirectory)
                    .GetFiles("*.log")
                    .OrderBy(f => f.CreationTime)
                    .ToList();

                // Delete oldest files if more than or equal to 5 logs exist
                while (logFiles.Count >= 5)
                {
                    logFiles[0].Delete();
                    logFiles.RemoveAt(0);
                }

                // Create new log file with current timestamp
                string fileName = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + AppConstants.Logging.Ext_Log;
                _logFilePath = Path.Combine(_logDirectory, fileName);
            }
            catch
            {
                // Suppress exceptions during logger initialization to avoid app crash
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public void Info(string message, [CallerFilePath] string callerFilePath = "")
            => LogInternal(AppConstants.Logging.Level_Info, message, GetClassName(callerFilePath));

        /// <summary>
        /// Logs a success message.
        /// </summary>
        public void Success(string message, [CallerFilePath] string callerFilePath = "")
            => LogInternal(AppConstants.Logging.Level_Success, message, GetClassName(callerFilePath));

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void Warning(string message, [CallerFilePath] string callerFilePath = "")
            => LogInternal(AppConstants.Logging.Level_Warning, message, GetClassName(callerFilePath));

        /// <summary>
        /// Logs an error message with optional caller method name.
        /// </summary>
        public void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            LogInternal(AppConstants.Logging.Level_Error, message, className);
        }

        /// <summary>
        /// Logs an exception's message and stack trace with optional caller method name.
        /// </summary>
        public void Error(Exception ex, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            string message = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            LogInternal(AppConstants.Logging.Level_Error, message, className);
        }

        /// <summary>
        /// Internal method to format and write log lines to file in a thread-safe manner.
        /// </summary>
        private void LogInternal(string level, string message, string className)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logLine = $"[{timestamp}][{className}]{level}: {message}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
            }
        }

        /// <summary>
        /// Extracts the class name from the full caller file path.
        /// Removes ".cs" or ".xaml.cs" extensions.
        /// </summary>
        private string GetClassName(string? callerFilePath)
        {
            if (string.IsNullOrWhiteSpace(callerFilePath))
                return "Unknown";

            var fileName = Path.GetFileName(callerFilePath);
            fileName = fileName.Replace(AppConstants.Logging.Ext_XamlCs, string.Empty).Replace(AppConstants.Logging.Ext_Cs, string.Empty);

            return fileName;
        }
    }
}
