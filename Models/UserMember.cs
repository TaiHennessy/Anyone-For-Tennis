namespace AnyoneForTennis.Models
{
    public class UserMember
    {
        public int UserId { get; set; } // Foreign key from User
        public int MemberId { get; set; } // Foreign key from Member

        // Navigation properties
        public User User { get; set; }
        public Member Member { get; set; }
    }

}
