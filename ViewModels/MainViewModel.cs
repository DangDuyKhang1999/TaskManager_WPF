using System.Collections.ObjectModel;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<TaskModel> Tasks { get; set; }
        public ObservableCollection<string> AvailableAssignees { get; set; }

        public MainViewModel()
        {
            AvailableAssignees = new ObservableCollection<string>
            {
                "Nguyễn Văn A", "Trần Thị B", "Lê Văn C", "Phạm Văn D"
            };

            Tasks = new ObservableCollection<TaskModel>
            {
                new TaskModel { Id = "CV001", Title = "Viết tài liệu", Description = "Soạn tài liệu sử dụng", Assignee = "Nguyễn Văn A", Status = "Chưa thực hiện" },
                new TaskModel { Id = "CV002", Title = "Kiểm thử", Description = "Test module đăng nhập", Assignee = "Trần Thị B", Status = "Đang thực hiện" },
                new TaskModel { Id = "CV003", Title = "Triển khai", Description = "Cài đặt server", Assignee = "Lê Văn C", Status = "Hoàn thành" }
            };
        }
    }
}
