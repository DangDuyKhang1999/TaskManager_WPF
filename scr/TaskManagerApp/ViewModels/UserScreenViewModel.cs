using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// ViewModel for managing users on the User screen.
    /// </summary>
    public class UserScreenViewModel : BaseViewModel
    {
        private ObservableCollection<UserModel> _users;

        /// <summary>
        /// Collection of users displayed in the UI.
        /// </summary>
        public ObservableCollection<UserModel> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private readonly UserRepository _userRepository;

        /// <summary>
        /// Command to delete a user.
        /// </summary>
        public ICommand DeleteUserCommand { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="UserScreenViewModel"/>.
        /// </summary>
        public UserScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;
            _userRepository = new UserRepository(connectionString);

            var userList = _userRepository.GetAllUsers();
            Users = new ObservableCollection<UserModel>(userList);

            DeleteUserCommand = new RelayCommand(DeleteUser);
        }

        /// <summary>
        /// Deletes a user after confirmation.
        /// </summary>
        /// <param name="parameter">The user to delete.</param>
        private void DeleteUser(object parameter)
        {
            if (parameter is not UserModel user || user.Id <= 0)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the user '{user.DisplayName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            bool isDeleted = _userRepository.DeleteUserById(user.Id);

            if (isDeleted)
            {
                Users.Remove(user);
                Logger.Instance.Information($"User '{user.Username}' removed from UI and database.");
                MessageBox.Show(
                    $"User '{user.DisplayName}' was deleted successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                Logger.Instance.Warning($"Failed to delete user '{user.Username}'.");
                MessageBox.Show(
                    $"Failed to delete user '{user.DisplayName}'. Please try again later.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
