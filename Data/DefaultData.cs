namespace Biblioteka.Data
{
    /// <summary>
    /// Inserts default data into a newly created database: security questions and admin user.
    /// </summary>
    public static class DefaultData
    {
        public static void InitializeDefaults()
        {
            // Determine project root directory
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                                 .Parent.Parent.Parent.FullName;
            var dbDir = Path.Combine(projectDir, "database");

            // Load configuration
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // Build DB path
            var connBuilder = new SQLiteConnectionStringBuilder(config.GetConnectionString("DefaultConnection"));
            var dbPath = Path.Combine(dbDir, Path.GetFileName(connBuilder.DataSource));

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var cmd = connection.CreateCommand();

            // Insert default security question if missing
            cmd.CommandText = @"
INSERT INTO security_questions (question)
SELECT 'What is your mother''s maiden name?'
WHERE NOT EXISTS (SELECT 1 FROM security_questions);
";
            cmd.ExecuteNonQuery();

            // Insert default admin user (SHA256('admin') Base64)
            // jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
            cmd.CommandText = @"
INSERT INTO users (login, password, created_at, is_admin, security_question, security_answer)
SELECT 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', @created, 1, 1, ''
WHERE NOT EXISTS (SELECT 1 FROM users);
";
            cmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery();
// Insert sample books
            cmd.CommandText = @"
INSERT OR IGNORE INTO books (title,author,isbn,publisher,year,pages,genre,language) VALUES
('Duma i uprzedzenie','Jane Austen','9780141439518','Penguin Classics',1813,432,'Powieść','angielski'),
('Moby Dick','Herman Melville','9780142437247','Penguin Classics',1851,720,'Powieść przygodowa','angielski'),
('Wojna i pokój','Lew Tołstoj','9780199232765','Oxford University Press',1869,1392,'Powieść historyczna','rosyjski'),
('Don Kichot','Miguel de Cervantes','9780060934347','Harper Perennial Modern Classics',1605,992,'Powieść','hiszpański'),
('Boska komedia','Dante Alighieri','9780140448955','Penguin Classics',1320,784,'Poemat epicki','włoski'),
('Zbrodnia i kara','Fiodor Dostojewski','9780140449136','Penguin Classics',1866,576,'Powieść psychologiczna','rosyjski'),
('Bracia Karamazow','Fiodor Dostojewski','9780374528379','Farrar, Straus and Giroux',1880,824,'Powieść filozoficzna','rosyjski'),
('Ulisses','James Joyce','9780199535675','Oxford World''s Classics',1922,736,'Powieść modernistyczna','angielski'),
('Pani Bovary','Gustave Flaubert','9780140449129','Penguin Classics',1856,352,'Powieść','francuski'),
('Odyseja','Homer','9780140268867','Penguin Classics',-800,560,'Poemat epicki','grecki');
";
            cmd.ExecuteNonQuery();
            transaction.Commit();
        }
    }
}