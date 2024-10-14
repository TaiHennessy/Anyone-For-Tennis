namespace AnyoneForTennis.Models
{
    public class SchedulesViewModel
    {
        public Schedule Schedule { get; set; }
        public SchedulePlus SchedulePlus { get; set; }
        public List<Coach> Coaches { get; set; }

        public SchedulesViewModel()
        {
            Schedule = new Schedule();
            SchedulePlus = new SchedulePlus();
            Coaches = new List<Coach>();
        }
    }
}
