
using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models;

public class CreateTicketInputModel
{
    // Naam, bijvoorbeeld Volwassenen, Kinderen of Gratis.
    [Required, StringLength(100)]
    public string TicketType { get; set; } = "";

    [Range(0, 999999)]
    public decimal Price { get; set; }

    // Gratis / Dagticket / Periode / Tijdslot
    public string DateType { get; set; } = "Gratis";

    // Voor dagticket en tijdslot
    public DateTime? TicketDate { get; set; }

    // Alleen voor tijdslot
    public TimeSpan? SlotStart { get; set; }
    public TimeSpan? SlotEnd { get; set; }

    // Meerdere losse dagen bij Periode
    public List<DateTime?> PeriodDates { get; set; } = new();
}
