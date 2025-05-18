namespace TaskManager.Contexts
{
    public class DatabaseContext
    {
        private static readonly Lazy<DatabaseContext> _instance = new(() => new DatabaseContext());

        public static DatabaseContext Instance => _instance.Value;

        private DatabaseContext()
        {
            NormalUsersList = new List<string>();
            AdminUsersList = new List<string>();
        }

        public List<string> NormalUsersList { get; private set; }
        public List<string> AdminUsersList { get; private set; }

        public void LoadNormalUsers(List<string> users)
        {
            NormalUsersList.Clear();
            NormalUsersList.AddRange(users);
        }

        public void LoadAdminUsers(List<string> admins)
        {
            AdminUsersList.Clear();
            AdminUsersList.AddRange(admins);
        }
    }
}
