namespace EventApp.OrganiserPortal.Models
{
    public class EventPeriodInputModel
    {
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }

        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
