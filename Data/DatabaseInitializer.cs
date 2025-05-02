namespace Biblioteka.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            // Determine project root directory (three levels up from output folder)
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                                 .Parent.Parent.Parent.FullName;

            // Ensure 'database' directory exists in project root
            var dbDir = Path.Combine(projectDir, "database");
            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir);

            // Ensure log file exists
            var logPath = Path.Combine(dbDir, "log.txt");
            if (!File.Exists(logPath))
                File.WriteAllText(logPath, string.Empty);

            // Locate appsettings.json in project root
            var appsettingsPath = Path.Combine(projectDir, "appsettings.json");
            if (!File.Exists(appsettingsPath))
                throw new FileNotFoundException($"Configuration file not found: {appsettingsPath}");

            // Load configuration
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // Read connection string and extract file name
            string connectionString = config.GetConnectionString("DefaultConnection");
            var connBuilder = new SQLiteConnectionStringBuilder(connectionString);
            string dbFileName = Path.GetFileName(connBuilder.DataSource);

            // Full path to database file in project/database
            string dbPath = Path.Combine(dbDir, dbFileName);

            // Create database file if it doesn't exist
            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);

            // Build actual connection string to the new path
            var actualConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3
            }.ToString();

            // Initialize schema only: tables and indexes
            using (var connection = new SQLiteConnection(actualConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    login TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    created_at TEXT NOT NULL,
    is_admin INTEGER NOT NULL,
    security_question INTEGER NOT NULL,
    security_answer TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS security_questions (
    question_id INTEGER PRIMARY KEY AUTOINCREMENT,
    question TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS books (
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

CREATE TABLE IF NOT EXISTS copies (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    book_id INTEGER NOT NULL,
    status INTEGER NOT NULL,
    notes TEXT,
    FOREIGN KEY(book_id) REFERENCES books(id)
);

CREATE TABLE IF NOT EXISTS loans (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    copy_id INTEGER NOT NULL,
    loan_date TEXT NOT NULL,
    return_date TEXT,
    FOREIGN KEY(user_id) REFERENCES users(id),
    FOREIGN KEY(copy_id) REFERENCES copies(id)
);

CREATE INDEX IF NOT EXISTS idx_books_isbn ON books(isbn);
CREATE INDEX IF NOT EXISTS idx_copies_book_id ON copies(book_id);
CREATE INDEX IF NOT EXISTS idx_loans_user_id ON loans(user_id);
CREATE INDEX IF NOT EXISTS idx_loans_copy_id ON loans(copy_id);
";
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
    }
}