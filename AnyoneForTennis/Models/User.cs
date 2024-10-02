namespace AnyoneForTennis.Models
{
    public class User
    {
        public int UserId { get; set; } // Primary key
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        // Relationships
        public ICollection<UserMember> UserMembers { get; set; }
        public ICollection<UserCoach> UserCoaches { get; set; }
    }

}
