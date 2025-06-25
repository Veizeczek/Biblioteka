using Biblioteka.Data.Entities;

namespace Biblioteka.Data.Repositories
{
    public class UserRepository : BaseRepository
    {
        public void Add(User user)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO users (login,password,created_at,is_admin,security_question,security_answer) 
                                VALUES (@login,@password,@created,@admin,@question,@answer);";
            cmd.Parameters.AddWithValue("@login", user.Login);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@created", user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@admin", user.IsAdmin ? 1 : 0);
            cmd.Parameters.AddWithValue("@question", user.SecurityQuestionId);
            cmd.Parameters.AddWithValue("@answer", user.SecurityAnswer);
            cmd.ExecuteNonQuery();
        }

        public User GetById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id,login,password,created_at,is_admin,security_question,security_answer FROM users WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                Password = reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3)),
                IsAdmin = reader.GetInt32(4) == 1,
                SecurityQuestionId = reader.GetInt32(5),
                SecurityAnswer = reader.GetString(6)
            };
        }

        public User GetByLogin(string login)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id,login,password,created_at,is_admin,security_question,security_answer FROM users WHERE login=@login;";
            cmd.Parameters.AddWithValue("@login", login);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                Password = reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3)),
                IsAdmin = reader.GetInt32(4) == 1,
                SecurityQuestionId = reader.GetInt32(5),
                SecurityAnswer = reader.GetString(6)
            };
        }

        public IEnumerable<User> GetAll()
        {
            var list = new List<User>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id,login,password,created_at,is_admin,security_question,security_answer FROM users;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    IsAdmin = reader.GetInt32(4) == 1,
                    SecurityQuestionId = reader.GetInt32(5),
                    SecurityAnswer = reader.GetString(6)
                });
            }
            return list;
        }

        public void Update(User user)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE users
                   SET login             = @login,
                       password          = @password,
                       is_admin          = @admin,
                       security_question = @question,
                       security_answer   = @answer
                 WHERE id = @id;";
            cmd.Parameters.AddWithValue("@login",    user.Login);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@admin",    user.IsAdmin ? 1 : 0);
            cmd.Parameters.AddWithValue("@question", user.SecurityQuestionId);
            cmd.Parameters.AddWithValue("@answer",   user.SecurityAnswer);
            cmd.Parameters.AddWithValue("@id",       user.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM users WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        public IEnumerable<User> GetPage(int skip, int take)
        {
            var list = new List<User>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT id, login, password, created_at, is_admin, security_question, security_answer
                FROM users
            ORDER BY id
            LIMIT @take OFFSET @skip;";
            cmd.Parameters.AddWithValue("@take", take);
            cmd.Parameters.AddWithValue("@skip", skip);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new User
                {
                    Id                 = reader.GetInt32(0),
                    Login              = reader.GetString(1),
                    Password           = reader.GetString(2),
                    CreatedAt          = DateTime.Parse(reader.GetString(3)),
                    IsAdmin            = reader.GetInt32(4) == 1,
                    SecurityQuestionId = reader.GetInt32(5),
                    SecurityAnswer     = reader.GetString(6)
                });
            }
            return list;
        }

    }
}