# Task List Management Application
The Task Manager project is a task management application built on the WPF platform using the MVVM pattern. The backend uses ASP.NET Core and SignalR to update data in real-time.  
The application includes login, user role management, task and user management, and password hashing.

## 1. Technologies Used

- **Frontend**: C# (.NET 6+), WPF (MVVM)  
- **Backend**: ASP.NET Core Web API  
- **Realtime**: SignalR  
- **Database**: SQL Server  
- **UI/UX**: Pure WPF  

---

## 2. Main Features

### 2.1 Login

- Startup screen: requires login to classify users and display tasks accordingly  
- Role-based access:  
  - **Admin**: full management rights  
  - **User**: can edit assigned tasks and create new tasks  

---

### 2.2 User Role Management

| Role  | View Tasks       | Update Tasks     | Assign Tasks | Manage Users |
|-------|------------------|------------------|--------------|--------------|
| Admin | All tasks        | All tasks        | Yes          | Yes          |
| User  | Own tasks only   | Own tasks only   | Yes          | No           |

---

### 2.3 Task Management

Each task includes:  
- Task Code  
- Task Name  
- Description  
- Assignee (person responsible)  
- Status: `0: Not started`, `1: In progress`, `2: Completed`  
- Due Date (optional)  
- Priority level (1 - 3)  

---

### 2.4 Real-time Communication (SignalR)

- When tasks are created or modified, all clients update their UI in real time.  
- For Admin users: when a user is created, all Admin clients update their UI in real time.  

---

## 2.5 Screens

---

### 2.5.1 Login Screen

**Purpose:**  
Allows users to enter username and password to access the system.

**UI Elements:**  
- **TextBox** `Username`  
  - Two-way binding with `Username` property in ViewModel.  
- **PasswordBox** `Password`  
  - Manually synchronized with `Password` property in ViewModel via `PasswordChanged` event.  
- **TextBlock** for displaying validation errors (e.g., incorrect username, empty password)  
  - Bound to `LoginErrorMessage` property in ViewModel.  
- **Button** `Login`  
  - Executes `LoginCommand` in ViewModel.

**Functionality:**  
- Validate input (username and password cannot be empty).  
- Lookup user in database and compare hashed password with BCrypt.  
- On success, initialize user session (`UserSession`) and close login window.  
- On failure, display appropriate error message.

---

### 2.5.2 Task Management Screen

**Purpose:**  
Display current task list and allow users (admin and user) to edit, update, and delete tasks (deletion only for admin).

**UI Elements:**  
- **DataGrid** bound to `Tasks` (ObservableCollection<TaskViewModel>).

**Columns:**  
| Column        | Type       | Binding / Editing     | Notes                                 |
|---------------|------------|----------------------|----------------------------------------|
| Task Code     | Text       | OneWay (ReadOnly)    | Bound to `Code` property               |
| Task Name     | Text       | TwoWay               | Bound to `Title`                       |
| Description   | Text       | TwoWay               | Bound to `Description`                 |
| Reporter      | ComboBox   | TwoWay               | Choose from `Reporters` (admins list)  |
| Assignee      | ComboBox   | TwoWay               | Choose from `Assignees` (users list)   |
| Status        | ComboBox   | TwoWay               | From `AppConstants.Statuses`           |
| Due Date      | DatePicker | TwoWay               | Format: `yyyy-MM-dd`                   |
| Priority      | ComboBox   | TwoWay               | From `AppConstants.Priorities`         |
| Created Date  | Text       | OneWay (ReadOnly)    | Format: `yyyy-MM-dd HH:mm`             |
| Updated Date  | Text       | OneWay (ReadOnly)    | Format: `yyyy-MM-dd HH:mm`             |
| Update        | Button     | Command              | Executes `UpdateTaskCommand`           |
| Delete        | Button     | Command + Visibility | Executes `DeleteTaskCommand`, visible only if `UserSession.Instance.IsAdmin == true` |

