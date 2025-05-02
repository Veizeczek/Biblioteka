using Biblioteka.Services;
using Biblioteka.Data.Entities;

namespace Biblioteka.Views
{
    public partial class ForgotPasswordPage : Page
    {
        private readonly AuthService _authService = new AuthService();

        public ForgotPasswordPage()
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new LoginPage());
        }

        private void RecoverButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var login = LoginBox.Text.Trim();
                var questionId = (int)QuestionCombo.SelectedValue;
                var answer = AnswerBox.Text.Trim();

                // Navigate to ChangePasswordPage with collected credentials
                var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                frame.Navigate(new ChangePasswordPage(login, questionId, answer));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd odzyskiwania hasła", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}