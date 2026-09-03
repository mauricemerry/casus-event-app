using EventApp.OrganiserPortal.Models;

namespace EventApp.OrganiserPortal.Repositories
{
    public interface IOrganiserRepository
    {
        Task<OrganiserModel?> GetByEmailAsync(string email);
        Task<List<OrganiserEventListItemModel>> GetEventsByOrganiserIdAsync(int organiserId);

        Task<int> GetNextEventIdAsync();
        Task CreateEventAsync(int organiserId, int eventId, CreateEventInputModel input);
        Task AddEventImagesAsync(int eventId, List<string> imageUrls);
        Task<CreateEventInputModel?> GetEventForEditAsync(int organiserId, int eventId);
        Task UpdateEventAsync(int organiserId, int eventId, CreateEventInputModel input);
        Task CancelEventAsync(int organiserId, int eventId);
    }
}