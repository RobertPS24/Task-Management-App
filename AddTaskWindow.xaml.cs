using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TaskManagementApp
{
    public partial class AddTaskWindow : Window
    {
        private Task taskToEdit;
        private readonly string createdBy; 

        public event EventHandler TaskUpdated;

        public AddTaskWindow(string username, Task task = null)
        {
            InitializeComponent();
            createdBy = username;

            TaskDate.DisplayDateStart = DateTime.Today; 

            if (task != null)
            {
                taskToEdit = task;

                TaskName.Text = task.TaskName;
                TaskDescription.Text = task.TaskDescription;

                if (!string.IsNullOrWhiteSpace(task.TaskName))
                    TitlePlaceholder.Visibility = Visibility.Collapsed;

                if (!string.IsNullOrWhiteSpace(task.TaskDescription))
                    DescriptionPlaceholder.Visibility = Visibility.Collapsed;

                if (DateTime.TryParse(task.TaskDate, out DateTime parsedDate))
                    TaskDate.SelectedDate = parsedDate;

                PriorityComboBox.SelectedItem = PriorityComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == task.Priority);

                ProjectComboBox.SelectedItem = ProjectComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == task.Project);

                AddTaskButton.Content = "Update Task";
            }
            else
            {
                AddTaskButton.Content = "Add Task";
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string taskName = TaskName.Text.Trim();
            string taskDescription = TaskDescription.Text.Trim();
            string taskDate = TaskDate.SelectedDate?.ToString("d") ?? "No date selected";
            string priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string project = (ProjectComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(taskName))
            {
                MessageBox.Show("Please enter a task title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (taskToEdit == null)
            {
                // Task nou
                SQLiteHelper.SaveTask(taskName, taskDescription, taskDate, priority, project, createdBy);
                MessageBox.Show("Task adăugat cu succes!", "Confirmare", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Task existent
                SQLiteHelper.UpdateTask(taskToEdit.Id, taskName, taskDescription, taskDate, priority, project);
                MessageBox.Show("Task actualizat cu succes!", "Confirmare", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            TaskUpdated?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TaskName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TitlePlaceholder.Visibility = string.IsNullOrWhiteSpace(TaskName.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TaskDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            DescriptionPlaceholder.Visibility = string.IsNullOrWhiteSpace(TaskDescription.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
