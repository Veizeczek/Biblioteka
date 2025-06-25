using Biblioteka.Services;

namespace Biblioteka.Views
{
    public partial class ChangePasswordPage : Page
    {
        private readonly AuthService _authService = new AuthService();
        private readonly string _login;
        private readonly int _questionId;
        private readonly string _answer;

        public ChangePasswordPage(string login, int questionId, string answer)
        {
            InitializeComponent();
            _login = login;
            _questionId = questionId;
            _answer = answer;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
            frame.Navigate(new ForgotPasswordPage());
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newPwd = NewPasswordBox.Password;
                var confirm = ConfirmBox.Password;
                if (newPwd != confirm)
                    throw new ApplicationException("Hasła nie są zgodne.");

                // Reset password using stored questionId and answer
                _authService.ResetPassword(_login, _questionId, _answer, newPwd);
                MessageBox.Show("Hasło zostało zmienione pomyślnie!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);

                var frame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                frame.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd zmiany hasła", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}