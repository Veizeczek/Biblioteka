using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;
using Biblioteka.Services;

namespace Biblioteka.Views
{
    public partial class ManageUsersView : Page
    {
        private readonly User _currentUser;
        private readonly UserService _userService = new UserService();
        private readonly Dictionary<string, bool> _originalStates = new();
        private bool _suppressEvents = false;   // <-- tłumienie zdarzeń

        public ManageUsersView(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            RefreshUsers();
        }

        private void RefreshUsers()
        {
            _suppressEvents = true;
            var users = _userService.GetAllUsers().ToList();

            _originalStates.Clear();
            foreach (var u in users)
                _originalStates[u.Login] = u.IsAdmin;

            UsersDataGrid.ItemsSource = users;
            _suppressEvents = false;
        }

        private void AdminCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents) return;         // <-- pomijamy przy inicjalizacji
            if (!(sender is CheckBox cb) || !(cb.DataContext is User u))
                return;

            bool oldValue = _originalStates[u.Login];
            bool newValue = cb.IsChecked == true;

            // blokada zmiany dla „admin”
            if (u.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                cb.IsChecked = oldValue;  // przywróć
                return;                   // bez komunikatu
            }

            if (newValue == oldValue) return;  // nic się nie zmieniło

            // potwierdzenie
            var role = newValue ? "administratora" : "użytkownika";
            var result = MessageBox.Show(
                $"Czy na pewno chcesz nadać użytkownikowi '{u.Login}' uprawnienia {role}?",
                "Potwierdź zmianę",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (newValue) _userService.PromoteUser(u.Login);
                    else          _userService.DemoteUser(u.Login);
                    RefreshUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd przy zapisywaniu zmian: {ex.Message}",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    cb.IsChecked = oldValue;
                }
            }
            else
            {
                // anulowane przez użytkownika
                cb.IsChecked = oldValue;
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new AddUserPage(_currentUser));
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(UsersDataGrid.SelectedItem is User u))
            {
                MessageBox.Show("Wybierz najpierw użytkownika.", "Informacja",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (u.IsAdmin)
            {
                MessageBox.Show("Nie można usunąć użytkownika z uprawnieniami administratora.",
                                "Odmowa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // **Nowy fragment**: sprawdzenie aktywnych pożyczek
            var activeLoans = _userService.GetActiveLoans(u.Id);
            if (activeLoans.Any())
            {
                MessageBox.Show(
                    "Użytkownik ma nierozliczone wypożyczenia i nie może zostać usunięty.",
                    "Odmowa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var res = MessageBox.Show(
                $"Czy na pewno chcesz usunąć użytkownika '{u.Login}'?",
                "Potwierdź usunięcie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            try
            {
                _userService.DeleteUser(u.Id);
                RefreshUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas usuwania: {ex.Message}",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
       private void UserDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            // 1) Pobieramy zaznaczonego użytkownika z DataGrid
            if (!(UsersDataGrid.SelectedItem is User u))
            {
                MessageBox.Show(
                    "Wybierz najpierw użytkownika.",
                    "Informacja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // 2) Tworzymy okno szczegółów i poprawnie ustawiamy Owner
            var detailsWindow = new UserDetails(u.Id)
            {
                // Window.GetWindow(this) zwróci rodzica typu Window,
                // dzięki czemu ShowDialog naprawdę otworzy modal.
                Owner = Window.GetWindow(this)
            };

            // 3) Wyświetlamy modalnie
            detailsWindow.ShowDialog();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new AdminPage(_currentUser));
        }
    }
}
