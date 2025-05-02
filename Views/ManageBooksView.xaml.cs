using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Views
{
    public partial class ManageBooksView : Page, INotifyPropertyChanged
    {
        private readonly BookRepository _bookRepo = new BookRepository();
        private readonly CopyRepository _copyRepo = new CopyRepository();
        private readonly User _currentUser;
        private Book _selectedBook;
        private List<CopyDetail> _selectedBookCopies;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));
                LoadCopies();
            }
        }

        public List<CopyDetail> SelectedBookCopies
        {
            get => _selectedBookCopies;
            set { _selectedBookCopies = value; OnPropertyChanged(nameof(SelectedBookCopies)); }
        }

        public ManageBooksView(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = this;
            LoadBooks();
        }

        private void LoadBooks()
        {
            var books = _bookRepo.GetAll().ToList();
            BooksGrid.ItemsSource = books;
            if (books.Any())
            {
                SelectedBook = books.First();
                BooksGrid.SelectedItem = SelectedBook;
            }
        }

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book b)
                SelectedBook = b;
        }

        private void LoadCopies()
        {
            if (SelectedBook != null)
            {
                SelectedBookCopies = _copyRepo.GetAll()
                    .Where(c => c.BookId == SelectedBook.Id)
                    .Select(c => new CopyDetail(c))
                    .ToList();
            }
            else
            {
                SelectedBookCopies = new List<CopyDetail>();
            }
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Dodaj książkę - TBD");
        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBook != null && MessageBox.Show($"Usunąć książkę '{SelectedBook.Title}'?", "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _bookRepo.Delete(SelectedBook.Id);
                LoadBooks();
            }
        }
        private void ImportCsvButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Import CSV - TBD");
        private void ExportCsvButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Eksport CSV - TBD");
        private void AddCopy_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBook != null)
            {
                _copyRepo.Add(new Copy { BookId = SelectedBook.Id, Status = 0, Notes = string.Empty });
                LoadCopies();
            }
        }
        private void DeleteCopy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id && MessageBox.Show($"Usunąć egzemplarz {id}?", "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _copyRepo.Delete(id);
                LoadCopies();
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new AdminPage(_currentUser));
        }

        // ViewModel for Copy entity with status mapping
        public class CopyDetail
        {
            public int Id { get; }
            public string StatusText { get; }
            public string Notes { get; set; }

            public CopyDetail(Copy c)
            {
                Id = c.Id;
                Notes = c.Notes;
                StatusText = c.Status switch
                {
                    1 => "wypożyczona",
                    2 => "uszkodzona",
                    _ => "dostępna",
                };
            }
        }
    }
}
