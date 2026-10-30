using EventApp.OrganiserPortal.Models;

namespace EventApp.OrganiserPortal.Repositories
{
    public interface IOrganiserRepository
    {
        Task<OrganiserModel?> GetByEmailAsync(string email);
        Task<List<DashboardEventModel>> GetEventsByOrganiserIdAsync(int organiserId);

        Task<int> GetNextEventIdAsync();
        Task CreateEventAsync(int organiserId, int eventId, EventInputModel input);
        Task AddEventImagesAsync(int eventId, List<string> imageUrls);
        Task RemoveEventImagesAsync(int eventId, List<string> keptImageUrls);
        Task<EventInputModel?> GetEventForEditAsync(int organiserId, int eventId);
        Task UpdateEventAsync(int organiserId, int eventId, EventInputModel input);
        Task CancelEventAsync(int organiserId, int eventId);
    }
}