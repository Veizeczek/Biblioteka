using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;   // <— do GetByLogin()
using Biblioteka.Services;

namespace Biblioteka.Views
{
    public partial class ForgotPasswordPage : Page
    {
        private readonly AuthService _authService = new AuthService();
        private readonly UserRepository _userRepo = new UserRepository();

        public ForgotPasswordPage()
        {
            InitializeComponent();
            LoadSecurityQuestions();
        }

        private void LoadSecurityQuestions()
        {
            var questions = _authService.GetAllSecurityQuestions();
            QuestionCombo.ItemsSource       = questions;
            QuestionCombo.DisplayMemberPath = "Question";
            QuestionCombo.SelectedValuePath = "QuestionId";
            if (questions is IList<SecurityQuestion> list && list.Count > 0)
                QuestionCombo.SelectedIndex = 0;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new LoginPage());
        }

        private void RecoverButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginBox.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Podaj login.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pobieramy użytkownika z bazy
            var user = _userRepo.GetByLogin(login);
            if (user == null)
            {
                MessageBox.Show("Nie znaleziono takiego użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Jeżeli konto utworzone przez admina (brak QA)
            if (user.SecurityQuestionId == 0)
            {
                MessageBox.Show("Skontaktuj się z administratorem systemu.", "Informacja",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                // Powrót do logowania
                var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                frame.Navigate(new LoginPage());
                return;
            }

            // Dalsza część – normalne przekazanie do zmiany hasła
            var questionId = (int)QuestionCombo.SelectedValue;
            var answer     = AnswerBox.Text.Trim();

            var frameNext = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frameNext.Navigate(new ChangePasswordPage(login, questionId, answer));
        }
    }
}
