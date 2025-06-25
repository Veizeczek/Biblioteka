using System.Windows;
using System.Windows.Controls;
using Biblioteka.Views;

namespace Biblioteka.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Navigate to LoginPage on startup
            MainFrame.Navigate(new LoginPage());
        }
    }
}