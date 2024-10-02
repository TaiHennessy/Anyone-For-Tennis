namespace AnyoneForTennis.Models
{
    public class SchedulePlus
    {
        public int SchedulePlusId { get; set; } // Primary key
        public int ScheduleId { get; set; } // Foreign key from Schedule
        public DateTime DateTime { get; set; }
        public int Duration { get; set; } // Additional field example

        // Navigation properties
        public Schedule Schedule { get; set; }
    }

}
