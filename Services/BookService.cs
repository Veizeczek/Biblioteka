using Biblioteka.Data.Entities;

namespace Biblioteka.Services
{
    public class BookService
    {
        private readonly string _connectionString;

        public BookService()
        {
            // Build connection string similar to BaseRepository
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                                 .Parent.Parent.Parent.FullName;
            var configPath = Path.Combine(projectDir, "appsettings.json");
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
            var connBuilder = new SQLiteConnectionStringBuilder(config.GetConnectionString("DefaultConnection"));
            var dbPath = Path.Combine(projectDir, "database", Path.GetFileName(connBuilder.DataSource));
            _connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3
            }.ToString();
        }

        // Search books by title, author, or ISBN
        public IEnumerable<Book> SearchBooks(string title = null, string author = null, string isbn = null)
        {
            var results = new List<Book>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            var clauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(title)) { clauses.Add("title LIKE @title"); cmd.Parameters.AddWithValue("@title", $"%{title}%"); }
            if (!string.IsNullOrWhiteSpace(author)) { clauses.Add("author LIKE @author"); cmd.Parameters.AddWithValue("@author", $"%{author}%"); }
            if (!string.IsNullOrWhiteSpace(isbn)) { clauses.Add("isbn = @isbn"); cmd.Parameters.AddWithValue("@isbn", isbn); }
            var where = clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) : string.Empty;
            cmd.CommandText = $"SELECT id,title,author,isbn,publisher,year,pages,genre,language FROM books {where};";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new Book
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    ISBN = reader.GetString(3),
                    Publisher = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Year = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                    Pages = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                    Genre = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Language = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }
            return results;
        }

        // Loan a book: finds an available copy and creates a loan transactionally
        public void LoanBook(int userId, int bookId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            // Find an available copy
            cmd.CommandText = "SELECT id FROM copies WHERE book_id = @bookId AND status = 0 LIMIT 1;";
            cmd.Parameters.AddWithValue("@bookId", bookId);
            var copyIdObj = cmd.ExecuteScalar();
            if (copyIdObj == null)
                throw new InvalidOperationException("No available copy to loan.");
            int copyId = Convert.ToInt32(copyIdObj);

            // Update copy status to loaned (1)
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE copies SET status = 1 WHERE id = @copyId;";
            cmd.Parameters.AddWithValue("@copyId", copyId);
            cmd.ExecuteNonQuery();

            // Insert loan record
            cmd.Parameters.Clear();
            cmd.CommandText = @"INSERT INTO loans (user_id,copy_id,loan_date) VALUES (@userId,@copyId,@loanDate);";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@copyId", copyId);
            cmd.Parameters.AddWithValue("@loanDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }

        // Return a loan: sets return date and updates copy status
        public void ReturnBook(int loanId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            // Get copy ID for the loan
            cmd.CommandText = "SELECT copy_id FROM loans WHERE id = @loanId AND return_date IS NULL;";
            cmd.Parameters.AddWithValue("@loanId", loanId);
            var copyIdObj = cmd.ExecuteScalar();
            if (copyIdObj == null)
                throw new InvalidOperationException("Loan not found or already returned.");
            int copyId = Convert.ToInt32(copyIdObj);

            // Update loans table return_date
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE loans SET return_date = @returnDate WHERE id = @loanId;";
            cmd.Parameters.AddWithValue("@returnDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@loanId", loanId);
            cmd.ExecuteNonQuery();

            // Update copy status to available (0)
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE copies SET status = 0 WHERE id = @copyId;";
            cmd.Parameters.AddWithValue("@copyId", copyId);
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
    }
}
