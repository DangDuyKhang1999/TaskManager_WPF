
namespace TaskManager.Contexts
{
    /// <summary>
    /// Singleton class that represents the application's database context.
    /// It holds shared data such as the list of usernames loaded from the database.
    /// </summary>
    public class DatabaseContext
    {
        // Lazy initialization to ensure thread-safe singleton instance.
        private static readonly Lazy<DatabaseContext> _instance = new(() => new DatabaseContext());

        /// <summary>
        /// Gets the singleton instance of the DatabaseContext.
        /// </summary>
        public static DatabaseContext Instance => _instance.Value;

        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// Initializes the UserNames list.
        /// </summary>
        private DatabaseContext()
        {
            UserNames = new List<string>();
        }

        /// <summary>
        /// Gets the list of usernames loaded in memory.
        /// </summary>
        public List<string> UserNames { get; private set; }

        /// <summary>
        /// Loads the list of usernames into the context by clearing the current list and adding the new usernames.
        /// </summary>
        /// <param name="userNames">The list of usernames to load.</param>
        public void LoadUserNames(List<string> userNames)
        {
            UserNames.Clear();
            UserNames.AddRange(userNames);
        }
    }
}
