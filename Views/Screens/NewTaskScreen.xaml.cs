using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManager.Contexts;

namespace TaskManager.Views.Screens
{
    public partial class NewTaskScreen : UserControl
    {
        private List<string> reporterUsers = new List<string>();
        private List<string> assigneeUsers = new List<string>();

        public NewTaskScreen()
        {
            InitializeComponent();

            LoadUsers();

            ReporterIdComboBox.IsEditable = true;
            ReporterIdComboBox.IsTextSearchEnabled = false;
            ReporterIdComboBox.StaysOpenOnEdit = true;

            AssigneeIdComboBox.IsEditable = true;
            AssigneeIdComboBox.IsTextSearchEnabled = false;
            AssigneeIdComboBox.StaysOpenOnEdit = true;

            ReporterIdComboBox.PreviewKeyUp += ReporterIdComboBox_PreviewKeyUp;
            AssigneeIdComboBox.PreviewKeyUp += AssigneeIdComboBox_PreviewKeyUp;
        }

        private void LoadUsers()
        {
            reporterUsers = DatabaseContext.Instance.AdminUsersList.ToList();
            assigneeUsers = DatabaseContext.Instance.NormalUsersList.ToList();

            ReporterIdComboBox.ItemsSource = reporterUsers.Take(10);
            AssigneeIdComboBox.ItemsSource = assigneeUsers.Take(10);

            if (ReporterIdComboBox.Items.Count > 0)
                ReporterIdComboBox.SelectedIndex = 0;

            if (AssigneeIdComboBox.Items.Count > 0)
                AssigneeIdComboBox.SelectedIndex = 0;
        }

        private void ReporterIdComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FilterComboBoxItems(ReporterIdComboBox, reporterUsers);
        }

        private void AssigneeIdComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FilterComboBoxItems(AssigneeIdComboBox, assigneeUsers);
        }

        private void FilterComboBoxItems(ComboBox comboBox, List<string> sourceList)
        {
            string text = comboBox.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                comboBox.ItemsSource = null;
                comboBox.IsDropDownOpen = false;
            }
            else
            {
                var filtered = sourceList
                    .Where(x => x.StartsWith(text, System.StringComparison.InvariantCultureIgnoreCase))
                    .Take(10)
                    .ToList();

                comboBox.ItemsSource = filtered;
                comboBox.IsDropDownOpen = filtered.Any();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save button clicked!");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CodeTextBox.Text = string.Empty;
            TitleTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            StatusComboBox.SelectedIndex = 0;
            DueDatePicker.SelectedDate = null;
            PriorityComboBox.SelectedIndex = 0;

            if (ReporterIdComboBox.Items.Count > 0)
                ReporterIdComboBox.SelectedIndex = 0;

            if (AssigneeIdComboBox.Items.Count > 0)
                AssigneeIdComboBox.SelectedIndex = 0;
        }
    }
}
