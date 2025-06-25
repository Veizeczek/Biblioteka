namespace Biblioteka.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }
        public int SecurityQuestionId { get; set; }
        public string SecurityAnswer { get; set; }
    }
}