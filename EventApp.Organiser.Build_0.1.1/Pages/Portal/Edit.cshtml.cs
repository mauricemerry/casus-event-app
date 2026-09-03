using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;

namespace EventApp.OrganiserPortal.Pages.Portal
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IOrganiserRepository _organiserRepository;

        public EditModel(IOrganiserRepository organiserRepository)
        {
            _organiserRepository = organiserRepository;
        }

        [BindProperty]
        public CreateEventInputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            var eventData = await _organiserRepository.GetEventForEditAsync(organiserId, Id);
            if (eventData == null)
            {
                return NotFound();
            }

            Input = eventData;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            if (Input.Tickets == null)
            {
                Input.Tickets = new();
            }

            Input.Tickets = Input.Tickets
                .Where(x => !string.IsNullOrWhiteSpace(x.TicketType))
                .ToList();

            if (!Input.Tickets.Any())
            {
                Input.Tickets.Add(new CreateTicketInputModel
                {
                    TicketType = "Gratis",
                    Price = 0
                });
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _organiserRepository.UpdateEventAsync(organiserId, Id, Input);
            return RedirectToPage("/Portal/Dashboard");
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            await _organiserRepository.CancelEventAsync(organiserId, Id);
            return RedirectToPage("/Portal/Dashboard");
        }
    }
}