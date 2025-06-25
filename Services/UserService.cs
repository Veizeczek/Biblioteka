using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly CopyRepository _copyRepo = new CopyRepository();
        private readonly BookRepository _bookRepo = new BookRepository();

        // Retrieve all users for admin management
        public IEnumerable<User> GetAllUsers()
        {
            return _userRepo.GetAll();
        }

        // Update existing user details (login, security question/answer)
        public void UpdateUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Login) || user.Login.Length < 3)
                throw new ArgumentException("Login must be at least 3 characters long.");
            _userRepo.Update(user);
        }

        // Delete a user by ID
        public void DeleteUser(int id)
        {
            // Prevent deletion of admin user
            var user = _userRepo.GetById(id);
            if (user == null)
                throw new ArgumentException("User not found.");
            if (user.IsAdmin)
                throw new InvalidOperationException("Cannot delete an admin user.");
            _userRepo.Delete(id);
        }
        public void PromoteUser(string login)
        {
            var user = _userRepo.GetByLogin(login) ?? throw new ArgumentException("User not found.");
            user.IsAdmin = true;
            _userRepo.Update(user);
        }

        public void DemoteUser(string login)
        {
            var user = _userRepo.GetByLogin(login) ?? throw new ArgumentException("User not found.");
            user.IsAdmin = false;
            _userRepo.Update(user);
        }

        // Retrieve current active loans for a user
        public IEnumerable<Loan> GetActiveLoans(int userId)
        {
            foreach (var loan in _loanRepo.GetLoansByUser(userId))
            {
                if (!loan.ReturnDate.HasValue)
                    yield return loan;
            }
        }

        // Retrieve full loan history for a user, enriched with book title
        public IEnumerable<(Loan Loan, string BookTitle)> GetLoanHistory(int userId)
        {
            foreach (var loan in _loanRepo.GetLoansByUser(userId))
            {
                var copy = _copyRepo.GetById(loan.CopyId);
                var book = _bookRepo.GetById(copy.BookId);
                yield return (loan, book.Title);
            }
        }
    }
}