namespace EventApp.OrganiserPortal.Models
{
    public class EventLocationInputModel
    {
        public string? Name { get; set; }

        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Addition { get; set; }
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
