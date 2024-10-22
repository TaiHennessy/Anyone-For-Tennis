namespace AnyoneForTennis.Models
{
    public class UserCoach
    {
        public int UserId { get; set; } // Foreign key from User
        public int CoachId { get; set; } // Foreign key from Coach

        // Navigation properties
        public User User { get; set; }
        public Coach Coach { get; set; }
    }

}
