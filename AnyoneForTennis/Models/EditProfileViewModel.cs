namespace AnyoneForTennis.Models
{
    public class EditProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // For password change
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
