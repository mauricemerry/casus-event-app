using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventApp.Web.Pages
{
    // De bookmarks pagina heeft geen server-side logica nodig:
    // de JavaScript op de pagina leest de opgeslagen event ID's uit localStorage
    // en haalt de bijbehorende events zelf op via de API endpoint.
    public class BookmarksModel : PageModel
    {
        public void OnGet()
        {
            // Pagina wordt gewoon geladen; de JS doet de rest.
        }
    }
}
