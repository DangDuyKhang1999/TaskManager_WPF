
namespace TaskManager.Models
{
    /// <summary>
    /// Represents a user entity with authentication and profile information.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user (primary key).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique employee code for the user.
        /// </summary>
        public string EmployeeCode { get; set; } = "";

        /// <summary>
        /// Gets or sets the username used for login (unique).
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        public string PasswordHash { get; set; } = "";

        /// <summary>
        /// Gets or sets the display name of the user (optional).
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user (optional).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has administrative rights.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
