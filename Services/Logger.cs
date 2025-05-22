using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using TaskManager.Common;

namespace TaskManager.Services
{
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.AppText.FolderLog);
        private readonly string _logFilePath;
        private readonly BlockingCollection<string> _logQueue = new();
        private readonly Thread _logThread;

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

            // Start the background thread
            _logThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "LoggerThread"
            };
            _logThread.Start();
        }

        public void Information(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.Logging.Level_Information, message, GetClassName(callerFilePath));

        public void Success(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.Logging.Level_Success, message, GetClassName(callerFilePath));

        public void Warning(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.Logging.Level_Warning, message, GetClassName(callerFilePath));

        public void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            EnqueueLog(AppConstants.Logging.Level_Error, message, className);
        }

        public void Error(Exception ex, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            string message = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            EnqueueLog(AppConstants.Logging.Level_Error, message, className);
        }

        public void DatabaseSuccess(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.ExecutionStatus.DbSuccess, message, GetClassName(callerFilePath));

        public void DatabaseFailure(string message, [CallerFilePath] string callerFilePath = "")
            => EnqueueLog(AppConstants.ExecutionStatus.DbFailure, message, GetClassName(callerFilePath));

        private void EnqueueLog(string level, string message, string className)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logLine = $"[{timestamp}][{className}]{level}: {message}";
            _logQueue.Add(logLine); // Add to queue
        }

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
                }
            }
        }

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
        /// Should be called when application exits.
        /// </summary>
        public void Shutdown()
        {
            _logQueue.CompleteAdding();   // Signal that no more logs will be added
            _logThread.Join();            // Wait for thread to finish writing logs
        }
    }
}
