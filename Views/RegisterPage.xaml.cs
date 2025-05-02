using Biblioteka.Services;
using Biblioteka.Data.Entities;

namespace Biblioteka.Views
{
    public partial class RegisterPage : Page
    {
        private readonly AuthService _authService = new AuthService();
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
            if (questions is IList<SecurityQuestion> list && list.Count > 0)
                QuestionCombo.SelectedIndex = 0;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var login = LoginBox.Text.Trim();
                var pwd = PasswordBox.Password;
                var confirm = ConfirmBox.Password;
                if (pwd != confirm)
                {
                    MessageBox.Show("Hasła się nie zgadzają.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var questionId = (int)QuestionCombo.SelectedValue;
                var answer = AnswerBox.Text.Trim();
                _authService.Register(login, pwd, questionId, answer);
                MessageBox.Show("Rejestracja zakończona sukcesem!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                // Powrót do logowania
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
