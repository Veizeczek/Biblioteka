using System;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;   // <— potrzebne dla klasy User
using Biblioteka.Services;        // <— potrzebne dla AuthService

namespace Biblioteka.Views
{
    public partial class AddUserPage : Page
    {
        private readonly User _currentUser;
        private readonly AuthService _authService = new AuthService();

        public AddUserPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var login   = LoginBox.Text.Trim().ToLowerInvariant();
                var pwd     = PasswordBox.Password;
                var confirm = ConfirmBox.Password;

                if (pwd != confirm)
                {
                    MessageBox.Show("Hasła się nie zgadzają.", "Błąd",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var isAdmin = IsAdminCheck.IsChecked == true;
                _authService.Register(login, pwd, isAdmin);

                MessageBox.Show("Użytkownik został dodany.", "OK",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                // wracamy do ManageUsers
                var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                frame.Navigate(new ManageUsersView(_currentUser));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd dodawania użytkownika",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new ManageUsersView(_currentUser));
        }
    }
}
