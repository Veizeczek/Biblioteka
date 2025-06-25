using System;

namespace Biblioteka.Data.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CopyId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}