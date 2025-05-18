using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TaskManager.Services
{
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppLogs");
        private readonly string _logFilePath;
        private readonly object _lock = new();

        private Logger()
        {
            string fallbackFileName = "fallback_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
            _logFilePath = Path.Combine(_logDirectory, fallbackFileName);

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

                string fileName = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".log";
                _logFilePath = Path.Combine(_logDirectory, fileName);
            }
            catch
            {
                //Ignore exceptions here
            }
        }

        public void Info(string message, [CallerFilePath] string callerFilePath = "")
            => LogInternal("[INFO]", message, GetClassName(callerFilePath));

        public void Success(string message, [CallerFilePath] string callerFilePath = "")
            => LogInternal("[SUCCESS]", message, GetClassName(callerFilePath));

        public void Warning(string message, [CallerFilePath] string callerFilePath = "")
            => LogInternal("[WARNING]", message, GetClassName(callerFilePath));

        public void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            LogInternal("[ERROR]", message, className);
        }

        public void Error(Exception ex, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            string className = GetClassName(callerFilePath);
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                className += $".{callerMemberName}";

            string message = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            LogInternal("[ERROR]", message, className);
        }

        private void LogInternal(string level, string message, string className)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logLine = $"[{timestamp}][{className}]{level}: {message}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
            }
        }

        private string GetClassName(string? callerFilePath)
        {
            if (string.IsNullOrWhiteSpace(callerFilePath))
                return "Unknown";

            var fileName = Path.GetFileName(callerFilePath);
            fileName = fileName.Replace(".xaml.cs", "").Replace(".cs", "");

            return fileName;
        }
    }
}
