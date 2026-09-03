using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.Web.Models;
using EventApp.Web.Repositories;

namespace EventApp.Web.Pages
{

    // Deze Class bevat de logica voor de homepagina
    // Razor Pages deelt de pagina in:
    // - .cshtml voor de html zelf
    // - .cshtml.cs voor de C# logica
    public class IndexModel : PageModel
    {
        // Deze variabele slaat de repository op zodat we die later kunnen gebruiken
        private readonly IEventRepository _eventRepository;

        // Deze eigenschap bewaart alle event cards voor de pagina
        // De pagina kan dit lezen en weergeven
        public List<EventCard> Events { get; set; } = new();

        // De constructor runt wanneer de pagina model is aangemaakt
        // ASP.NET injecteert IEventRepository automatisch als Program.cs correct is ingesteld
        public IndexModel(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        // OnGetAssync runt wanneer iemand de homepagina opent met een normale GET request
        // Dit is waar we de events laden van de database
        public async Task OnGetAsync()
        {
            Events = await _eventRepository.GetAllEventsAsync();
        }
    }
}