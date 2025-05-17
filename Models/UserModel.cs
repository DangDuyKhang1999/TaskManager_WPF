using System;

namespace TaskManager.Models
{
    public class UserModel
    {
        public int Id { get; set; } // Primary key
        public string EmployeeCode { get; set; } = ""; // Employee code (unique)
        public string Username { get; set; } = ""; // Login name (unique)
        public string PasswordHash { get; set; } = ""; // Password (hash)
        public string? DisplayName { get; set; } // Display name
        public string? Email { get; set; } // Email
        public bool IsAdmin { get; set; } // Admin rights
        public bool IsActive { get; set; } // Active status
        public DateTime CreatedAt { get; set; } // Creation date
    }
}