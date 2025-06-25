using Biblioteka.Data.Entities;

namespace Biblioteka.Data.Repositories
{
    public class LoanRepository : BaseRepository
    {
        public void Add(Loan loan)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO loans (user_id,copy_id,loan_date,return_date)
                                VALUES (@userId,@copyId,@loanDate,@returnDate);";
            cmd.Parameters.AddWithValue("@userId", loan.UserId);
            cmd.Parameters.AddWithValue("@copyId", loan.CopyId);
            cmd.Parameters.AddWithValue("@loanDate", loan.LoanDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@returnDate", loan.ReturnDate.HasValue ? (object)loan.ReturnDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public Loan GetById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id,user_id,copy_id,loan_date,return_date
                                FROM loans WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Loan
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                CopyId = reader.GetInt32(2),
                LoanDate = DateTime.Parse(reader.GetString(3)),
                ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4))
            };
        }

        public IEnumerable<Loan> GetAll()
        {
            var list = new List<Loan>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id,user_id,copy_id,loan_date,return_date FROM loans;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Loan
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CopyId = reader.GetInt32(2),
                    LoanDate = DateTime.Parse(reader.GetString(3)),
                    ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4))
                });
            }
            return list;
        }

        public void Update(Loan loan)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE loans SET user_id=@userId,copy_id=@copyId,loan_date=@loanDate,return_date=@returnDate WHERE id=@id;";
            cmd.Parameters.AddWithValue("@userId", loan.UserId);
            cmd.Parameters.AddWithValue("@copyId", loan.CopyId);
            cmd.Parameters.AddWithValue("@loanDate", loan.LoanDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@returnDate", loan.ReturnDate.HasValue ? (object)loan.ReturnDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value);
            cmd.Parameters.AddWithValue("@id", loan.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM loans WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Retrieves all loans made by a specific user.
        /// </summary>
        public IEnumerable<Loan> GetLoansByUser(int userId)
        {
            var list = new List<Loan>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id,user_id,copy_id,loan_date,return_date
                                FROM loans WHERE user_id=@userId;";
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Loan
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CopyId = reader.GetInt32(2),
                    LoanDate = DateTime.Parse(reader.GetString(3)),
                    ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4))
                });
            }
            return list;
        }

        /// <summary>
        /// Retrieves all loans for a specific copy.
        /// </summary>
        public IEnumerable<Loan> GetLoansByCopy(int copyId)
        {
            var list = new List<Loan>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id,user_id,copy_id,loan_date,return_date
                                FROM loans WHERE copy_id=@copyId;";
            cmd.Parameters.AddWithValue("@copyId", copyId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Loan
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CopyId = reader.GetInt32(2),
                    LoanDate = DateTime.Parse(reader.GetString(3)),
                    ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4))
                });
            }
            return list;
        }
        public IEnumerable<Loan> GetLoansPageDescending(int skip, int take)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT id, user_id, copy_id, loan_date, return_date
                FROM loans
            ORDER BY id DESC
            LIMIT @take OFFSET @skip;";
            cmd.Parameters.AddWithValue("@take", take);
            cmd.Parameters.AddWithValue("@skip", skip);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Loan {
                    Id         = reader.GetInt32(0),
                    UserId     = reader.GetInt32(1),
                    CopyId     = reader.GetInt32(2),
                    LoanDate   = DateTime.Parse(reader.GetString(3)),
                    ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4))
                };
            }
        }

    }
}
