
using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models;

public class CreateEventInputModel
{
    // Basis
    [Required, StringLength(255)]
    public string EventName { get; set; } = "";

    [StringLength(50)]
    public string? EventType { get; set; }

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    // Bestaande Edit-pagina blijft dit veld gebruiken.
    [Required]
    public string IndoorsOutdoors { get; set; } = "";

    // Meerdere omgevingen tegelijk mogelijk.
    public bool IsIndoor { get; set; }
    public bool IsOutdoor { get; set; }
    public bool IsOnline { get; set; }

    // Details
    [Required]
    public string Description { get; set; } = "";

    public List<ImageCropPositionInputModel> ImageCropPositions
    { get; set; } = new();

    // Periodes
    public List<EventPeriodInputModel> Periods
    { get; set; } = new();

    // Globaal bereik, voor bestaande databasecode.
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    // Extra informatie
    public string? Notes { get; set; }

    public List<string> AccessibilityOptions
    { get; set; } = new();

    public bool HasOtherAccessibility { get; set; }

    public string? OtherAccessibilityDescription
    { get; set; }

    public string? SelectedCategory { get; set; }

    public List<string> Categories { get; set; } = new();

    public List<string> Tags { get; set; } = new();

    // Locaties
    public List<EventLocationInputModel> Locations
    { get; set; } = new();

    // Oude velden behouden voor Edit.cshtml.
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Bestaande voorzieningen.
    public bool? HasStandingPlaces { get; set; }
    public bool? HasSittingPlaces { get; set; }
    public bool? DisabledParkingAvailable { get; set; }
    public bool? DisabledToiletAvailable { get; set; }
    public bool? WheelchairAccessibleToilet { get; set; }

    // Tickets
    public List<CreateTicketInputModel> Tickets
    { get; set; } = new();
}

public class EventPeriodInputModel
{
    public string? Name { get; set; }

    public DateTime? StartDate { get; set; }
    public TimeSpan? StartTime { get; set; }

    public DateTime? EndDate { get; set; }
    public TimeSpan? EndTime { get; set; }
}

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