**Functionality:**  
- Load data from repositories (Tasks, Users) on ViewModel initialization.  
- On Update button click, confirm action; if confirmed, update DB and broadcast SignalR event for realtime update.  
- Task deletion is admin-only, with confirmation and realtime notification.  
- Listen to SignalR events to reload task list when changes happen from other clients.  
- Role-based UI permissions:  
  - Only admins can delete tasks.  
  - Both admins and users can edit/update tasks.

---

### 2.5.3 User Management Screen

**Purpose:**  
Display user list and allow admins to delete users.

**UI Elements:**  
- **DataGrid** bound to `Users` (ObservableCollection<UserViewModel>).

**Columns:**  
| Column        | Type       | Notes                            |
|---------------|------------|----------------------------------|
| Employee Code | Text       | Employee code                    |
| Username      | Text       | Login username                   |
| Display Name  | Text       | Display name                     |
| Email         | Text       | Email (optional)                 |
| Is Admin      | CheckBox   | Readonly, indicates admin status |
| Is Active     | CheckBox   | Readonly, active status          |
| Created At    | Text       | Format: `yyyy-MM-dd HH:mm`       |
| Delete        | Button     | Executes `DeleteUserCommand`, visible only if `UserSession.Instance.IsAdmin == true` |

**Functionality:**  
- Load user list from repository on ViewModel init and on SignalR events.  
- On Delete button click, confirm; if yes, delete user and broadcast SignalR notification.  
- Show error if deletion fails.  
- Role-based visibility: only admin can see/use Delete button.

---

### 2.5.4 New Task Creation Screen

**Purpose:**  
Allows users to create new tasks.

**UI Elements:**  
- **TextBox** `TaskName` (TwoWay binding with `Title` in ViewModel)  
- **TextBox** `Description` (TwoWay binding)  
- **DatePicker** `DueDate` (TwoWay binding)  
- **ComboBox** `Reporter` (ItemsSource from Admin list, TwoWay binding)  
- **ComboBox** `Assignee` (ItemsSource from User list, TwoWay binding)  
- **ComboBox** `Status` (ItemsSource from `AppConstants.Statuses`, store ID)  
- **ComboBox** `Priority` (ItemsSource from `AppConstants.Priorities`, store ID)  
- **Button** `Create` activates `CreateTaskCommand`

**Functionality:**  
- Validate input: task name not empty, valid due date, reporter and assignee selected.  
- On Create click, validate and call repository to add new task.  
- On success, close or refresh UI; task list auto-updates via SignalR.  
- Role-based access: only authorized users can create tasks.  
- Show input or save error messages if any.

---

### 2.5.5 New User Creation Screen

**Purpose:**  
Allows Admin to create new user accounts.

**UI Elements:**  
- **TextBox** `EmployeeCode` (TwoWay binding, required, check duplicates)  
- **TextBox** `Username` (TwoWay binding, required, check duplicates)  
- **PasswordBox** `Password` (Manually synced via `PasswordChanged`, required, error display)  
- **TextBox** `DisplayName` (TwoWay binding, required)  
- **TextBox** `Email` (TwoWay binding, optional)  
- **CheckBox** `IsAdmin` (TwoWay binding)  
- Validation errors displayed immediately under each input via Data Validation Template  
- **Button** `Save` activates `SaveCommand`  
- **Button** `Clear` activates `ClearCommand`

**Functionality:**  
- Validate: EmployeeCode, Username, Password, DisplayName not empty; check duplicates for EmployeeCode and Username.  
- On Save, if valid, call repository to save user; show success or error notification.  
- Broadcast SignalR notification to update user lists elsewhere.  
- After saving or clearing, reset form inputs.  
- Role-based access: only Admin can access and operate this screen.

---
## 3. Schema Diagram:

+-----------------------+        +------------------------+
|       Users           |        |        Tasks           |
+-----------------------+        +------------------------+
| Id (PK)               |<----+  | Id (PK)                |
| EmployeeCode (UNIQUE) |     |  | Code (UNIQUE)          |
| Username (UNIQUE)     |     +--| Title                  |
| PasswordHash          |        | Description            |
| DisplayName           |        | Status                 |
| Email                 |        | DueDate                |
| IsAdmin               |        | Priority               |
| IsActive              |        | ReporterId (FK)--------+---------> Users.EmployeeCode
| CreatedAt             |        | AssigneeId (FK)--------+---------> Users.EmployeeCode
+-----------------------+        | CreatedAt              |
                                 | UpdatedAt              |
                                 +------------------------+
