using Biblioteka.Data.Entities;

namespace Biblioteka.Data.Repositories
{
    public class SecurityQuestionRepository : BaseRepository
    {
        public void Add(SecurityQuestion question)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO security_questions (question) VALUES (@question);";
            cmd.Parameters.AddWithValue("@question", question.Question);
            cmd.ExecuteNonQuery();
        }

        public SecurityQuestion GetById(int questionId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT question_id,question FROM security_questions WHERE question_id=@id;";
            cmd.Parameters.AddWithValue("@id", questionId);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new SecurityQuestion
            {
                QuestionId = reader.GetInt32(0),
                Question = reader.GetString(1)
            };
        }

        public IEnumerable<SecurityQuestion> GetAll()
        {
            var list = new List<SecurityQuestion>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT question_id,question FROM security_questions;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SecurityQuestion
                {
                    QuestionId = reader.GetInt32(0),
                    Question = reader.GetString(1)
                });
            }
            return list;
        }

        public void Update(SecurityQuestion question)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE security_questions SET question=@question WHERE question_id=@id;";
            cmd.Parameters.AddWithValue("@question", question.Question);
            cmd.Parameters.AddWithValue("@id", question.QuestionId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int questionId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM security_questions WHERE question_id=@id;";
            cmd.Parameters.AddWithValue("@id", questionId);
            cmd.ExecuteNonQuery();
        }
    }
}
