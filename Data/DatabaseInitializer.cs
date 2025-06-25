// DatabaseInitializer.cs
using System;
using System.IO;
using System.Data.SQLite;
using Microsoft.Extensions.Configuration;

namespace Biblioteka.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            // 1) Znajdź katalog projektu (trzy poziomy w górę)
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                                 .Parent.Parent.Parent.FullName;

            // 2) Upewnij się, że katalog 'database' istnieje
            var dbDir = Path.Combine(projectDir, "database");
            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir);

            // 3) Wczytaj konfigurację z appsettings.json
            var config = new ConfigurationBuilder()
                             .SetBasePath(projectDir)
                             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                             .Build();

            // 4) Odczytaj flagi inicjalizacji bazy
            var dbInitSection = config.GetSection("DatabaseInitialization");
            bool createDatabase = true;
            string createDbStr = dbInitSection["CreateDatabase"];
            if (!string.IsNullOrEmpty(createDbStr))
                bool.TryParse(createDbStr, out createDatabase);

            bool seedSampleData = true;
            string seedDataStr = dbInitSection["SeedSampleData"];
            if (!string.IsNullOrEmpty(seedDataStr))
                bool.TryParse(seedDataStr, out seedSampleData);

            // 5) Ustal ścieżkę do pliku bazy
            var connBuilder = new SQLiteConnectionStringBuilder(
                                  config.GetConnectionString("DefaultConnection"));
            string dbFileName = Path.GetFileName(connBuilder.DataSource);
            string dbPath     = Path.Combine(dbDir, dbFileName);

            // 6) Jeśli tworzenie bazy jest wyłączone LUB plik już istnieje, zakończ
            if (!createDatabase || File.Exists(dbPath))
                return;

            // 7) Tworzymy nowy plik bazy
            SQLiteConnection.CreateFile(dbPath);

            // 8) Budujemy właściwy ConnectionString
            var actualConnString = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version    = 3
            }.ToString();

            // 9) Tworzymy schemę (tabele i indeksy)
            using (var connection = new SQLiteConnection(actualConnString))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
CREATE TABLE users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    login TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    created_at TEXT NOT NULL,
    is_admin INTEGER NOT NULL,
    security_question INTEGER NOT NULL,
    security_answer TEXT NOT NULL
);
CREATE TABLE security_questions (
    question_id INTEGER PRIMARY KEY AUTOINCREMENT,
    question TEXT NOT NULL UNIQUE
);
CREATE TABLE books (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    title TEXT NOT NULL,
    author TEXT NOT NULL,
    isbn TEXT NOT NULL UNIQUE,
    publisher TEXT,
    year INTEGER,
    pages INTEGER,
    genre TEXT,
    language TEXT
);
CREATE TABLE copies (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    book_id INTEGER NOT NULL,
    status INTEGER NOT NULL,
    notes TEXT,
    FOREIGN KEY(book_id) REFERENCES books(id)
);
CREATE TABLE loans (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    copy_id INTEGER NOT NULL,
    loan_date TEXT NOT NULL,
    return_date TEXT,
    FOREIGN KEY(user_id) REFERENCES users(id),
    FOREIGN KEY(copy_id) REFERENCES copies(id)
);

CREATE TABLE user_details (
    user_id    INTEGER PRIMARY KEY,   -- zamiast osobnego id
    first_name TEXT    NOT NULL,
    last_name  TEXT    NOT NULL,
    date_of_birth  TEXT    NOT NULL,
    phone      TEXT,
    email      TEXT,
    FOREIGN KEY(user_id) REFERENCES users(id)
);

CREATE TABLE settings (
    name  TEXT    NOT NULL PRIMARY KEY,  -- identyfikator ustawienia
    note  TEXT,                          -- opis ustawienia
    value TEXT    NOT NULL               -- wartość
);

-- Ustawienia (0 = bez limitu)
INSERT INTO settings (name, note, value) VALUES
    ('loan_period_days', 'Liczba dni na zwrot książki','14'),
    ('max_simultaneous_loans', 'Maksymalna liczba książek wypożyczonych jednocześnie', '3'),
    ('max_monthly_loans', 'Maksymalna liczba książek wypożyczonych w ciągu miesiąca', '15');

CREATE INDEX IF NOT EXISTS idx_user_details_email 
    ON user_details(email);

CREATE INDEX IF NOT EXISTS idx_books_isbn    
    ON books(isbn);
CREATE INDEX IF NOT EXISTS idx_copies_book_id 
    ON copies(book_id);
CREATE INDEX IF NOT EXISTS idx_loans_user_id  
    ON loans(user_id);
CREATE INDEX IF NOT EXISTS idx_loans_copy_id  
    ON loans(copy_id);
";

                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }

            // 10) Jeśli w appsettings włączono seeding, zasiej przykładowe dane
            if (seedSampleData)
                DefaultData.InitializeDefaults();
        }
    }
}
