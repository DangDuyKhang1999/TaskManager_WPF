using System;
using System.ComponentModel;

namespace TaskManager.Models
{
    /// <summary>
    /// Represents a task entity with properties for data binding.
    /// Implements INotifyPropertyChanged to notify the UI of property changes.
    /// </summary>
    public class TaskModel : INotifyPropertyChanged
    {
        private string _assignee = string.Empty;
        private int _status;
        private int _priority;

        /// <summary>
        /// Gets or sets the unique identifier of the task.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the task code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the title of the task.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the current status of the task (0=Not Started, 1=In Progress, 2=Completed).
        /// </summary>
        public int Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusString));
                }
            }
        }

        /// <summary>
        /// Gets the string representation of the task status for UI display.
        /// </summary>
        public string StatusString => _status switch
        {
            0 => "Not Started",
            1 => "In Progress",
            2 => "Completed",
            _ => "Unknown"
        };

        /// <summary>
        /// Gets or sets the reporter (creator) of the task.
        /// </summary>
        public string? ReporterId { get; set; }

        /// <summary>
        /// Gets or sets the assignee of the task.
        /// Raises PropertyChanged event when the value changes.
        /// </summary>
        public string AssigneeId
        {
            get => _assignee;
            set
            {
                if (_assignee != value)
                {
                    _assignee = value;
                    OnPropertyChanged(nameof(AssigneeId));
                }
            }
        }

        /// <summary>
        /// Gets or sets the due date of the task.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the priority of the task.
        /// </summary>
        public int Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                    OnPropertyChanged(nameof(PriorityString));
                }
            }
        }

        /// <summary>
        /// Gets the string representation of the priority for UI display.
        /// </summary>
        public string PriorityString => _priority switch
        {
            0 => "High",
            1 => "Medium",
            2 => "Low",
            _ => "Unknown"
        };

        /// <summary>
        /// Gets or sets the date and time when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the task was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify UI about property changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
