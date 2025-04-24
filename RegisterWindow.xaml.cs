using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TaskManagementApp
{
    public partial class RegisterWindow : Window
    {
        private string profileImagePath = null;

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void ChoosePicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dlg.ShowDialog() == true)
            {
                profileImagePath = dlg.FileName;
                ProfileImage.Source = new BitmapImage(new Uri(profileImagePath));
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim().ToLower();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(profileImagePath))
            {
                MessageBox.Show("Te rugăm să completezi toate câmpurile și să alegi o poză.");
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Parola trebuie să conțină minim 4 caractere.");
                return;
            }

            
            string[] allowedDomains = { "gmail.com", "yahoo.com", "outlook.com", "protonmail.com", "icloud.com" };
            bool isValidEmail = allowedDomains.Any(domain => email.EndsWith("@" + domain));

            if (!isValidEmail)
            {
                MessageBox.Show("Emailul trebuie să fie de tip @gmail.com, @yahoo.com, @outlook.com etc.");
                return;
            }

            
            if (!SQLiteHelper.RegisterUser(username, password, email, profileImagePath))
            {
                MessageBox.Show("Acest username sau email există deja.");
                return;
            }

            MessageBox.Show("Cont creat cu succes!");
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }



        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
