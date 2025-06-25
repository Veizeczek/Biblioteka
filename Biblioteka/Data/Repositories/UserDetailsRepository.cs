using System;
using System.Data;
using System.Data.SQLite;
using Biblioteka.Data.Entities;

namespace Biblioteka.Data.Repositories
{
    public class UserDetailsRepository : BaseRepository
    {
        public void Add(UserDetails d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO user_details
  (user_id, first_name, last_name, date_of_birth, phone, email)
VALUES
  (@userId, @firstName, @lastName, @dateOfBirth, @phone, @email);
";
            cmd.Parameters.AddWithValue("@userId",       d.UserId);
            cmd.Parameters.AddWithValue("@firstName",    d.FirstName);
            cmd.Parameters.AddWithValue("@lastName",     d.LastName);
            cmd.Parameters.AddWithValue("@dateOfBirth",  d.DateOfBirth.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@phone",        (object)d.Phone  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email",        (object)d.Email  ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public UserDetails GetByUserId(int userId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT user_id, first_name, last_name, date_of_birth, phone, email
  FROM user_details
 WHERE user_id = @userId;
";
            cmd.Parameters.AddWithValue("@userId", userId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new UserDetails
            {
                UserId       = r.GetInt32(0),
                FirstName    = r.GetString(1),
                LastName     = r.GetString(2),
                DateOfBirth  = DateTime.Parse(r.GetString(3)),
                Phone        = r.IsDBNull(4) ? null : r.GetString(4),
                Email        = r.IsDBNull(5) ? null : r.GetString(5)
            };
        }

        public void Update(UserDetails d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE user_details
   SET first_name    = @firstName,
       last_name     = @lastName,
       date_of_birth = @dateOfBirth,
       phone         = @phone,
       email         = @email
 WHERE user_id      = @userId;
";
            cmd.Parameters.AddWithValue("@firstName",    d.FirstName);
            cmd.Parameters.AddWithValue("@lastName",     d.LastName);
            cmd.Parameters.AddWithValue("@dateOfBirth",  d.DateOfBirth.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@phone",        (object)d.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email",        (object)d.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@userId",       d.UserId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUserId(int userId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM user_details WHERE user_id = @userId;";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}