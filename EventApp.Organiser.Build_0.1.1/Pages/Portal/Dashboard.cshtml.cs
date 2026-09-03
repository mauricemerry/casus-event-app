using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;

namespace EventApp.OrganiserPortal.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly IOrganiserRepository _organiserRepository;

        public DashboardModel(IOrganiserRepository organiserRepository)
        {
            _organiserRepository = organiserRepository;
        }

        public List<OrganiserEventListItemModel> Events { get; set; } = [];

        public async Task OnGetAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                Events = [];
                return;
            }

            Events = await _organiserRepository.GetEventsByOrganiserIdAsync(organiserId);
        }
    }
}