using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;
using Biblioteka.Services;

namespace Biblioteka.Views
{
    /// <summary>
    /// Code-behind for HistoryPage.xaml
    /// Loads data in background and displays a LoadingWindow during load.
    /// </summary>
    public partial class HistoryPage : Page
    {
        private readonly AdminHistoryService _service = new AdminHistoryService();

        // Master lists
        private List<HistoryCopyView> _allCopies;
        private List<HistoryUserView> _allUsers;
        private List<AdminOperationView> _allRecent;

        // Observable for UI
        private ObservableCollection<HistoryCopyView> _copies;
        private ObservableCollection<HistoryLoanAdminView> _history;
        private ObservableCollection<HistoryUserView> _users;
        private ObservableCollection<HistoryLoanAdminView> _userHistory;
        private ObservableCollection<AdminOperationView> _recent;

        public HistoryPage(User currentUser)
        {
            InitializeComponent();

            // Admin rights check
            if (!currentUser.IsAdmin)
            {
                MessageBox.Show("Brak uprawnień dostępu do tej strony.", "Odmowa dostępu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Initialize collections
            _copies = new ObservableCollection<HistoryCopyView>();
            CopiesGrid.ItemsSource = _copies;

            _history = new ObservableCollection<HistoryLoanAdminView>();
            HistoryGrid.ItemsSource = _history;

            _users = new ObservableCollection<HistoryUserView>();
            UsersGrid.ItemsSource = _users;

            _userHistory = new ObservableCollection<HistoryLoanAdminView>();
            UserHistoryGrid.ItemsSource = _userHistory;

            _recent = new ObservableCollection<AdminOperationView>();
            RecentGrid.ItemsSource = _recent;
        }

        /// <summary>
        /// On page load, display LoadingWindow, load all data asynchronously,
        /// then populate UI collections and close loading.
        /// </summary>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var loading = new LoadingWindow { Owner = Window.GetWindow(this) };
            loading.Show();

            // Fetch data off UI thread
            await Task.Run(() =>
            {
                _allCopies = _service.GetAllCopies().ToList();
                _allUsers = _service.GetAllUsers().ToList();
                _allRecent = _service.GetRecentOperationsPage(0, int.MaxValue).ToList();
            });

            // Populate UI
            _copies.Clear();
            foreach (var c in _allCopies) _copies.Add(c);

            _users.Clear();
            foreach (var u in _allUsers) _users.Add(u);

            _recent.Clear();
            foreach (var r in _allRecent) _recent.Add(r);

            loading.Close();
        }

        #region Copy History
        private async void CopiesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CopiesGrid.SelectedItem is not HistoryCopyView copy) return;

            var loading = new LoadingWindow { Owner = Window.GetWindow(this) };
            loading.Show();

            List<HistoryLoanAdminView> hist = null;
            await Task.Run(() =>
            {
                hist = _service.GetLoanHistoryForCopy(copy.CopyId)
                               .OrderByDescending(x => x.LoanDate)
                               .ToList();
            });

            _history.Clear();
            foreach (var h in hist) _history.Add(h);

            loading.Close();
        }

        // Stubs for XAML events
        private void CopiesGrid_Loaded(object sender, RoutedEventArgs e) { }
        private void HistoryGrid_Loaded(object sender, RoutedEventArgs e) { }
        private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void CopiesGrid_Sorting(object sender, DataGridSortingEventArgs e) { }
        #endregion

        #region Recent Operations
        private void RecentGrid_Loaded(object sender, RoutedEventArgs e) { }
        private void RecentGrid_Sorting(object sender, DataGridSortingEventArgs e) { }
        #endregion

        #region User History
        private async void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not HistoryUserView user) return;

            var loading = new LoadingWindow { Owner = Window.GetWindow(this) };
            loading.Show();

            List<HistoryLoanAdminView> hist = null;
            await Task.Run(() =>
            {
                hist = _service.GetLoanHistoryForUser(user.UserId)
                               .OrderByDescending(x => x.LoanDate)
                               .ToList();
            });

            _userHistory.Clear();
            foreach (var h in hist) _userHistory.Add(h);

            loading.Close();
        }

        private void UsersGrid_Loaded(object sender, RoutedEventArgs e) { }
        private void UserHistoryGrid_Loaded(object sender, RoutedEventArgs e) { }
        private void UsersGrid_Sorting(object sender, DataGridSortingEventArgs e) { }
        #endregion

        #region Search & Navigation
       private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = SearchBox.Text.Trim().ToLower();

            // 1) Jeśli przeglądamy egzemplarze
            if (MainTab.SelectedItem == TabCopies)
            {
                _copies.Clear();
                foreach (var c in _allCopies
                    .Where(x => x.DisplayName.ToLower().Contains(q)
                            || x.CopyId.ToString().Contains(q)))
                {
                    _copies.Add(c);
                }
            }
            // 2) Jeśli przeglądamy ostatnie operacje
            else if (MainTab.SelectedItem == TabRecent)
            {
                _recent.Clear();
                foreach (var r in _allRecent
                    .Where(x => x.Type.ToLower().Contains(q)
                            || x.BookTitle.ToLower().Contains(q)
                            || x.UserLogin.ToLower().Contains(q)))
                {
                    _recent.Add(r);
                }
            }
            // 3) Jeśli przeglądamy użytkowników
            else if (MainTab.SelectedItem == TabUsers)
            {
                _users.Clear();
                foreach (var u in _allUsers
                    .Where(x => x.DisplayName.ToLower().Contains(q)
                            || x.UserId.ToString().Contains(q)))
                {
                    _users.Add(u);
                }
            }
        }
        private void SearchBox_Search(object sender, RoutedEventArgs e)
        {
            SearchBox_TextChanged(sender, null);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow.FindName("MainFrame") as Frame)?.GoBack();
        }
        #endregion
    }
}
