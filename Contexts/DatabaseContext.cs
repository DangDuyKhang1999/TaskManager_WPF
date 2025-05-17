using System;
using System.Collections.Generic;

namespace TaskManager.Contexts
{
    public class DatabaseContext
    {
        private static readonly Lazy<DatabaseContext> _instance = new(() => new DatabaseContext());
        public static DatabaseContext Instance => _instance.Value;

        private DatabaseContext()
        {
            UserNames = new List<string>();
        }

        public List<string> UserNames { get; private set; }

        public void LoadUserNames(List<string> userNames)
        {
            UserNames.Clear();
            UserNames.AddRange(userNames);
        }
    }
}
