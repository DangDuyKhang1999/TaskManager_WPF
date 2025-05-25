# Task Manager Application

A task management application developed with C# (.NET 8) and WPF using the MVVM pattern. Data is stored in SQL Server.

---

## Requirements

- .NET 8 SDK or higher  
- SQL Server or SQL Server Express  
- Visual Studio 2022 or later (to run the WPF project)

---

## Installation Guide

### 1. Create the Database and Tables

- Open **SQL Server Management Studio** (or any equivalent tool).
- Run the script below to create the database and tables:

```sql
CREATE DATABASE TaskManagerDB;
GO
USE TaskManagerDB;
GO

CREATE TABLE [dbo].[Users] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EmployeeCode NVARCHAR(20) NOT NULL UNIQUE,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(100) NULL,
    Email NVARCHAR(100) NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [dbo].[Tasks] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status INT NOT NULL DEFAULT 0,
    DueDate DATETIME NULL,
    Priority INT NOT NULL DEFAULT 1,
    ReporterId NVARCHAR(20) NOT NULL,
    AssigneeId NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Tasks_Assignee FOREIGN KEY (AssigneeId) REFERENCES [dbo].[Users] (EmployeeCode),
    CONSTRAINT FK_Tasks_Reporter FOREIGN KEY (ReporterId) REFERENCES [dbo].[Users] (EmployeeCode)
);
```

---

## How to Run the Application

### Run via Command Line

```bash
dotnet run --project TaskManagerApp
```

### Or Run via Visual Studio

- Open the `.sln` file
- Press **Start (F5)** to run the application

---

## Key Features

- Add / Edit / Delete tasks
- User management (Admin only)
- User-friendly WPF interface
- User roles: Admin vs Regular

---

## Features That Can Be Further Developed

- No need to log in every time the app is opened: Save token/session after login; auto-authenticate on startup  
- Add Logout functionality: Clear session/token; navigate back to login screen  
- Real-time database update notifications: Use SignalR to show notifications (snackbar/toast) when data changes from the server  
- Edit user information  
- Convert `TaskManagerSignalRHub` from `.exe` to `.dll` and publish as a single executable  
- Embed the database instead of creating a new one manually  
- In the ComboBox for selecting users, automatically show at most 5 users that best match the input characters instead of displaying the entire user list
 

---

## Notes

- Make sure **SQL Server is running** and the **database has been created** before starting the application.
- If you have sample data, you can use `INSERT INTO` statements in the script above.

---

## Contact

Email: dangduykhang7999@gmail.com
