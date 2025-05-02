using System.Windows;
using System.Windows.Controls;
using Biblioteka.Services;
using Biblioteka.Data.Entities;

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
                var user = _authService.Login(LoginBox.Text, PasswordBox.Password);
                if (user == null)
                {
                    MessageBox.Show("Nieprawidłowy login lub hasło.", "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
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