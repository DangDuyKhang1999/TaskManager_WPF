using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Common;

namespace TaskManager.ViewModels
{
    public class TaskScreenViewModel : BaseViewModel
    {
        private ObservableCollection<TaskModel> _tasks;
        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        private ObservableCollection<string> _availableAssignees;
        public ObservableCollection<string> AvailableAssignees
        {
            get => _availableAssignees;
            set => SetProperty(ref _availableAssignees, value);
        }

        private readonly TaskRepository _taskRepository;

        public TaskScreenViewModel()
        {
            _taskRepository = new TaskRepository(AppConstants.Database.ConnectionString);
            var taskList = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);
            AvailableAssignees = new ObservableCollection<string>();
        }
    }
}
