namespace Biblioteka.Data.Entities
{
    public class Copy
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Status { get; set; } // 0 = available, 1 = loaned, 2 = maintenance
        public string Notes { get; set; }
    }
}