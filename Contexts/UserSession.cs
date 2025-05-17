using System;

namespace TaskManager.Contexts
{
    internal class UserSession
    {
        private static readonly Lazy<UserSession> _instance = new(() => new UserSession());

        public static UserSession Instance => _instance.Value;

        public string UserName { get; private set; } = string.Empty;
        public bool IsAdmin { get; private set; } = false;

        private UserSession() { }

        public void Initialize(string userName, bool isAdmin)
        {
            UserName = userName;
            IsAdmin = isAdmin;
        }

        public void Clear()
        {
            UserName = string.Empty;
            IsAdmin = false;
        }
    }
}
