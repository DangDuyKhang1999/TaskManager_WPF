using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Common;
using TaskManagerApp.Contexts;
using TaskManagerApp.Data;
using TaskManagerApp.Models;
using TaskManagerApp.Services;

namespace TaskManagerApp.ViewModels
{
    /// <summary>
    /// ViewModel for managing users on the User screen.
    /// </summary>
    public class UserScreenViewModel : BaseViewModel
    {
        private ObservableCollection<UserModel>? _users;

        /// <summary>
        /// Collection of users displayed in the UI.
        /// </summary>
        public ObservableCollection<UserModel>? Users
        {
            get => _users ??= new ObservableCollection<UserModel>();
            set => SetProperty(ref _users, value);
        }

        private readonly UserRepository _userRepository;

        /// <summary>
        /// Command to delete a user.
        /// </summary>
        public ICommand DeleteUserCommand { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="UserScreenViewModel"/>.
        /// Sets up repositories, commands, event listeners, and initializes SignalR.
        /// </summary>
        public UserScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;
            _userRepository = new UserRepository(connectionString);

            // Listen for the TaskSaved event to refresh the user list.
            TaskEvents.TaskSaved += () =>
            {
                Application.Current.Dispatcher.Invoke(ReloadUsers);
            };

            ReloadUsers();

            // Initialize command for user deletion.
            DeleteUserCommand = new RelayCommand(DeleteUser);

            // Start SignalR connection asynchronously.
            _ = InitSignalRAsync();
        }

        /// <summary>
        /// Initializes SignalR connection and subscribes to UsersChanged event.
        /// </summary>
        private async Task InitSignalRAsync()
        {
            string hubUrl = "http://localhost:5000/taskhub";
            await SignalRService.Instance.StartAsync(hubUrl);

            // Subscribe to user change notifications from SignalR.
            SignalRService.Instance.UsersChanged += SignalR_OnUsersChanged;
        }

        /// <summary>
        /// Handler for user list changes received via SignalR.
        /// Triggers user list reload on the UI thread.
        /// </summary>
        private void SignalR_OnUsersChanged()
        {
            Application.Current.Dispatcher.Invoke(ReloadUsers);
        }

        /// <summary>
        /// Reloads the list of users from the database and updates the UI.
        /// </summary>
        public void ReloadUsers()
        {
            try
            {
                var userList = _userRepository.GetAllUsers();
                Users = new ObservableCollection<UserModel>(userList);
                Logger.Instance.Information("Users reloaded successfully.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Error reloading users: " + ex.Message);
                MessageBox.Show("Failed to reload users. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Deletes the specified user after user confirmation.
        /// </summary>
        /// <param name="parameter">The user to delete (as UserModel).</param>
        private void DeleteUser(object? parameter)
        {
            // Ensure the parameter is a valid user model with a valid ID.
            if (parameter is not UserModel user || user.Id <= 0)
                return;

            // Prompt for user confirmation before deletion.
            var result = MessageBox.Show(
                $"Are you sure you want to delete the user '{user.DisplayName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Attempt to delete the user from the database.
            bool isDeleted = _userRepository.DeleteUserById(user.Id);

            if (isDeleted)
            {
                // Remove the user from the UI list and notify via SignalR.
                if (Users != null)
                {
                    Users.Remove(user);
                    Logger.Instance.Information($"User '{user.Username}' removed from UI and database.");
                    MessageBox.Show(
                        $"User '{user.DisplayName}' was deleted successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    _ = SignalRService.Instance.NotifyUserChangedAsync();
                }
            }
            else
            {
                // Log and display error if deletion fails.
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
