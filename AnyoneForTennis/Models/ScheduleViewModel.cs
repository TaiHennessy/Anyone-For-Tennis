namespace AnyoneForTennis.Models
{
    public class ScheduleViewModel
    {
        // Local Schedules
        public List<Schedule> LocalSchedule { get; set; }
        // Main Schedules
        public List<Schedule> MainSchedule {  get; set; } 
        // Memberships
        // Members
        public List<Member> Member { get; set; }
        // Coaches
        public List<Coach> Coach { get; set; }

    }
}
