using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class UserScreenViewModel : BaseViewModel
    {
        private ObservableCollection<UserModel> _users;
        public ObservableCollection<UserModel> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private readonly UserRepository _userRepository;

        public ICommand DeleteUserCommand { get; }

        public UserScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;
            _userRepository = new UserRepository(connectionString);

            var userList = _userRepository.GetAllUsers();
            Users = new ObservableCollection<UserModel>(userList);

            DeleteUserCommand = new RelayCommand(DeleteUser);
        }

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
