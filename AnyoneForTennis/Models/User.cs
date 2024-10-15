using Microsoft.AspNetCore.Identity;
namespace AnyoneForTennis.Models
{
    public class User : IdentityUser<int>
    {

        public bool IsAdmin { get; set; }

        // Relationships
        public ICollection<UserMember> UserMembers { get; set; }
        public ICollection<UserCoach> UserCoaches { get; set; }
    }

}
