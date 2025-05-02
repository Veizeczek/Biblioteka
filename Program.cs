global using System;
global using System.IO;
global using System.Data.SQLite;
global using Microsoft.Extensions.Configuration;
global using System.Windows;
global using System.Windows.Controls;
using Biblioteka.Data;

namespace Biblioteka
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                // Initialize database: create if not exists
                DatabaseInitializer.Initialize();
                DefaultData.InitializeDefaults();
                
                // Start WPF application
                var app = new Application();
                var mainWindow = new Views.MainWindow();
                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                // Determine project root (three levels up from output directory)
                var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                    .Parent.Parent.Parent.FullName;
                // Ensure 'database' directory exists in project root
                var dbDir = Path.Combine(projectDir, "database");
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // Log initialization errors to project database folder
                var logPath = Path.Combine(dbDir, "log.txt");
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] ERROR: {ex.Message}{Environment.NewLine}");

                // Show error to user
                MessageBox.Show("Wystąpił błąd podczas uruchamiania aplikacji. Sprawdź logi.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}