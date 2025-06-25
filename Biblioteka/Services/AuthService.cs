using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo             = new UserRepository();
        private readonly SecurityQuestionRepository _questionRepo = new SecurityQuestionRepository();
        private readonly UserDetailsRepository _detailsRepo   = new UserDetailsRepository();

        public User Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Login and password cannot be empty.");

            var user = _userRepo.GetByLogin(login);
            if (user == null) return null;

            var hashed = HashPassword(password);
            return user.Password == hashed ? user : null;
        }

        // self-service registration (wymaga QA + danych personalnych)
        public void Register(
            string login,
            string password,
            int securityQuestionId,
            string securityAnswer,
            string firstName,
            string lastName,
            DateTime dateOfBirth,     // ← nowy parametr
            string phone,
            string email,
            bool isAdmin = false)
        {
            // login: alfanumeryczny 3–20 znaków
            if (!Regex.IsMatch(login, "^[a-zA-Z0-9]{3,20}$"))
                throw new ArgumentException("Login musi być alfanumeryczny i mieć 3–20 znaków.");
            // hasło: min 6 znaków
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ArgumentException("Hasło musi mieć co najmniej 6 znaków.");
            // imię i nazwisko: nie mogą być puste
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Imię i nazwisko nie mogą być puste.");
            // data urodzenia musi być nie później niż dziś
            if (dateOfBirth > DateTime.Today)
                throw new ArgumentException("Data urodzenia nie może być z przyszłości.");
            // telefon: jeśli podany, format 999-999-999
            if (!string.IsNullOrEmpty(phone) && !Regex.IsMatch(phone, @"^\d{3}-\d{3}-\d{3}$"))
                throw new ArgumentException("Numer telefonu musi mieć format 999-999-999.");
            // email: jeśli podany, musi zawierać '@'
            if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
                throw new ArgumentException("Adres e-mail musi zawierać znak '@'.");

            if (!isAdmin)
            {
                if (securityQuestionId <= 0)
                    throw new ArgumentException("Musisz wybrać pytanie pomocnicze.");
                if (string.IsNullOrWhiteSpace(securityAnswer))
                    throw new ArgumentException("Musisz podać odpowiedź na pytanie pomocnicze.");
            }

            if (_userRepo.GetByLogin(login) != null)
                throw new InvalidOperationException("Użytkownik o takim loginie już istnieje.");

            // 1) dodajemy podstawowe konto w tabeli users
            var user = new User
            {
                Login              = login,
                Password           = HashPassword(password),
                CreatedAt          = DateTime.Now,
                IsAdmin            = isAdmin,
                SecurityQuestionId = isAdmin ? 0 : securityQuestionId,
                SecurityAnswer     = isAdmin ? string.Empty : securityAnswer.Trim()
            };
            _userRepo.Add(user);

            // 2) pobieramy dopiero co utworzone konto (żeby mieć Id)
            var created = _userRepo.GetByLogin(login)
                          ?? throw new InvalidOperationException("Błąd tworzenia konta.");

            // 3) zapisujemy dane w user_details
            var details = new UserDetails
            {
                UserId      = created.Id,
                FirstName   = firstName.Trim(),
                LastName    = lastName.Trim(),
                DateOfBirth = dateOfBirth,
                Phone       = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                Email       = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
            };
            _detailsRepo.Add(details);
        }

        // adminowskie dodawanie (bez QA i bez danych dodatkowych)
        public void Register(string login, string password, bool isAdmin)
        {
            // tej metody użyj, jeśli chcesz tylko utworzyć admina
            if (!Regex.IsMatch(login, "^[a-zA-Z0-9]{3,20}$"))
                throw new ArgumentException("Login musi być alfanumeryczny i mieć 3–20 znaków.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ArgumentException("Hasło musi mieć co najmniej 6 znaków.");
            if (_userRepo.GetByLogin(login) != null)
                throw new InvalidOperationException("Użytkownik o takim loginie już istnieje.");

            var user = new User
            {
                Login              = login,
                Password           = HashPassword(password),
                CreatedAt          = DateTime.Now,
                IsAdmin            = isAdmin,
                SecurityQuestionId = 0,
                SecurityAnswer     = string.Empty
            };
            _userRepo.Add(user);
        }

        /// <summary>
        /// Reset hasła po poprawnej odpowiedzi na pytanie.
        /// Jeśli konto admina (securityQuestionId == 0), prosi o kontakt z adminem.
        /// </summary>
        public void ResetPassword(string login, int questionId, string answer, string newPassword)
        {
            var user = _userRepo.GetByLogin(login)
                       ?? throw new InvalidOperationException("Użytkownik nie znaleziony.");

            if (user.SecurityQuestionId == 0)
                throw new InvalidOperationException("Skontaktuj się z administratorem systemu.");

            if (user.SecurityQuestionId != questionId || user.SecurityAnswer != answer)
                throw new InvalidOperationException("Odpowiedź na pytanie pomocnicze jest nieprawidłowa.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ArgumentException("Hasło musi mieć co najmniej 6 znaków.");

            user.Password = HashPassword(newPassword);
            _userRepo.Update(user);
        }

        public IEnumerable<SecurityQuestion> GetAllSecurityQuestions()
        {
            return _questionRepo.GetAll();
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
