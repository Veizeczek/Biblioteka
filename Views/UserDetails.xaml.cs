using System;
using System.Diagnostics;
using System.Windows;
using Biblioteka.Data.Repositories;
using Biblioteka.Data.Entities;

namespace Biblioteka.Views
{
    public partial class UserDetails : Window
    {
        private readonly string _email;

        public UserDetails(int userId)
        {
            InitializeComponent();

            var user    = new UserRepository().GetById(userId);
            var details = new UserDetailsRepository().GetByUserId(userId);

            DataContext = new
            {
                Login      = user?.Login         ?? string.Empty,
                Id         = user?.Id            ?? 0,
                FirstName  = details?.FirstName  ?? string.Empty,
                LastName   = details?.LastName   ?? string.Empty,
                DateOfBirth  = details?.DateOfBirth  ?? DateTime.MinValue,
                Phone      = details?.Phone      ?? string.Empty,
                Email      = details?.Email      ?? string.Empty,
                CreatedAt  = user?.CreatedAt     ?? DateTime.MinValue
            };

            _email = details?.Email ?? string.Empty;
        }

        private void ContactEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_email))
            {
                MessageBox.Show("Brak adresu e-mail do kontaktu.", "Informacja",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{_email}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się otworzyć klienta poczty: {ex.Message}",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
