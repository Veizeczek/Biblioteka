using System;
using System.Collections.Generic;
using System.Linq;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Services
{
    public class AdminHistoryService
    {
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly CopyRepository _copyRepo = new CopyRepository();
        private readonly BookRepository _bookRepo = new BookRepository();

        // Pobiera wszystkie egzemplarze (tylko tytuły)
        public IEnumerable<HistoryCopyView> GetAllCopies()
        {
            foreach (var copy in _copyRepo.GetAll())
            {
                var book = _bookRepo.GetById(copy.BookId);
                yield return new HistoryCopyView
                {
                    CopyId      = copy.Id,
                    DisplayName = book.Title
                };
            }
        }

        // Stronicowanie egzemplarzy według ID
        public IEnumerable<HistoryCopyView> GetCopiesPage(int skip, int take)
        {
            foreach (var copy in _copyRepo.GetPage(skip, take))
            {
                var book = _bookRepo.GetById(copy.BookId);
                yield return new HistoryCopyView
                {
                    CopyId      = copy.Id,
                    DisplayName = book.Title
                };
            }
        }

        // Historia wypożyczeń dla konkretnego egzemplarza, malejąco po dacie wypożyczenia
        public IEnumerable<HistoryLoanAdminView> GetLoanHistoryForCopy(int copyId)
        {
            var copy = _copyRepo.GetById(copyId);
            var book = _bookRepo.GetById(copy.BookId);
            var loans = _loanRepo.GetLoansByCopy(copyId)
                                .OrderByDescending(l => l.LoanDate);
            foreach (var loan in loans)
            {
                var user = _userRepo.GetById(loan.UserId);
                yield return new HistoryLoanAdminView
                {
                    LoanId     = loan.Id,
                    UserLogin  = user.Login,
                    BookTitle  = book.Title,
                    LoanDate   = loan.LoanDate,
                    ReturnDate = loan.ReturnDate
                };
            }
        }

        // Pobiera użytkowników (tylko loginy)
        public IEnumerable<HistoryUserView> GetAllUsers()
        {
            foreach (var user in _userRepo.GetAll())
            {
                yield return new HistoryUserView
                {
                    UserId      = user.Id,
                    DisplayName = user.Login
                };
            }
        }

        // Stronicowanie użytkowników według ID
        public IEnumerable<HistoryUserView> GetUsersPage(int skip, int take)
        {
            foreach (var user in _userRepo.GetPage(skip, take))
            {
                yield return new HistoryUserView
                {
                    UserId      = user.Id,
                    DisplayName = user.Login
                };
            }
        }

        // Historia wypożyczeń dla konkretnego użytkownika, malejąco po dacie wypożyczenia
        public IEnumerable<HistoryLoanAdminView> GetLoanHistoryForUser(int userId)
        {
            var user = _userRepo.GetById(userId);
            var loans = _loanRepo.GetLoansByUser(userId)
                                .OrderByDescending(l => l.LoanDate);
            foreach (var loan in loans)
            {
                var copy = _copyRepo.GetById(loan.CopyId);
                var book = _bookRepo.GetById(copy.BookId);
                yield return new HistoryLoanAdminView
                {
                    LoanId     = loan.Id,
                    UserLogin  = user.Login,
                    BookTitle  = book.Title,
                    LoanDate   = loan.LoanDate,
                    ReturnDate = loan.ReturnDate
                };
            }
        }

        // Pobiera ostatnie operacje (wypożyczenia i zwroty) w porządku rosnącym skip/take
        public IEnumerable<AdminOperationView> GetRecentOperationsPage(int skip, int take)
        {
            foreach (var loan in _loanRepo.GetLoansPageDescending(skip, take))
            {
                var copy = _copyRepo.GetById(loan.CopyId);
                var book = _bookRepo.GetById(copy.BookId);
                var user = _userRepo.GetById(loan.UserId);

                yield return new AdminOperationView
                {
                    CopyId    = copy.Id,
                    BookTitle = book.Title,
                    UserLogin = user.Login,
                    Date      = loan.LoanDate,
                    Type      = loan.ReturnDate.HasValue ? "Zwrot" : "Wypożyczenie"
                };
            }
        }
    }

    public class HistoryCopyView
    {
        public int    CopyId      { get; set; }
        public string DisplayName { get; set; }
    }

    public class HistoryLoanAdminView
    {
        public int        LoanId     { get; set; }
        public string     UserLogin  { get; set; }
        public string     BookTitle  { get; set; }
        public DateTime   LoanDate   { get; set; }
        public DateTime?  ReturnDate { get; set; }
    }

    public class HistoryUserView
    {
        public int    UserId      { get; set; }
        public string DisplayName { get; set; }
    }

    public class AdminOperationView
    {
        public int      CopyId    { get; set; }
        public string   BookTitle { get; set; }
        public string   UserLogin { get; set; }
        public DateTime Date      { get; set; }
        public string   Type      { get; set; }
    }
}
