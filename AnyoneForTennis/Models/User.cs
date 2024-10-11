using System.ComponentModel.DataAnnotations;

namespace AnyoneForTennis.Models
{
    public class User
    {
        public int UserId { get; set; } // Primary key
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        // Relationships
        public ICollection<UserMember> UserMembers { get; set; }
        public ICollection<UserCoach> UserCoaches { get; set; }
    }

}
