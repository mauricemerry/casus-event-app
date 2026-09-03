using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models
{
    public class CreateEventInputModel
    {
        [Required(ErrorMessage = "Naam van het evenement is verplicht.")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kies Binnen, Buiten of Beide.")]
        [RegularExpression("Binnen|Buiten|Beide", ErrorMessage = "Kies Binnen, Buiten of Beide.")]
        public string IndoorsOutdoors { get; set; } = string.Empty;

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres is verplicht.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kies een locatie op de kaart.")]
        public decimal? Latitude { get; set; }

        [Required(ErrorMessage = "Kies een locatie op de kaart.")]
        public decimal? Longitude { get; set; }

        [Required(ErrorMessage = "Geef aan of er staanplaatsen zijn.")]
        public bool? HasStandingPlaces { get; set; }

        [Required(ErrorMessage = "Geef aan of er zitplaatsen zijn.")]
        public bool? HasSittingPlaces { get; set; }

        [Required(ErrorMessage = "Geef aan of er invalidenparkeerplaatsen zijn.")]
        public bool? DisabledParkingAvailable { get; set; }

        [Required(ErrorMessage = "Geef aan of er een invalidentoilet is.")]
        public bool? DisabledToiletAvailable { get; set; }

        [Required(ErrorMessage = "Geef aan of het toilet rolstoeltoegankelijk is.")]
        public bool? WheelchairAccessibleToilet { get; set; }

        public List<CreateTicketInputModel> Tickets { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> AccessibilityOptions { get; set; } = new();
        public List<ImageCropPositionInputModel> ImageCropPositions { get; set; } = new();
    }
}