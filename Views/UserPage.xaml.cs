using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Views
{
    public partial class UserPage : Page
    {
        private readonly User _currentUser;
        public UserPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            // PROSTE pobranie szczegółów po user.Id
            var details = new UserDetailsRepository().GetByUserId(user.Id);
            // wyświetlamy imię (jeśli jest null, fallback na login)
            var nameToShow = details?.FirstName ?? _currentUser.Login;
            GreetingTextBlock.Text = $"Witaj, {nameToShow}!";

        }

        private void BrowseBooks_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new BrowseBooks(_currentUser));
        }

        private void MyLoans_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new MyLoansPage(_currentUser));
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new MyLoansHistory(_currentUser));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
             var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new SettingsPage(_currentUser));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var mainFrame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            mainFrame.Navigate(new LoginPage());
        }
    }
}