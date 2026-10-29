namespace EventApp.OrganiserPortal.Models
{
    public class EventCategory
    {
        private static readonly Dictionary<string, EventCategory> BY_NAME = [];

        public static readonly EventCategory MUSIC = new("Muziek");
        public static readonly EventCategory FESTIVAL = new("Festival");
        public static readonly EventCategory THEATER = new("Theater");
        public static readonly EventCategory SPORTS = new("Sport");
        public static readonly EventCategory WORKSHOP = new("Workshop");
        public static readonly EventCategory MISCELLANEOUS = new("Overig");

        private readonly string name;

        private EventCategory(string name)
        {
            this.name = name;
            BY_NAME.Add(name, this);
        }

        public string Name()
        {
            return this.name;
        }

        public static EventCategory? ByName(string name)
        {
            return BY_NAME.GetValueOrDefault(name);
        }
    }
}
