using Biblioteka.Data.Entities;

namespace Biblioteka.Data.Repositories
{
    public class CopyRepository : BaseRepository
    {
        public void Add(Copy copy)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO copies (book_id,status,notes)
                                VALUES (@bookId,@status,@notes);";
            cmd.Parameters.AddWithValue("@bookId", copy.BookId);
            cmd.Parameters.AddWithValue("@status", copy.Status);
            cmd.Parameters.AddWithValue("@notes", (object)copy.Notes ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public Copy GetById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id,book_id,status,notes
                                FROM copies WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Copy
            {
                Id = reader.GetInt32(0),
                BookId = reader.GetInt32(1),
                Status = reader.GetInt32(2),
                Notes = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }

        public IEnumerable<Copy> GetAll()
        {
            var list = new List<Copy>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id,book_id,status,notes FROM copies;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Copy
                {
                    Id = reader.GetInt32(0),
                    BookId = reader.GetInt32(1),
                    Status = reader.GetInt32(2),
                    Notes = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
            return list;
        }

        public void Update(Copy copy)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE copies SET book_id=@bookId,status=@status,notes=@notes WHERE id=@id;";
            cmd.Parameters.AddWithValue("@bookId", copy.BookId);
            cmd.Parameters.AddWithValue("@status", copy.Status);
            cmd.Parameters.AddWithValue("@notes", (object)copy.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", copy.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM copies WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns all available copies (status=0) for a given book.
        /// </summary>
        public IEnumerable<Copy> GetAvailableCopiesByBookId(int bookId)
        {
            var list = new List<Copy>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id,book_id,status,notes
                                FROM copies
                                WHERE book_id=@bookId AND status=0;";
            cmd.Parameters.AddWithValue("@bookId", bookId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Copy
                {
                    Id = reader.GetInt32(0),
                    BookId = reader.GetInt32(1),
                    Status = reader.GetInt32(2),
                    Notes = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
            return list;
        }
    }
}
