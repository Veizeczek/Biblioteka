using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Biblioteka.Data.Entities;

namespace Biblioteka.Views
{
    public partial class SettingsPage : Page
    {
        private readonly User _currentUser;

        public SettingsPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            // Pokaż dodatkowe opcje, jeśli bieżący użytkownik jest administratorem
            if (_currentUser.IsAdmin)
            {
                AdminSettingsBorder.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}
