namespace EventApp.OrganiserPortal.Models
{
    public class TicketType
    {
        private static readonly Dictionary<string, TicketType> BY_NAME = [];

        public static readonly TicketType FREE = new("Gratis");
        public static readonly TicketType DAY_TICKET = new("Dagticket");
        public static readonly TicketType PERIOD = new("Periode");
        public static readonly TicketType TIME_SLOT = new("Tijdslot");

        private readonly string name;

        private TicketType(string name)
        {
            this.name = name;
            BY_NAME.Add(name, this);
        }

        public string Name()
        {
            return this.name;
        }

        public static TicketType? ByName(string name)
        {
            return BY_NAME.GetValueOrDefault(name);
        }
    }
}
