using System;
using System.Collections.Generic;

namespace TaskManagerApp.Contexts
{
    /// <summary>
    /// Provides a singleton context for storing lists of normal and admin users in memory.
    /// </summary>
    public class DatabaseContext
    {
        private static readonly Lazy<DatabaseContext> _instance = new(() => new DatabaseContext());

        /// <summary>
        /// Gets the singleton instance of the <see cref="DatabaseContext"/>.
        /// </summary>
        public static DatabaseContext Instance => _instance.Value;

        // Private constructor to enforce singleton pattern
        private DatabaseContext()
        {
            NormalUsersList = new List<string>();
            AdminUsersList = new List<string>();
        }

        /// <summary>
        /// Gets the list of normal users.
        /// </summary>
        public List<string> NormalUsersList { get; private set; }

        /// <summary>
        /// Gets the list of admin users.
        /// </summary>
        public List<string> AdminUsersList { get; private set; }

        /// <summary>
        /// Loads a list of normal users into the context, replacing any existing entries.
        /// </summary>
        /// <param name="users">The list of normal users.</param>
        public void LoadNormalUsers(List<string> users)
        {
            NormalUsersList.Clear();
            NormalUsersList.AddRange(users);
        }

        /// <summary>
        /// Loads a list of admin users into the context, replacing any existing entries.
        /// </summary>
        /// <param name="admins">The list of admin users.</param>
        public void LoadAdminUsers(List<string> admins)
        {
            AdminUsersList.Clear();
            AdminUsersList.AddRange(admins);
        }
    }
}
