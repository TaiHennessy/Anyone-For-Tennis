namespace AnyoneForTennis.Models
{
    public class HomePageViewModel
    {
        public List<UserCoach> UserCoaches { get; set; } // List of UserCoaches for the logged-in user
        public List<UserMember> UserMembers { get; set; } // List of UserMembers for the logged-in user
        public List<Coach> Coaches { get; set; } // List of coaches to display on the homepage
        public List<Schedule> Schedules { get; set; } // List of schedules to display on the homepage
        public List<SchedulePlus> SchedulePluses { get; set; } // List of SchedulePlus to display schedules assigned to coaches
        public List<Enrollment> Enrollments { get; set; }  // Enrollments for members
        public List<Member> RegisteredMembers { get; set; }  // Members registered for each schedule for the coach to see

    }
}
