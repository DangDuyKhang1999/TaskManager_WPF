using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Common;
using TaskManagerApp.Data;
using TaskManagerApp.Models;
using TaskManagerApp.Services;
using TaskManagerApp.Contexts;

namespace TaskManagerApp.ViewModels
{
    /// <summary>
    /// ViewModel responsible for creating a new user with input validation.
    /// Implements IDataErrorInfo for validation feedback.
    /// </summary>
    public class NewUserViewModel : BaseViewModel, IDataErrorInfo
    {
        private bool _hasAttemptedSave;

        private string _employeeCode = string.Empty;
        /// <summary>
        /// Unique employee code identifier for the new user.
        /// </summary>
        public string EmployeeCode
        {
            get => _employeeCode;
            set => SetProperty(ref _employeeCode, value);
        }

        private string _username = string.Empty;
        /// <summary>
        /// Username used for user login.
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = string.Empty;
        /// <summary>
        /// User's password input. Will be hashed on save.
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _displayName = string.Empty;
        /// <summary>
        /// Display name shown in the application.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private string _email = string.Empty;
        /// <summary>
        /// Email address of the new user.
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private bool _isAdmin;
        /// <summary>
        /// Flag indicating if the user has administrative privileges.
        /// </summary>
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        /// <summary>
        /// Command to trigger saving the new user.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Command to clear all input fields in the form.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Constructor initializing commands and clearing fields.
        /// </summary>
        public NewUserViewModel()
        {
            SaveCommand = new RelayCommand(_ => SaveExecute());
            ClearCommand = new RelayCommand(_ => ClearFields());
            ClearFields();
        }

        /// <summary>
        /// Provides validation error messages for individual properties.
        /// Only performs validation after an attempt to save has been made.
        /// </summary>
        public string this[string columnName]
        {
            get
            {
                if (!_hasAttemptedSave) return string.Empty;

                switch (columnName)
                {
                    case nameof(EmployeeCode):
                        if (string.IsNullOrWhiteSpace(EmployeeCode))
                            return AppConstants.AppText.ValidationMessages.CodeRequired;
                        if (IsEmployeeCodeDuplicate(EmployeeCode))
                            return AppConstants.AppText.ValidationMessages.CodeDuplicate;
                        break;

                    case nameof(Username):
                        if (string.IsNullOrWhiteSpace(Username))
                            return AppConstants.AppText.ValidationMessages.UsernameRequired;
                        if (IsUsernameDuplicate(Username))
                            return AppConstants.AppText.ValidationMessages.UsernameDuplicate;
                        break;

                    case nameof(Password):
                        if (string.IsNullOrWhiteSpace(Password))
                            return AppConstants.AppText.ValidationMessages.PasswordRequired;
                        break;

                    case nameof(DisplayName):
                        if (string.IsNullOrWhiteSpace(DisplayName))
                            return AppConstants.AppText.ValidationMessages.DisplayNameRequired;
                        break;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Returns a general validation error for the entire object.
        /// Not used here, always returns empty.
        /// </summary>
        public string Error => string.Empty;

        /// <summary>
        /// Indicates whether all required fields pass validation.
        /// </summary>
        public bool IsValid =>
            string.IsNullOrEmpty(this[nameof(EmployeeCode)]) &&
            string.IsNullOrEmpty(this[nameof(Username)]) &&
            string.IsNullOrEmpty(this[nameof(Password)]) &&
            string.IsNullOrEmpty(this[nameof(DisplayName)]);

        /// <summary>
        /// Executes the user save operation:
        /// Validates inputs, inserts user into repository, 
        /// and triggers notifications or error messages accordingly.
        /// </summary>
        private void SaveExecute()
        {
            _hasAttemptedSave = true;

            // Notify UI to re-evaluate validation for these properties.
            OnPropertyChanged(nameof(EmployeeCode));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(DisplayName));

            if (!IsValid)
                return;

            // Prepare a new UserModel instance for insertion.
            var user = new UserModel
            {
                EmployeeCode = EmployeeCode,
                Username = Username,
                PasswordHash = Password, // Password hashing should be done in repository/service.
                DisplayName = DisplayName,
                Email = Email,
                IsAdmin = IsAdmin,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            try
            {
                var userRepository = new UserRepository(AppConstants.Database.ConnectionString);
                bool inserted = userRepository.InsertUser(user);
                if (inserted)
                {
                    MessageBox.Show(AppConstants.AppText.Message_UserSaveSuccess, AppConstants.ExecutionStatus.Success);
                    _ = SignalRService.Instance.NotifyUserChangedAsync(); // Notify other parts of app about user change.
                    TaskEvents.RaiseTaskSaved(); // Trigger any subscribed event handlers for saved tasks.
                }
                else
                {
                    MessageBox.Show(AppConstants.AppText.Message_UserSaveFailed, AppConstants.ExecutionStatus.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.AppText.Message_UnexpectedError + ex.Message);
            }

            // Reset form fields after save attempt.
            ClearFields();
        }

        /// <summary>
        /// Clears all user input fields and resets validation state.
        /// </summary>
        private void ClearFields()
        {
            _hasAttemptedSave = false;

            EmployeeCode = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            DisplayName = string.Empty;
            Email = string.Empty;
            IsAdmin = false;

            // Notify UI of cleared fields.
            OnPropertyChanged(nameof(EmployeeCode));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(IsAdmin));
        }

        /// <summary>
        /// Checks if the given employee code already exists in the database.
        /// Logs and returns false if an error occurs during check.
        /// </summary>
        private bool IsEmployeeCodeDuplicate(string employeeCode)
        {
            try
            {
                var userRepository = new UserRepository(AppConstants.Database.ConnectionString);
                return userRepository.DoesEmployeeCodeExist(employeeCode);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error checking EmployeeCode duplication: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the given username already exists in the database.
        /// Logs and returns false if an error occurs during check.
        /// </summary>
        private bool IsUsernameDuplicate(string username)
        {
            try
            {
                var userRepository = new UserRepository(AppConstants.Database.ConnectionString);
                return userRepository.DoesUsernameExist(username);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error checking Username duplication: {ex.Message}");
                return false;
            }
        }
    }
}
