using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TaskManagementApp
{
    public partial class MainWindow : Window
    {
        private string loggedInUsername;
        private bool isDark = false;

        public MainWindow(string username)
        {
            InitializeComponent();
            loggedInUsername = username;

            HighlightSelectedButton(Home);
            SQLiteHelper.CreateTasksTable();
            LoadTasks();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            var (email, profileImagePath) = SQLiteHelper.GetUserInfo(loggedInUsername);
            UsernameText.Text = loggedInUsername;

            if (!string.IsNullOrWhiteSpace(profileImagePath) && File.Exists(profileImagePath))
                ProfileImage.Source = new BitmapImage(new Uri(profileImagePath));
            else
                ProfileImage.Source = new BitmapImage(new Uri("Images/profile-user.png", UriKind.Relative));
        }

        private void LoadTasks()
        {
            TaskListPanel.Children.Clear();

            using (var connection = SQLiteHelper.GetConnection())
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Tasks WHERE CreatedBy = @Username;";

                using (var cmd = new SQLiteCommand(selectQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", loggedInUsername);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Task task = new Task(
                                Convert.ToInt32(reader["Id"]),
                                reader["TaskName"].ToString(),
                                reader["TaskDescription"].ToString(),
                                reader["TaskDate"].ToString(),
                                reader["Priority"].ToString(),
                                reader["Project"].ToString()
                            );

                            AddTaskToRoutines(task);
                        }
                    }
                }
            }
        }

        public void AddTaskToRoutines(Task task)
        {
            Brush color = Brushes.Gray;
            if (task.Project == "All") color = Brushes.Blue;
            else if (task.Project == "Work") color = Brushes.Orange;
            else if (task.Project == "Home") color = Brushes.Pink;
            else if (task.Project == "Personal") color = Brushes.Green;

            StackPanel taskStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5),
                VerticalAlignment = VerticalAlignment.Center
            };

            Ellipse colorDot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = color,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 5, 0)
            };

            StackPanel textStack = new StackPanel { Orientation = Orientation.Vertical };

            textStack.Children.Add(new TextBlock
            {
                Text = task.TaskName,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            textStack.Children.Add(new TextBlock
            {
                Text = task.TaskDescription,
                FontSize = 13,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 0)
            });

            textStack.Children.Add(new TextBlock
            {
                Text = $"Date: {task.TaskDate}, Priority: {task.Priority}, Project: {task.Project}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 0)
            });

            taskStack.Children.Add(colorDot);
            taskStack.Children.Add(textStack);

            var newTaskCheckBox = new CheckBox
            {
                Content = taskStack,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 0, 5)
            };

            newTaskCheckBox.Checked += (s, e) =>
            {
                MessageBox.Show("✅ Task finalizat cu succes!", "Finalizat", MessageBoxButton.OK, MessageBoxImage.Information);
                DeleteTask(task);
            };

            var deleteButton = new Button
            {
                Content = "Delete",
                Width = 70,
                Height = 25,
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                Padding = new Thickness(5)
            };
            deleteButton.Click += (sender, e) => DeleteTask(task);

            var editButton = new Button
            {
                Content = "Edit",
                Width = 60,
                Height = 25,
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                Padding = new Thickness(5)
            };
            editButton.Click += (sender, e) => EditTask(task);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };
            buttonPanel.Children.Add(deleteButton);
            buttonPanel.Children.Add(editButton);

            TaskListPanel.Children.Add(newTaskCheckBox);
            TaskListPanel.Children.Add(buttonPanel);
        }

        private void DeleteTask(Task task)
        {
            SQLiteHelper.DeleteTask(task.Id);
            LoadTasks();
        }

        public void EditTask(Task task)
        {
            var addTaskWindow = new AddTaskWindow(loggedInUsername, task);
            addTaskWindow.Owner = this;
            addTaskWindow.TaskUpdated += (_, __) => LoadTasks();
            addTaskWindow.ShowDialog();
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow(loggedInUsername);
            addTaskWindow.Owner = this;
            addTaskWindow.TaskUpdated += (_, __) => LoadTasks();
            addTaskWindow.ShowDialog();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(Home);
            SectionTitle.Text = "Home";
            AddTaskButton.Visibility = Visibility.Visible;
            LoadTasks();
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(TodayButton);
            SectionTitle.Text = "Today";
            AddTaskButton.Visibility = Visibility.Collapsed;

            DateTime today = DateTime.Today;
            LoadTasksFiltered(task =>
                DateTime.TryParse(task.TaskDate, out DateTime date) && date.Date == today);
        }

        private void UpcomingButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(UpcomingButton);
            SectionTitle.Text = "Upcoming";
            AddTaskButton.Visibility = Visibility.Collapsed;

            DateTime tomorrow = DateTime.Today.AddDays(1);
            LoadTasksFiltered(task =>
                DateTime.TryParse(task.TaskDate, out DateTime date) && date.Date >= tomorrow);
        }

        private void LoadTasksFiltered(Func<Task, bool> filter)
        {
            TaskListPanel.Children.Clear();
            List<Task> tasks = SQLiteHelper.GetAllTasks(loggedInUsername);

            foreach (var task in tasks.Where(filter))
            {
                AddTaskToRoutines(task);
            }
        }

        private void HighlightSelectedButton(Button selectedButton)
        {
            Home.Background = Brushes.Transparent;
            TodayButton.Background = Brushes.Transparent;
            UpcomingButton.Background = Brushes.Transparent;

            Home.Foreground = Brushes.Black;
            TodayButton.Foreground = Brushes.Black;
            UpcomingButton.Foreground = Brushes.Black;

            selectedButton.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            selectedButton.Foreground = Brushes.White;
        }

    }
}
