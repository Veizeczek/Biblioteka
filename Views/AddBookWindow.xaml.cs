// AddBookWindow.xaml.cs
using System;
using System.Windows;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Views
{
    public partial class AddBookWindow : Window
    {
        private readonly BookRepository _bookRepo = new BookRepository();

        public AddBookWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var book = new Book
                {
                    Title     = TitleBox.Text.Trim(),
                    Author    = AuthorBox.Text.Trim(),
                    ISBN      = IsbnBox.Text.Trim(),
                    Publisher = PublisherBox.Text.Trim(),
                    Year      = int.TryParse(YearBox.Text, out var y) ? y : (int?)null,
                    Pages     = int.TryParse(PagesBox.Text, out var p) ? p : (int?)null,
                    Genre     = GenreBox.Text.Trim(),
                    Language  = LanguageBox.Text.Trim()
                };

                _bookRepo.Add(book);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania książki: {ex.Message}", 
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
            finally
            {
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