---

## 4. Project Structure (MVVM)
Solution 'TaskManager' (2 of 2 projects)
├── TaskManagerApp/
│   ├── Dependencies
│   ├── Common/
│   │   ├── AppConstants.cs
│   │   ├── IniConfig.cs
│   │   ├── InvokeCommandActionWithEventArgs.cs
│   │   └── RelayCommand.cs
│   ├── Contexts/
│   │   ├── DatabaseContext.cs
│   │   └── UserSession.cs
│   ├── Converters/
│   │   └── BoolToEditUpdateConverter.cs
│   ├── Data/
│   │   ├── TaskRepository.cs
│   │   └── UserRepository.cs
│   ├── Debug/
│   │   └── TaskManager.ini
│   ├── Models/
│   │   ├── TaskModel.cs
│   │   └── UserModel.cs
│   ├── Services/
│   │   ├── Logger.cs
│   │   ├── PasswordBoxHelper.cs
│   │   ├── SignalRService.cs
│   │   ├── Startup.cs
│   │   └── TaskService.cs
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs
│   │   ├── LoginViewModel.cs
│   │   ├── MainWindowViewModel.cs
│   │   ├── NewTaskScreenViewModel.cs
│   │   ├── NewUserViewModel.cs
│   │   ├── TaskScreenViewModel.cs
│   │   └── UserScreenViewModel.cs
│   ├── Views/
│   │   ├── Screens/
│   │   │   ├── NewTaskScreen.xaml
│   │   │   ├── NewTaskScreen.xaml.cs
│   │   │   ├── NewUserScreen.xaml
│   │   │   ├── NewUserScreen.xaml.cs
│   │   │   ├── SettingScreen.xaml
│   │   │   ├── TaskScreen.xaml
│   │   │   └── TaskScreen.xaml.cs
│   │   ├── LoginWindow.xaml
│   │   ├── LoginWindow.xaml.cs
│   │   ├── MainWindow.xaml
│   │   └── MainWindow.xaml.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   └── AssemblyInfo.cs
└── TaskManagerSignalRHub/
    ├── Connected Services
    ├── Dependencies
    ├── Properties
    ├── Hubs/
    │   └── TaskHub.cs
    └── Program.cs

