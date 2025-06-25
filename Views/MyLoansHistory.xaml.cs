using System;
using System.Collections.Generic;
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
    public partial class MyLoansHistory : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private readonly User _currentUser;
        private readonly UserService _userService    = new UserService();
        private readonly CopyRepository _copyRepo    = new CopyRepository();
        private readonly BookRepository _bookRepo    = new BookRepository();

        // Pełna lista historii (do filtrowania)
        private List<HistoryLoanView> _allLoanHistory;

        // Zbindowana kolekcja
        public ObservableCollection<HistoryLoanView> LoanHistory { get; set; }

        public MyLoansHistory(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            DataContext = this;
            LoadLoanHistory();
        }

        private void LoadLoanHistory()
        {
            // Pobranie historii: Loan + tytuł książki :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}
            var history = _userService.GetLoanHistory(_currentUser.Id).ToList();

            // Mapowanie na widok i pobranie autora z repozytoriów
            var viewList = history
                .Select(tuple =>
                {
                    var loan = tuple.Loan;
                    var copy = _copyRepo.GetById(loan.CopyId);  // :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}
                    var book = _bookRepo.GetById(copy.BookId);   // :contentReference[oaicite:4]{index=4}:contentReference[oaicite:5]{index=5}

                    return new HistoryLoanView
                    {
                        BookTitle  = book.Title,
                        Author     = book.Author,
                        LoanDate   = loan.LoanDate,
                        ReturnDate = loan.ReturnDate ?? DateTime.MinValue
                    };
                })
                // Domyślnie sortuj malejąco po dacie zwrotu
                .OrderByDescending(h => h.ReturnDate)
                .ToList();

            _allLoanHistory = viewList;
            LoanHistory    = new ObservableCollection<HistoryLoanView>(viewList);
            OnPropertyChanged(nameof(LoanHistory));
        }

        // Filtrowanie po tytule lub autorze :contentReference[oaicite:6]{index=6}:contentReference[oaicite:7]{index=7}
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var term = SearchBox.Text.Trim().ToLower();
            var filtered = string.IsNullOrEmpty(term)
                ? _allLoanHistory
                : _allLoanHistory
                    .Where(h =>
                        (h.BookTitle?.ToLower().Contains(term) ?? false)
                     || (h.Author?.ToLower().Contains(term)    ?? false))
                    .ToList();

            LoanHistory = new ObservableCollection<HistoryLoanView>(filtered);
            OnPropertyChanged(nameof(LoanHistory));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new UserPage(_currentUser));
        }

        // Klasa widoku dla DataGrid
        public class HistoryLoanView
        {
            public string   BookTitle  { get; set; }
            public string   Author     { get; set; }
            public DateTime LoanDate   { get; set; }
            public DateTime ReturnDate { get; set; }
        }
    }
}
