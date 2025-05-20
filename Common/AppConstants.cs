namespace TaskManager.Common
{
    public static class AppConstants
    {
        public static class AppText
        {
            public const string MainWindowTitle = "Task Manager";
            public const string FolderLog = "AppLogs";

            // Message for UI
            public const string Message_LoginEmptyFields = "Username or password cannot be empty!";
            public const string Message_LoginInvalidCredentials = "Invalid username or password.";

            // Validation messages
            public static class ValidationMessages
            {
                public const string CodeRequired = "Code must not be empty";
                public const string TitleRequired = "Title must not be empty";
                public const string InvalidStatus = "Invalid status value";
                public const string InvalidPriority = "Invalid priority value";
                public const string ReporterRequired = "Reporter must be selected";
                public const string AssigneeRequired = "Assignee must be selected";
            }
        }

        public static class Database
        {
            public const string ConnectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";
        }

        public static class Logging
        {
            public const string Ext_Log = ".log";
            public const string Ext_Cs = ".cs";
            public const string Ext_XamlCs = ".xaml.cs";

            public const string FilePrefix_Fallback = "fallback_";
            public const string Level_Information = "[INFO]";
            public const string Level_Success = "[SUCCESS]";
            public const string Level_Warning = "[WARN]";
            public const string Level_Error = "[ERROR]";

            public const string Message_TaskManagerStart = "***** Task Manager start *****";
            public const string Message_TaskManagerEnd = "***** Task Manager end *****";
            public const string Message_UnexpectedError = "Unexpected error";

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
