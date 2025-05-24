namespace TaskManager.Contexts
{
    /// <summary>
    /// Singleton class representing the current user session.
    /// Stores information about the logged-in user such as username, employee code and admin status.
    /// </summary>
    public class UserSession
    {
        // Lazy initialization to ensure thread-safe singleton instance.
        private static readonly Lazy<UserSession> _instance = new(() => new UserSession());

        /// <summary>
        /// Gets the singleton instance of the UserSession.
        /// </summary>
        public static UserSession Instance => _instance.Value;

        /// <summary>
        /// Gets the username of the currently logged-in user.
        /// </summary>
        public string UserName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the employee code of the currently logged-in user.
        /// </summary>
        public string EmployeeCode { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the current user has admin privileges.
        /// </summary>
        public bool IsAdmin { get; private set; } = false;

        // Private constructor to prevent external instantiation.
        private UserSession() { }

        public void SetEmployeeForDebug(string code)
        {
            EmployeeCode = code;
        }

        /// <summary>
        /// Initializes the user session with the specified username, employee code, and admin status.
        /// </summary>
        /// <param name="userName">The username of the logged-in user.</param>
        /// <param name="employeeCode">The employee code of the logged-in user.</param>
        /// <param name="isAdmin">True if the user is an admin; otherwise, false.</param>
        public void Initialize(string userName, string employeeCode, bool isAdmin)
        {
            UserName = userName;
            EmployeeCode = employeeCode;
            IsAdmin = isAdmin;
        }

        /// <summary>
        /// Clears the user session, resetting the username, employee code, and admin status.
        /// </summary>
        public void Clear()
        {
            UserName = string.Empty;
            EmployeeCode = string.Empty;
            IsAdmin = false;
        }
    }
}
