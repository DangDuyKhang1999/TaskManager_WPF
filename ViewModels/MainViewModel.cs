using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<TaskModel> Tasks { get; set; }
        public ObservableCollection<string> AvailableAssignees { get; set; }

        private TaskRepository _repository;

        public MainViewModel()
        {
            string connectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";
            _repository = new TaskRepository(connectionString);

            // Load from DB
            var taskList = _repository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);

            // Lấy danh sách người phụ trách từ các công việc
            AvailableAssignees = new ObservableCollection<string>();
            foreach (var task in taskList)
            {
                if (!AvailableAssignees.Contains(task.Assignee))
                    AvailableAssignees.Add(task.Assignee);
            }
        }
    }
}
