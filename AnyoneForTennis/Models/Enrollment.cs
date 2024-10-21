namespace AnyoneForTennis.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; } // Primary Key
        public int MemberId { get; set; } // Foreign Key for Member
        public int CoachId { get; set; } // Foreign Key for Coach
        public int ScheduleId { get; set; } // Foreign Key for Schedule

        public Member Member { get; set; }
        public Coach Coach { get; set; }
        public Schedule Schedule { get; set; }
    }
}
