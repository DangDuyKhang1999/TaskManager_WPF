using System;
using System.ComponentModel;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Data;

namespace TaskManager.ViewModels
{
    public class NewUserViewModel : BaseViewModel, IDataErrorInfo
    {
        private bool _hasAttemptedSave;

        private string _employeeCode;
        public string EmployeeCode
        {
            get => _employeeCode;
            set => SetProperty(ref _employeeCode, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }

        public NewUserViewModel()
        {
            SaveCommand = new RelayCommand(_ => SaveExecute());
            ClearCommand = new RelayCommand(_ => ClearFields());
            ClearFields();
        }

        public string this[string columnName]
        {
            get
            {
                if (!_hasAttemptedSave) return null;
                switch (columnName)
                {
                    case nameof(EmployeeCode):
                        return string.IsNullOrWhiteSpace(EmployeeCode)
                            ? AppConstants.AppText.ValidationMessages.CodeRequired
                            : null;
                    case nameof(Username):
                        return string.IsNullOrWhiteSpace(Username)
                            ? AppConstants.AppText.ValidationMessages.UsernameRequired
                            : null;
                    case nameof(Password):
                        return string.IsNullOrWhiteSpace(Password)
                            ? AppConstants.AppText.ValidationMessages.PasswordRequired
                            : null;
                }
                return null;
            }
        }

        public string Error => null;

        public bool IsValid =>
            this[nameof(EmployeeCode)] == null &&
            this[nameof(Username)] == null &&
            this[nameof(Password)] == null;

        private void SaveExecute()
        {
            _hasAttemptedSave = true;

            OnPropertyChanged(nameof(EmployeeCode));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));

            if (!IsValid) return;

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
                    System.Windows.MessageBox.Show(AppConstants.AppText.Message_UserSaveSuccess, AppConstants.ExecutionStatus.Success);
                    ClearFields();
                }
                else
                {
                    System.Windows.MessageBox.Show(AppConstants.AppText.Message_UserSaveFailed, AppConstants.ExecutionStatus.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.AppText.Message_UnexpectedError + ex.Message);
            }
        }

        private void ClearFields()
        {
            _hasAttemptedSave = false;
            EmployeeCode = Username = Password = DisplayName = Email = string.Empty;
            IsAdmin = false;

            OnPropertyChanged(nameof(EmployeeCode));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
        }
    }
}
