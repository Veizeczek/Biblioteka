// BrowseBooks.xaml.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;           // ← dodane
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;
using Biblioteka.Services;

namespace Biblioteka.Views
{
    public partial class BrowseBooks : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private readonly BookRepository _bookRepo      = new BookRepository();
        private readonly CopyRepository _copyRepo      = new CopyRepository();
        private readonly User _currentUser;
        private readonly string _thumbnailDir;         // ← dodane

        private List<Book> _allAvailableBooks;
        private Book _selectedBook;
        private BitmapImage _selectedBookImage;        // ← dodane

        /// <summary>
        /// Obrazek okładki aktualnie wybranej książki
        /// </summary>
        public BitmapImage SelectedBookImage
        {
            get => _selectedBookImage;
            set
            {
                if (_selectedBookImage == value) return;
                _selectedBookImage = value;
                OnPropertyChanged(nameof(SelectedBookImage));
            }
        }

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                if (_selectedBook == value) return;
                _selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));

                // ← dopisane: załaduj obraz okładki
                LoadSelectedBookImage();

                UpdateDetailsVisibility();
            }
        }

        public BrowseBooks(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            DataContext = this;

            // ← dopisane: ustawiamy ścieżkę do folderu z miniaturami
            var projectDir    = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                                     .Parent.Parent.Parent.FullName;
            _thumbnailDir     = Path.Combine(projectDir, "database", "Thumbnails");

            LoadAvailableBooks();
            UpdateDetailsVisibility(); // na start: brak wyboru
        }

        private void LoadAvailableBooks()
        {
            _allAvailableBooks = _bookRepo.GetAll()
                .Where(b => _copyRepo.GetAvailableCopiesByBookId(b.Id).Any())
                .ToList();

            AvailableBooksGrid.ItemsSource = _allAvailableBooks;
        }

        /// <summary>
        /// Ładuje SelectedBookImage z Thumbnails/{id}.png lub fallback 0.png
        /// </summary>
        private void LoadSelectedBookImage()
        {
            if (SelectedBook == null)
            {
                SelectedBookImage = null;
                return;
            }

            // ścieżka do pliku o nazwie {ID}.png
            var file = Path.Combine(_thumbnailDir, $"{SelectedBook.Id}.png");
            if (!File.Exists(file))
                file = Path.Combine(_thumbnailDir, "0.png");

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource     = new Uri(file, UriKind.Absolute);
                bmp.CacheOption   = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                SelectedBookImage = bmp;
            }
            catch
            {
                // awaryjnie: domyślne
                var fallback        = Path.Combine(_thumbnailDir, "0.png");
                SelectedBookImage  = new BitmapImage(new Uri(fallback, UriKind.Absolute));
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var term = SearchBox.Text.Trim().ToLower();
            var filtered = string.IsNullOrEmpty(term)
                ? _allAvailableBooks
                : _allAvailableBooks
                    .Where(b => (b.Title?.ToLower().Contains(term) ?? false)
                             || (b.Author?.ToLower().Contains(term) ?? false))
                    .ToList();

            AvailableBooksGrid.ItemsSource = filtered;

            if (!filtered.Contains(SelectedBook))
                SelectedBook = null;
        }

        private void AvailableBooksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedBook = AvailableBooksGrid.SelectedItem as Book;
        }

        private void UpdateDetailsVisibility()
        {
            bool has = SelectedBook != null;
            NoSelectionText.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
            DetailsContent.Visibility  = has ? Visibility.Visible   : Visibility.Collapsed;
        }

        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBook == null) return;
            try
            {
                var svc = new BookService();
                svc.LoanBook(_currentUser.Id, SelectedBook.Id);

                MessageBox.Show(
                    $"Pomyślnie wypożyczono: «{SelectedBook.Title}»",
                    "Wypożyczanie zakończone",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadAvailableBooks();
                SelectedBook = null;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Błąd wypożyczenia",
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new UserPage(_currentUser));
        }
    }
}
