
using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models;

public class TicketInputModel
{
    // Naam, bijvoorbeeld Volwassenen, Kinderen of Gratis.
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Range(0, 999999)]
    public decimal? Price { get; set; }

    // Gratis / Dagticket / Periode / Tijdslot
    public string TypeName { get; set; } = TicketType.FREE.Name();
    public TicketType? Type { get; set; } = TicketType.FREE;

    // Voor dagticket en tijdslot
    public DateTime? Date { get; set; }

    // Alleen voor tijdslot
    public TimeSpan? SlotStart { get; set; }
    public TimeSpan? SlotEnd { get; set; }

    // Meerdere losse dagen bij Periode
    public List<DateTime?> PeriodDates { get; set; } = new();
}
