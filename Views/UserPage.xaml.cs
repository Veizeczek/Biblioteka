using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;

namespace Biblioteka.Views
{
    public partial class UserPage : Page
    {
        private readonly User _currentUser;
        public UserPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void BrowseBooks_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to book browsing
        }

        private void MyLoans_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to active loans
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to loan history
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to user settings
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var mainFrame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            mainFrame.Navigate(new LoginPage());
        }
    }
}