=====================================================================================================
Solution 'TaskManager' (2 of 2 projects)
├── TaskManagerApp/
│   ├── Dependencies/ (Folder containing external packages and libraries)
│   ├── Common/ (Folder for common utilities, constants, commands, etc.)
│   │   ├── AppConstants.cs (Application-wide constants)
│   │   ├── IniConfig.cs (Class for handling configuration from INI files, if any)
│   │   ├── InvokeCommandActionWithEventArgs.cs (A helper class to invoke commands with event arguments)
│   │   └── RelayCommand.cs (ICommand implementation class for simple commands)
│   │
│   ├── Contexts/ (Folder for classes managing data contexts or user sessions)
│   │   ├── DatabaseContext.cs (Database context, e.g., Entity Framework DbContext)
│   │   ├── SignalRService.cs (Service class for interacting with SignalR, if the app uses real-time features)
│   │   └── UserSession.cs (Class for managing user sessions, login information, etc.)
│   │
│   ├── Converters/ (Folder for value converter classes used in Data Binding)
│   │   └── BoolToEditUpdateConverter.cs (Converter to transform boolean values to string or edit/update states)
│   │
│   ├── Data/ (Folder for data access classes - Repositories)
│   │   ├── TaskRepository.cs (Class for data access operations related to Task)
│   │   └── UserRepository.cs (Class for data access operations related to User)
│   │
│   ├── Debug/ (Folder containing configuration files for Debug mode)
│   │   └── TaskManager.ini (Application configuration file)
│   │
│   ├── Models/ (Folder for classes representing data structures - Business Logic objects)
│   │   ├── TaskModel.cs (Model for the Task object)
│   │   └── UserModel.cs (Model for the User object)
│   │
│   ├── Services/ (Folder for service classes implementing business functionalities)
│   │   ├── Logger.cs (Logging class)
│   │   ├── PasswordBoxHelper.cs (Helper class for PasswordBox, as it doesn't support direct binding)
│   │   ├── SignalRService.cs (Service class for interacting with SignalR, if the app uses real-time features)
│   │   ├── Startup.cs  (Configures SignalR services and request pipeline for the application.)
│   │   └── TaskService.cs (Service class containing business logic related to Tasks)
│   │
│   ├── ViewModels/ (Folder for ViewModel classes, mediating between View and Model)
│   │   ├── BaseViewModel.cs (Base class for other ViewModels, containing common properties like INotifyPropertyChanged)
│   │   ├── LoginViewModel.cs (ViewModel for the login screen)
│   │   ├── MainWindowViewModel.cs (ViewModel for the main application window)
│   │   ├── NewTaskScreenViewModel.cs (ViewModel for the new Task creation screen)
│   │   ├── NewUserViewModel.cs (ViewModel for the new User creation screen)
│   │   ├── TaskScreenViewModel.cs (ViewModel for the Task display/management screen)
│   │   └── UserScreenViewModel.cs (ViewModel for the User display/management screen)
│   │
│   ├── Views/ (Folder for XAML files defining the user interface)
│   │   ├── Screens/ (Subfolder for UserControls or Pages used as screens)
│   │   │   ├── NewTaskScreen.xaml (UI for creating a new Task)
│   │   │   ├── NewTaskScreen.xaml.cs (Code-behind for NewTaskScreen)
│   │   │   ├── NewUserScreen.xaml (UI for creating a new User)
│   │   │   ├── NewUserScreen.xaml.cs (Code-behind for NewUserScreen)
│   │   │   ├── SettingScreen.xaml (UI for the settings screen)
│   │   │   ├── TaskScreen.xaml (UI for displaying/managing Tasks)
│   │   │   └── TaskScreen.xaml.cs (Code-behind for TaskScreen)
│   │   │
│   │   ├── LoginWindow.xaml (UI for the login window)
│   │   ├── LoginWindow.xaml.cs (Code-behind for LoginWindow)
│   │   ├── MainWindow.xaml (UI for the main application window)
│   │   └── MainWindow.xaml.cs (Code-behind for MainWindow)
│   │
│   ├── App.xaml (Application entry point, defines common resources and startup window)
│   ├── App.xaml.cs (Code-behind for App.xaml, contains application initialization logic)
│   └── AssemblyInfo.cs (Assembly information, e.g., version, author)
│
└── TaskManagerSignalRHub/ (Separate project if you have a standalone SignalR Hub)
    ├── Connected Services/ (Folder containing references to connected services)
    ├── Dependencies/ (Folder containing external packages and libraries for the Hub project)
    ├── Properties/ (Folder containing project-specific configuration files)
    ├── Hubs/ (Folder containing SignalR Hub classes)
    │   └── TaskHub.cs (SignalR Hub for Task-related operations)
    └── Program.cs (Entry point for the SignalR Hub application)
---

## 6. Logger used to log the entire application:
- Singleton Pattern: Ensures only one instance of the Logger exists throughout the application.
- Asynchronous Logging: Writes log entries to a file on a separate thread, improving performance.
- Log Rotation: Automatically manages log files by deleting older logs to keep the total count at a maximum of 5.
- Contextual Logging: Automatically captures the calling class and method for error logs.
- Graceful Shutdown: Provides a Shutdown() method to ensure all queued logs are written before the application exits.

---

## Debug Mode Configuration

The application supports reading a configuration file named `TaskManager.ini` for development and testing purposes.

Example `TaskManager.ini` content:

```ini
[AppSettings]
Mode=Debug
IsAdmin=false
```

### Behavior in Debug Mode:
- **Skip Login Screen**: The app will bypass the login process and go directly to the main screen.
- **Admin Privileges**:  
  - If `IsAdmin=true`, the current user is treated as an **Admin**.  
  - If `IsAdmin=false`, the current user is treated as a **regular user**.

> In `Release` mode, this configuration is ignored and the app follows the normal login workflow.
