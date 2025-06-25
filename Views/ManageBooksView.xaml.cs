// ManageBooksView.xaml.cs
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;
using Biblioteka.Services;

namespace Biblioteka.Views
{
    public partial class ManageBooksView : Page, INotifyPropertyChanged
    {
        private readonly BookRepository _bookRepo = new BookRepository();
        private readonly CopyRepository _copyRepo = new CopyRepository();
        private readonly CsvService _csvService;
        private readonly User _currentUser;

        private List<Book> _allBooks;
        private Book _selectedBook;
        private ObservableCollection<CopyDetail> _selectedBookCopies;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));
                OnPropertyChanged(nameof(SelectedBookTitle));
                OnPropertyChanged(nameof(SelectedBookAuthor));
                OnPropertyChanged(nameof(SelectedBookISBN));
                OnPropertyChanged(nameof(SelectedBookPublisher));
                OnPropertyChanged(nameof(SelectedBookYear));
                OnPropertyChanged(nameof(SelectedBookPages));
                OnPropertyChanged(nameof(SelectedBookGenre));
                OnPropertyChanged(nameof(SelectedBookLanguage));
                LoadCopies();
            }
        }

        public ObservableCollection<CopyDetail> SelectedBookCopies
        {
            get => _selectedBookCopies;
            set { _selectedBookCopies = value; OnPropertyChanged(nameof(SelectedBookCopies)); }
        }

        public string SelectedBookTitle     => SelectedBook?.Title     ?? string.Empty;
        public string SelectedBookAuthor    => SelectedBook?.Author    ?? string.Empty;
        public string SelectedBookISBN      => SelectedBook?.ISBN      ?? string.Empty;
        public string SelectedBookPublisher => SelectedBook?.Publisher ?? string.Empty;
        public string SelectedBookYear      => SelectedBook?.Year?.ToString()  ?? string.Empty;
        public string SelectedBookPages     => SelectedBook?.Pages?.ToString() ?? string.Empty;
        public string SelectedBookGenre     => SelectedBook?.Genre     ?? string.Empty;
        public string SelectedBookLanguage  => SelectedBook?.Language  ?? string.Empty;

        public ManageBooksView(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = this;
            _csvService = new CsvService(_bookRepo);

            LoadBooks();
            BooksGrid.UnselectAll();

            // pozwala stronie przechwycić naciśnięcie klawisza Delete
            this.Focusable = true;
            this.PreviewKeyDown += ManageBooksView_PreviewKeyDown;
        }

        private void ManageBooksView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // logika w DeleteBookButton_Click już obsługuje brak zaznaczenia
                DeleteBookButton_Click(null, null);
                e.Handled = true;
            }
        }

        private void LoadBooks()
        {
            _allBooks = _bookRepo.GetAll().ToList();
            BooksGrid.ItemsSource = _allBooks;
            SelectedBook = null;
            BooksGrid.UnselectAll();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchTextBox.Text.Trim();
            var filtered = _allBooks
                .Where(b =>
                    (!string.IsNullOrEmpty(b.Title)  && b.Title.   IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.Author) && b.Author.  IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                    (b.Year != null                  && b.Year.ToString().IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                )
                .ToList();

            BooksGrid.ItemsSource = filtered;
            if (filtered.Any())
            {
                BooksGrid.SelectedItem = filtered.First();
            }
            else
            {
                SelectedBook = null;
                SelectedBookCopies?.Clear();
            }
        }

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book b)
                SelectedBook = b;
            else
                SelectedBook = null;

            BooksGrid.Focus();  // ← added to ensure Delete key is received by the grid
        }

        private void BooksGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && BooksGrid.IsKeyboardFocusWithin)
            {
                if (SelectedBook == null)
                {
                    MessageBox.Show(
                        "Nie wybrano żadnej książki do usunięcia.",
                        "Brak wyboru",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
                else
                {
                    DeleteBookButton_Click(null, null);
                }
                e.Handled = true;
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBook == null)
            {
                MessageBox.Show(
                    "Nie wybrano żadnej książki do usunięcia.",
                    "Brak wyboru",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var msg = $"Usunąć książkę „{SelectedBook.Title}” wraz ze wszystkimi jej egzemplarzami?";
            if (MessageBox.Show(msg, "Usuń książkę", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                var copies = _copyRepo.GetAll()
                                     .Where(c => c.BookId == SelectedBook.Id)
                                     .ToList();
                foreach (var copy in copies)
                    _copyRepo.Delete(copy.Id);

                _bookRepo.Delete(SelectedBook.Id);
                LoadBooks();
            }
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddBookWindow { Owner = Window.GetWindow(this) };
            if (addWindow.ShowDialog() == true)
                LoadBooks();
        }

        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "CSV (*.csv)|*.csv", Title = "Importuj książki z pliku CSV" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _csvService.ImportBooks(dlg.FileName);
                LoadBooks();
                MessageBox.Show("Import zakończony pomyślnie.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd importu CSV:\n{ex.Message}", "Błąd importu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", Title = "Eksportuj książki do pliku CSV", FileName = "books_export.csv" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _csvService.ExportBooks(dlg.FileName);
                MessageBox.Show("Eksport zakończony pomyślnie.", "Eksport CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd eksportu CSV:\n{ex.Message}", "Błąd eksportu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
            if (sender is Button btn && btn.Tag is int id &&
                MessageBox.Show("Usunąć zaznaczony egzemplarz?", "Usuń egzemplarz", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
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

        private void LoadCopies()
        {
            if (SelectedBook != null)
            {
                SelectedBookCopies = new ObservableCollection<CopyDetail>(
                    _copyRepo.GetAll()
                             .Where(c => c.BookId == SelectedBook.Id)
                             .Select(c => new CopyDetail(c, _copyRepo))
                );
            }
            else
            {
                SelectedBookCopies = new ObservableCollection<CopyDetail>();
            }
        }

        public class CopyDetail : INotifyPropertyChanged
        {
            private readonly Copy _model;
            private readonly CopyRepository _repo;
            private string _notes;
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string prop) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

            public int Id => _model.Id;
            public string StatusText => _model.Status switch
            {
                1 => "wypożyczona",
                2 => "uszkodzona",
                _ => "dostępna",
            };

            public string Notes
            {
                get => _notes;
                set
                {
                    if (_notes != value)
                    {
                        _notes = value;
                        _model.Notes = value;
                        _repo.Update(_model);
                        OnPropertyChanged(nameof(Notes));
                    }
                }
            }

            public CopyDetail(Copy model, CopyRepository repo)
            {
                _model = model;
                _repo = repo;
                _notes = model.Notes;
            }
        }
    }
}
