namespace Biblioteka.Data.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int? Year { get; set; }
        public int? Pages { get; set; }
        public string Genre { get; set; }
        public string Language { get; set; }
    }
}