using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AnyoneForTennis.Models
{
    public class User : IdentityUser<int>
    {
        // Additional properties
        public bool IsAdmin { get; set; }

        // Relationships
        public virtual ICollection<UserMember> UserMembers { get; set; } = new List<UserMember>();
        public virtual ICollection<UserCoach> UserCoaches { get; set; } = new List<UserCoach>();
    }
}
