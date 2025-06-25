// BookRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Biblioteka.Data.Entities;

namespace Biblioteka.Data.Repositories
{
    public class BookRepository : BaseRepository
    {
        /// <summary>
        /// Dodaje nową książkę, ustawiając Id = (max istniejących Id) + 1.
        /// </summary>
        public void Add(Book book)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            // policzymy najwyższe dotychczasowe Id
            using var idCmd = conn.CreateCommand();
            idCmd.CommandText = "SELECT MAX(id) FROM books;";
            var result = idCmd.ExecuteScalar();
            var maxId = (result != null && result != DBNull.Value)
                ? Convert.ToInt32(result)
                : 0;

            // przypisujemy kolejne Id
            book.Id = maxId + 1;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO books
                  (id, title, author, isbn, publisher, year, pages, genre, language)
                VALUES
                  (@id, @title, @author, @isbn,
                   @publisher, @year, @pages, @genre, @language);
            ";
            cmd.Parameters.AddWithValue("@id",        book.Id);
            cmd.Parameters.AddWithValue("@title",     book.Title);
            cmd.Parameters.AddWithValue("@author",    book.Author);
            cmd.Parameters.AddWithValue("@isbn",      book.ISBN);
            cmd.Parameters.AddWithValue("@publisher", (object)book.Publisher ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@year",      (object)book.Year      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pages",     (object)book.Pages     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@genre",     (object)book.Genre     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@language",  (object)book.Language  ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public Book GetById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, title, author, isbn, publisher, year, pages, genre, language
                  FROM books
                 WHERE id = @id;
            ";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Book
            {
                Id        = reader.GetInt32(0),
                Title     = reader.GetString(1),
                Author    = reader.GetString(2),
                ISBN      = reader.GetString(3),
                Publisher = reader.IsDBNull(4) ? null : reader.GetString(4),
                Year      = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                Pages     = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                Genre     = reader.IsDBNull(7) ? null : reader.GetString(7),
                Language  = reader.IsDBNull(8) ? null : reader.GetString(8)
            };
        }

        public IEnumerable<Book> GetAll()
        {
            var list = new List<Book>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, title, author, isbn, publisher, year, pages, genre, language
                  FROM books;
            ";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Book
                {
                    Id        = reader.GetInt32(0),
                    Title     = reader.GetString(1),
                    Author    = reader.GetString(2),
                    ISBN      = reader.GetString(3),
                    Publisher = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Year      = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                    Pages     = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                    Genre     = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Language  = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }
            return list;
        }

        public void Update(Book book)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE books 
                   SET title = @title,
                       author = @author,
                       isbn = @isbn,
                       publisher = @publisher,
                       year = @year,
                       pages = @pages,
                       genre = @genre,
                       language = @language
                 WHERE id = @id;
            ";
            cmd.Parameters.AddWithValue("@title",     book.Title);
            cmd.Parameters.AddWithValue("@author",    book.Author);
            cmd.Parameters.AddWithValue("@isbn",      book.ISBN);
            cmd.Parameters.AddWithValue("@publisher", (object)book.Publisher ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@year",      (object)book.Year      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pages",     (object)book.Pages     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@genre",     (object)book.Genre     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@language",  (object)book.Language  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id",        book.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM books WHERE id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public Book GetByISBN(string isbn)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, title, author, isbn, publisher, year, pages, genre, language
                  FROM books
                 WHERE isbn = @isbn;
            ";
            cmd.Parameters.AddWithValue("@isbn", isbn);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Book
            {
                Id        = reader.GetInt32(0),
                Title     = reader.GetString(1),
                Author    = reader.GetString(2),
                ISBN      = reader.GetString(3),
                Publisher = reader.IsDBNull(4) ? null : reader.GetString(4),
                Year      = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                Pages     = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                Genre     = reader.IsDBNull(7) ? null : reader.GetString(7),
                Language  = reader.IsDBNull(8) ? null : reader.GetString(8)
            };
        }
    }
}
