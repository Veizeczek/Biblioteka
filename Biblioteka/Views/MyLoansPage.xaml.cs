using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;
using Biblioteka.Services;

namespace Biblioteka.Views
{
    public partial class MyLoansPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private readonly User _currentUser;
        private readonly UserService _userService = new UserService();
        private readonly CopyRepository _copyRepo = new CopyRepository();
        private readonly BookRepository _bookRepo = new BookRepository();
        private readonly BookService _bookService = new BookService();

        public ObservableCollection<ActiveLoanView> ActiveLoans { get; set; }

        public MyLoansPage(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            DataContext = this;
            LoadActiveLoans();
        }

        private void LoadActiveLoans()
        {
            // Pobierz wszystkie aktywne wypożyczenia
            var loans = _userService.GetActiveLoans(_currentUser.Id)
                                    .ToList(); // :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}

            // Przekonwertuj je na widok z tytułem i datą
            var viewList = loans.Select(l =>
            {
                var copy = _copyRepo.GetById(l.CopyId);                            // :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}
                var book = _bookRepo.GetById(copy.BookId);                         // :contentReference[oaicite:4]{index=4}:contentReference[oaicite:5]{index=5}
                return new ActiveLoanView
                {
                    LoanId    = l.Id,
                    BookTitle = book.Title,
                    LoanDate  = l.LoanDate
                };
            }).ToList();

            ActiveLoans = new ObservableCollection<ActiveLoanView>(viewList);
            OnPropertyChanged(nameof(ActiveLoans));
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            int loanId = (int)((Button)sender).Tag;
            try
            {
                _bookService.ReturnBook(loanId); // :contentReference[oaicite:6]{index=6}:contentReference[oaicite:7]{index=7}
                MessageBox.Show(
                    "Książka została zwrócona.",
                    "Zwrot książki",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                LoadActiveLoans();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Błąd zwrotu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Wystąpił błąd: {ex.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ReportProblemButton_Click(object sender, RoutedEventArgs e)
        {
            // Na razie tylko placeholder
            MessageBox.Show(
                "Funkcja zgłaszania problemu jest w przygotowaniu.",
                "Zgłoś problem",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // Klasa wspomagająca do bindowania w DataGrid
        public class ActiveLoanView
        {
            public int LoanId { get; set; }
            public string BookTitle { get; set; }
            public DateTime LoanDate { get; set; }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new UserPage(_currentUser));
        }
    }
}
