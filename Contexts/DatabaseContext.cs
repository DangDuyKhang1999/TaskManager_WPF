namespace TaskManager.Contexts
{
    /// <summary>
    /// Singleton class representing shared database context.
    /// Holds cached lists of users separated by role (normal users and admins).
    /// </summary>
    public class DatabaseContext
    {
        // Lazy initialization ensures thread-safe, single instance creation.
        private static readonly Lazy<DatabaseContext> _instance = new(() => new DatabaseContext());

        /// <summary>
        /// Gets the singleton instance of the DatabaseContext.
        /// </summary>
        public static DatabaseContext Instance => _instance.Value;

        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// Initializes lists for normal users and admins.
        /// </summary>
        private DatabaseContext()
        {
            NormalUsers = new List<string>();
            AdminUsers = new List<string>();
        }

        /// <summary>
        /// List of normal (non-admin) users cached in memory.
        /// </summary>
        public List<string> NormalUsers { get; private set; }

        /// <summary>
        /// List of admin users cached in memory.
        /// </summary>
        public List<string> AdminUsers { get; private set; }

        /// <summary>
        /// Loads the list of normal users by clearing the current list and adding new items.
        /// </summary>
        /// <param name="users">List of normal usernames to load.</param>
        public void LoadNormalUsers(List<string> users)
        {
            NormalUsers.Clear();
            NormalUsers.AddRange(users);
        }

        /// <summary>
        /// Loads the list of admin users by clearing the current list and adding new items.
        /// </summary>
        /// <param name="admins">List of admin usernames to load.</param>
        public void LoadAdminUsers(List<string> admins)
        {
            AdminUsers.Clear();
            AdminUsers.AddRange(admins);
        }
    }
}
