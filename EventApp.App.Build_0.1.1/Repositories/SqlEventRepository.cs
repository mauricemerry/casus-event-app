using Microsoft.Data.SqlClient;
using EventApp.Web.Models;
using EventApp.Web.Repositories.Events;

namespace EventApp.Web.Repositories
{
    public class SqlEventRepository : IEventRepository
    {
        private readonly string _connectionString;

        public SqlEventRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<EventCard>> GetAllEventsAsync()
        {
            var events = new List<EventCard>();
            const string sql = EventQueries.GetAllEvents;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var eventName = reader["event_name"]?.ToString() ?? string.Empty;
                var categories = reader["categories"]?.ToString()?.Split(", ").ToList() ?? [];

                var ratingValue = GetRatingValue(eventName);
                var eventCard = new EventCard
                {
                    EventId = reader.GetInt32(reader.GetOrdinal("event_id")),
                    EventName = eventName,
                    Description = reader["description"] == DBNull.Value
                        ? string.Empty
                        : reader["description"]?.ToString() ?? string.Empty,
                    ImageUrl = reader["image_url"] == DBNull.Value
                        ? "/uploads/events/placeholder.jpg"
                        : reader["image_url"]?.ToString() ?? "/uploads/events/placeholder.jpg",
                    MinPrice = reader.GetDecimal(reader.GetOrdinal("min_price")),
                    RatingValue = ratingValue,
                    RatingText = GetRatingText(ratingValue),
                    IsAdultsOnly = GetIsAdultsOnly(eventName, categories),
                    Categories = categories,
                    AccessibilityText = reader["accessibility_text"] == DBNull.Value
                        ? string.Empty
                        : reader["accessibility_text"]?.ToString() ?? string.Empty
                };

                events.Add(eventCard);
            }

            return events;
        }

        // Haalt specifieke events op aan de hand van een lijst van event ID's.
        // Wordt aangeroepen door de bookmarks API endpoint in Program.cs.
        public async Task<List<EventCard>> GetEventsByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();

            // Als er geen ID's zijn, hoeven we niks op te halen
            if (idList.Count == 0)
                return [];

            // Bouw geparametriseerde IN-clause: @id0, @id1, @id2, ...
            // Dit voorkomt SQL injection: de waarden worden nooit direct in de query gezet
            var paramNames = idList.Select((_, i) => $"@id{i}").ToList();
            var inClause = string.Join(", ", paramNames);
            var sql = EventQueries.GetEventsByIds(inClause);

            var events = new List<EventCard>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);

            // Voeg elke ID toe als aparte parameter
            for (var i = 0; i < idList.Count; i++)
                command.Parameters.AddWithValue($"@id{i}", idList[i]);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var eventName = reader["event_name"]?.ToString() ?? string.Empty;
                var categories = reader["categories"]?.ToString()?.Split(", ").ToList() ?? [];

                var ratingValue = GetRatingValue(eventName);
                var eventCard = new EventCard
                {
                    EventId = reader.GetInt32(reader.GetOrdinal("event_id")),
                    EventName = eventName,
                    Description = reader["description"] == DBNull.Value
                        ? string.Empty
                        : reader["description"]?.ToString() ?? string.Empty,
                    ImageUrl = reader["image_url"] == DBNull.Value
                        ? "/uploads/events/placeholder.jpg.svg"
                        : reader["image_url"]?.ToString() ?? "/uploads/events/placeholder.jpg.svg",
                    MinPrice = reader.GetDecimal(reader.GetOrdinal("min_price")),
                    RatingValue = ratingValue,
                    RatingText = GetRatingText(ratingValue),
                    IsAdultsOnly = GetIsAdultsOnly(eventName, categories),
                    Categories = categories,
                    AccessibilityText = reader["accessibility_text"] == DBNull.Value
                        ? string.Empty
                        : reader["accessibility_text"]?.ToString() ?? string.Empty
                };

                events.Add(eventCard);
            }

            return events;
        }

        private static int GetRatingValue(string eventName)
        {
            if (eventName.Contains("Pinkpop", StringComparison.OrdinalIgnoreCase))
            {
                return 5;
            }

            if (eventName.Contains("Cultura Nova", StringComparison.OrdinalIgnoreCase) ||
                eventName.Contains("Liquicity", StringComparison.OrdinalIgnoreCase) ||
                eventName.Contains("Amsterdam Dance Event", StringComparison.OrdinalIgnoreCase))
            {
                return 4;
            }

            return 3;
        }

        private static string GetRatingText(int ratingValue)
        {
            return ratingValue switch
            {
                5 => "Uitstekend",
                4 => "Heel goed",
                3 => "Goed",
                2 => "Matig",
                _ => "Onbekend"
            };
        }

        private static bool GetIsAdultsOnly(string eventName, List<string> categories)
        {
            var source = $"{eventName} {categories}";

            return categories.Contains("House")
                || categories.Contains("Techno")
                || categories.Contains("EDM")
                || categories.Contains("Trance")
                || categories.Contains("Drum & Bass")
                || categories.Contains("Hardstyle")
                || eventName.Contains("Liquicity", StringComparison.OrdinalIgnoreCase)
                || eventName.Contains("Amsterdam Dance Event", StringComparison.OrdinalIgnoreCase);
        }
    }
}