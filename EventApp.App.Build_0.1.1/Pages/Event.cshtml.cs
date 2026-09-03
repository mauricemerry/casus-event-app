using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.Web.Models;
using EventApp.Web.Repositories;
using EventApp.Web.Repositories.Events;

namespace EventApp.Web.Pages
{
    public class EventModel : PageModel
    {
        private readonly IEventRepository _eventRepository;

        public EventModel(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public EventCard? EventCard { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var events = await _eventRepository.GetAllEventsAsync();
            EventCard = events.FirstOrDefault(equals => equals.EventId == id);

            if (EventCard == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}