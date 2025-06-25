// Biblioteka.Data.Entities/UserDetails.cs
namespace Biblioteka.Data.Entities
{
    public class UserDetails
    {
        // user_id jest PK i FK do users.id
        public int    UserId    { get; set; }
        public string FirstName { get; set; }    // NOT NULL
        public string LastName  { get; set; }    // NOT NULL
        public DateTime DateOfBirth  { get; set; } 
        public string Phone     { get; set; }    // opcjonalne
        public string Email     { get; set; }    // opcjonalne
    }
}