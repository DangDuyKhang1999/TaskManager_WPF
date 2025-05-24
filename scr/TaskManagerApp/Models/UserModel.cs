namespace TaskManagerApp.Models
{
    /// <summary>
    /// Represents a user entity with authentication and profile information.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique employee code.
        /// </summary>
        public string EmployeeCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the login username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name (optional).
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address (optional).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating administrative rights.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time of the account.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
