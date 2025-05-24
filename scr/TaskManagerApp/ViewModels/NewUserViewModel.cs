using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;
using TaskManagerApp.Contexts;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new user with validation support.
    /// </summary>
    public class NewUserViewModel : BaseViewModel, IDataErrorInfo
    {
        private bool _hasAttemptedSave;

        private string _employeeCode = string.Empty;
        /// <summary>
        /// Gets or sets the unique employee code.
        /// </summary>
        public string EmployeeCode
        {
            get => _employeeCode;
            set => SetProperty(ref _employeeCode, value);
        }

        private string _username = string.Empty;
        /// <summary>
        /// Gets or sets the username for login.
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = string.Empty;
        /// <summary>
        /// Gets or sets the password entered by the user.
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _displayName = string.Empty;
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private string _email = string.Empty;
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private bool _isAdmin;
        /// <summary>
        /// Gets or sets a value indicating whether the user has admin rights.
        /// </summary>
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        /// <summary>
        /// Command to save the new user.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Command to clear all input fields.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewUserViewModel"/> class.
        /// </summary>
        public NewUserViewModel()
        {
            SaveCommand = new RelayCommand(_ => SaveExecute());
            ClearCommand = new RelayCommand(_ => ClearFields());
            ClearFields();
        }

        /// <summary>
        /// Gets validation error for a specific property.
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
        /// Gets an object-level validation error. Always returns null.
        /// </summary>
        public string Error => string.Empty;

        /// <summary>
        /// Indicates whether all required fields are valid.
        /// </summary>
        public bool IsValid =>
            string.IsNullOrEmpty(this[nameof(EmployeeCode)]) &&
            string.IsNullOrEmpty(this[nameof(Username)]) &&
            string.IsNullOrEmpty(this[nameof(Password)]) &&
            string.IsNullOrEmpty(this[nameof(DisplayName)]);

        /// <summary>
        /// Executes the save operation: validates input, inserts the user, and notifies the user.
        /// </summary>
        private void SaveExecute()
        {
            _hasAttemptedSave = true;

            OnPropertyChanged(nameof(EmployeeCode));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(DisplayName));

            if (!IsValid)
                return;

            var user = new UserModel
            {
                EmployeeCode = EmployeeCode,
                Username = Username,
                PasswordHash = Password,
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
                    _ = SignalRService.Instance.NotifyTaskChangedAsync();
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

            ClearFields();
        }

        /// <summary>
        /// Clears all input fields and resets validation state.
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

            OnPropertyChanged(nameof(EmployeeCode));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(IsAdmin));
        }

        /// <summary>
        /// Checks if the specified employee code already exists.
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
        /// Checks if the specified username already exists.
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
