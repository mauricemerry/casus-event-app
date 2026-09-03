using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models
{
    public class CreateTicketInputModel
    {
        [Required(ErrorMessage = "Ticketsoort is verplicht.")]
        public string TicketType { get; set; } = string.Empty;

        [Range(0, 999999)]
        public decimal Price { get; set; }
    }
}