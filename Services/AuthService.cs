using System.Text.RegularExpressions;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Biblioteka.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly SecurityQuestionRepository _questionRepo = new SecurityQuestionRepository();

        // Validate login credentials and return user or null
        public User Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Login and password cannot be empty.");

            var user = _userRepo.GetByLogin(login);
            if (user == null) return null;
            var hashed = HashPassword(password);
            return user.Password == hashed ? user : null;
        }

        // Register new user with validation
        public void Register(string login, string password, int securityQuestionId, string securityAnswer, bool isAdmin = false)
        {
            // Validate login (alphanumeric, 3-20 chars)
            if (!Regex.IsMatch(login, "^[a-zA-Z0-9]{3,20}$"))
                throw new ArgumentException("Login must be alphanumeric and 3-20 characters long.");
            // Validate password (min 6 chars)
            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long.");
            // Validate security answer
            if (string.IsNullOrWhiteSpace(securityAnswer))
                throw new ArgumentException("Security answer cannot be empty.");

            if (_userRepo.GetByLogin(login) != null)
                throw new InvalidOperationException("User with this login already exists.");

            var user = new User
            {
                Login = login,
                Password = HashPassword(password),
                CreatedAt = DateTime.Now,
                IsAdmin = isAdmin,
                SecurityQuestionId = securityQuestionId,
                SecurityAnswer = securityAnswer.Trim()
            };
            _userRepo.Add(user);
        }

        // Reset password if answer matches
        public void ResetPassword(string login, int questionId, string answer, string newPassword)
        {
            var user = _userRepo.GetByLogin(login);
            if (user == null)
                throw new InvalidOperationException("User not found.");
            if (user.SecurityQuestionId != questionId || user.SecurityAnswer != answer)
                throw new InvalidOperationException("Security answer is incorrect.");
            if (newPassword.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long.");

            user.Password = HashPassword(newPassword);
            _userRepo.Update(user);
        }

        // Retrieve all security questions
        public IEnumerable<SecurityQuestion> GetAllSecurityQuestions()
        {
            return _questionRepo.GetAll();
        }

        // Simple SHA256 hashing
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
