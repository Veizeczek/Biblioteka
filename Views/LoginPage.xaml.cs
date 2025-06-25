using System;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Services;
using Biblioteka.Data.Entities;
using System.Windows.Input;


namespace Biblioteka.Views
{
    public partial class LoginPage : Page
    {
        private readonly AuthService _authService = new AuthService();

        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // konwertujemy login na małe litery przed sprawdzeniem
                var login = LoginBox.Text.ToLowerInvariant();
                var user = _authService.Login(login, PasswordBox.Password);

                if (user == null)
                {
                    MessageBox.Show(
                        "Nieprawidłowy login lub hasło.",
                        "Błąd logowania",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Navigate based on role
                var mainFrame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                if (user.IsAdmin)
                    mainFrame.Navigate(new AdminPage(user));
                else
                    mainFrame.Navigate(new UserPage(user));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
     // focus przechodzi z LoginBox do PasswordBox
        private void LoginBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
                e.Handled = true;
            }
        }

        // w PasswordBox Enter wywołuje logowanie, ale tylko gdy są obie wartości
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var login    = LoginBox.Text;
                var password = PasswordBox.Password;
                if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrEmpty(password))
                {
                    LoginButton_Click(LoginButton, new RoutedEventArgs());
                }
                e.Handled = true;
            }
        }


        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new RegisterPage());
        }

        private void ForgotButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new ForgotPasswordPage());
        }
    }
}
