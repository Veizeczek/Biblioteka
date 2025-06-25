using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Services;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Views
{
    public partial class RegisterPage : Page
    {
        private readonly AuthService _authService = new AuthService();
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly UserDetailsRepository _detailsRepo = new UserDetailsRepository();
        private string _lastGeneratedLogin = string.Empty;

        public RegisterPage()
        {
            InitializeComponent();
            LoadSecurityQuestions();
        }

        private void LoadSecurityQuestions()
        {
            var questions = _authService.GetAllSecurityQuestions();
            QuestionCombo.ItemsSource = questions;
            QuestionCombo.DisplayMemberPath = "Question";
            QuestionCombo.SelectedValuePath = "QuestionId";
            if (questions is IList list && list.Count > 0)
                QuestionCombo.SelectedIndex = 0;
        }

        // Event to update default login as user types name
        private void NameBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            var fn = FirstNameBox.Text.Trim();
            var ln = LastNameBox.Text.Trim();

            // Generuj tylko jeśli imię i nazwisko mają co najmniej 3 znaki
            if (fn.Length < 3 || ln.Length < 3)
                return;

            string genFn = fn.Substring(0, 3);
            string genLn = ln.Substring(0, 3);
            var suggestion = (genFn + genLn).ToLowerInvariant();

            // Nadpisuj tylko, gdy pole jest puste lub zawiera poprzednio wygenerowaną wartość
            if (string.IsNullOrEmpty(LoginBox.Text) || LoginBox.Text == _lastGeneratedLogin)
            {
                LoginBox.Text = suggestion;
                _lastGeneratedLogin = suggestion;
            }
        }


        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1) Imię i nazwisko – wymagane
                var firstName = FirstNameBox.Text.Trim();
                var lastName = LastNameBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    MessageBox.Show("Imię i nazwisko nie mogą być puste.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1b) Data urodzenia – wymagane
                if (BirthDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Musisz podać datę urodzenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var dateOfBirth = BirthDatePicker.SelectedDate.Value;

                // 2) Login – już wypełniony lub ręcznie zmieniony
                var login = LoginBox.Text.Trim().ToLowerInvariant();

                // 3) Numer telefonu – opcjonalny, ale jeśli podany, musi mieć format 999-999-999
                var phone = PhoneBox.Text.Trim();
                if (!string.IsNullOrEmpty(phone) &&
                    !Regex.IsMatch(phone, @"^\d{3}-\d{3}-\d{3}$"))
                {
                    MessageBox.Show("Numer telefonu musi mieć format 999-999-999.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 4) Email – opcjonalny, ale jeśli podany, musi zawierać '@'
                var email = EmailBox.Text.Trim();
                if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
                {
                    MessageBox.Show("Adres e-mail musi zawierać znak '@'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 5) Hasło i potwierdzenie – wymagane
                var pwd = PasswordBox.Password;
                var confirm = ConfirmBox.Password;
                if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 6)
                {
                    MessageBox.Show("Hasło musi mieć co najmniej 6 znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (pwd != confirm)
                {
                    MessageBox.Show("Hasła się nie zgadzają.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 6) Pytanie i odpowiedź – wymagane
                var questionId = (int)QuestionCombo.SelectedValue;
                var answer = AnswerBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(answer))
                {
                    MessageBox.Show("Odpowiedź na pytanie bezpieczeństwa nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 7) Rejestracja użytkownika + szczegóły
                _authService.Register(
                    login,
                    pwd,
                    questionId,
                    answer,
                    firstName,
                    lastName,
                    dateOfBirth,
                    phone,
                    email,
                    false
                );

                MessageBox.Show("Rejestracja zakończona sukcesem!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);

                // Powrót do strony logowania
                var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                frame.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new LoginPage());
        }
    }
}