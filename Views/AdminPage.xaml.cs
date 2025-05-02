using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;

namespace Biblioteka.Views
{
    public partial class AdminPage : Page
    {
        private readonly User _currentUser;
        public AdminPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void ManageBooks_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to ManageBooksView, passing the current user
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new ManageBooksView(_currentUser));
        }


        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to user management
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to history view
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to admin settings
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var mainFrame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            mainFrame.Navigate(new LoginPage());
        }
    }
}