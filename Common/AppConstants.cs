namespace TaskManager.Common
{
    public static class AppConstants
    {
        public static class AppText
        {
            public const string MainWindowTitle = "Task Manager";
            // Folder
            public const string FolderLog = "AppLogs";
        }
        public static class Database
        {
            public const string ConnectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";
        }

        public static class Logging
        {

            // Extensions
            public const string Ext_Log = ".log";
            public const string Ext_Cs = ".cs";
            public const string Ext_XamlCs = ".xaml.cs";

            // File Prefix
            public const string FilePrefix_Fallback = "fallback_";

            // Log Levels
            public const string Level_Info = "[INFO]";
            public const string Level_Success = "[SUCCESS]";
            public const string Level_Warning = "[WARNING]";
            public const string Level_Error = "[ERROR]";

            // Messages
            public const string Message_TaskManagerStart = "***** Task Manager start *****";
            public const string Message_TaskManagerEnd = "***** Task Manager end *****";
            public const string Message_UnexpectedError = "Unexpected error";

            // Login-related Warnings/Errors
            public const string Message_LoginEmptyFields = "Username or password cannot be empty!";
            public const string Message_LoginInvalidCredentials = "Invalid username or password.";
            public const string Message_LoginFailed = "An error occurred during login. Please contact admin.";
            public const string Message_LoginAuthFailed = "Authentication failed:";
        }

        public static class PriorityLevels
        {
            public const string High = "High";
            public const string Medium = "Medium";
            public const string Low = "Low";
            public const string Unknown = "Unknown";
        }

        public static class StatusValues
        {
            public const string NotStarted = "Not Started";
            public const string InProgress = "In Progress";
            public const string Completed = "Completed";
            public const string Unknown = "Unknown";
        }
    }
}
