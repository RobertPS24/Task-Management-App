using System.Windows;

namespace TaskManagementApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            if (SQLiteHelper.ValidateLogin(username, password))
            {
                MessageBox.Show("Login successful!");

                
                var mainWindow = new MainWindow(username);
                mainWindow.Show();

                this.Close(); 
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }


        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close(); 
        }
    }
}
