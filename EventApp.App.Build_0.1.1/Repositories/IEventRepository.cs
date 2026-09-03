using EventApp.Web.Models;

namespace EventApp.Web.Repositories
{
    public interface IEventRepository
    {
       // Deze method zegt:
       // Geef me alle events als een lijst met EventCard objecten.

        // Waarom 'Task<>' ?
        // Omdat database calls het liefst async zijn zodat de app niet blokkeert als er gewacht wordt op data.

        // Waarom 'List<EventCard>' ?
        // Omdat de homepage meerdere cards nodig heeft, niet alleen een.
        Task<List<EventCard>> GetAllEventsAsync();

        // Geeft specifieke events terug op basis van een lijst van event ID's.
        // Gebruikt door de bookmarks API om alleen de opgeslagen events op te halen.
        Task<List<EventCard>> GetEventsByIdsAsync(IEnumerable<int> ids);
    }
}