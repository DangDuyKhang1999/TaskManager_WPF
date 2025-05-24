namespace TaskManager.Common
{
    /// <summary>
    /// Contains application-wide constants.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// UI text and message constants.
        /// </summary>
        public static class AppText
        {
            public const string MainWindowTitle = "Task Manager";
            public const string FolderLog = "AppLogs";

            // UI display messages
            public const string Message_LoginEmptyFields = "Username or password cannot be empty!";
            public const string Message_LoginInvalidCredentials = "Invalid username or password.";
            public const string Message_TaskSaveSuccess = "Task saved successfully.";
            public const string Message_TaskSaveFailed = "Failed to save the task. Please try again or contact support.";
            public const string Message_UserSaveSuccess = "User created successfully.";
            public const string Message_UserSaveFailed = "Failed to create user. Please try again or contact support.";
            public const string Message_UnexpectedError = "Unexpected error: ";
            public const string Message_TaskCodeExists = "This task code already exists. Please enter a unique code.";
            public const string Message_UsernameExists = "This username already exists. Please choose another.";

            /// <summary>
            /// Validation message constants.
            /// </summary>
            public static class ValidationMessages
            {
                public const string CodeRequired = "Code must not be empty";
                public const string TitleRequired = "Title must not be empty";
                public const string InvalidStatus = "Invalid status value";
                public const string InvalidPriority = "Invalid priority value";
                public const string ReporterRequired = "Reporter must be selected";
                public const string AssigneeRequired = "Assignee must be selected";

                public const string DisplayNameRequired = "Display name must not be empty";
                public const string UsernameRequired = "Username must not be empty";
                public const string PasswordRequired = "Password must not be empty";
                public const string RoleRequired = "Role must be selected";
                public const string CodeDuplicate = "This employee code already exists. Please enter a unique code.";
                public const string UsernameDuplicate = "This username already exists. Please choose another.";
            }
        }

        /// <summary>
        /// Constants related to database configuration and queries.
        /// </summary>
        public static class Database
        {
            public const string ConnectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;TrustServerCertificate=True;";
            public const string Query_GetUsersAndAdmins = "SELECT DisplayName, IsAdmin FROM Users WHERE IsActive = 1";
            public const string Query_GetAllUsers = "SELECT * FROM Users ORDER BY Id ASC";
            public const string Query_CheckEmployeeCode = "SELECT COUNT(1) FROM Users WHERE EmployeeCode = @EmployeeCode";
            public const string Query_CheckUsername = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            public const string Query_DeleteUserById = "DELETE FROM Users WHERE Id = @Id";

            public const string ReporterCodeNotFound = "Reporter employee code not found for: ";
            public const string AssigneeCodeNotFound = "Assignee employee code not found for: ";
        }

        /// <summary>
        /// Logging-related constants.
        /// </summary>
        public static class Logging
        {
            public const string Ext_Log = ".log";
            public const string Ext_Cs = ".cs";
            public const string Ext_XamlCs = ".xaml.cs";

            public const string FilePrefix_Fallback = "fallback_";
            public const string Information = "[INFO]";
            public const string Success = "[SUCCESS]";
            public const string Warning = "[WARN]";
            public const string Error = "[ERROR]";

            public const string TaskManagerStart = "           ************ Task Manager start ************";
            public const string TaskManagerEnd = "    ************ Task Manager end ************";

            public const string LoginFailed = "An error occurred during login. Please contact admin.";
            public const string LoginAuthFailed = "Authentication failed:";
            public const string BlankPadding = "                                              ";
        }

        /// <summary>
        /// Priority level labels used in the application.
        /// </summary>
        public static class PriorityLevels
        {
            public const string High = "High";
            public const string Medium = "Medium";
            public const string Low = "Low";
            public const string Unknown = "Unknown";
        }

        /// <summary>
        /// Task status values used in the application.
        /// </summary>
        public static class StatusValues
        {
            public const string NotStarted = "Not Started";
            public const string InProgress = "In Progress";
            public const string Completed = "Completed";
            public const string Unknown = "Unknown";
        }

        /// <summary>
        /// Execution status values for logging and tracking.
        /// </summary>
        public static class ExecutionStatus
        {
            public const string Success = "Success";
            public const string Error = "Error";
            public const string DbSuccess = "[DB_SUCCESS]";
            public const string DbFailure = "[DB_FAILURE]";
        }
    }
}
