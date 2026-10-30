
using System.Data;
using Microsoft.Data.SqlClient;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Queries;

namespace EventApp.OrganiserPortal.Repositories
{
    public class SqlOrganiserRepository : IOrganiserRepository
    {
        private readonly string _connectionString;

        public SqlOrganiserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("db-eventapp")
                ?? throw new InvalidOperationException(
                    "Connection string 'db-eventapp' not found or unreachable.");
        }

        public async Task RemoveEventImagesAsync(int eventId, List<string> keptImageUrls)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(OrganiserQueries.RemoveEventImages, connection);
            command.Parameters.AddWithValue("@EventId", eventId);
            command.Parameters.AddWithValue("@KeptImageUrls", string.Join('|', keptImageUrls ?? []));
            await command.ExecuteNonQueryAsync();
        }

        public async Task<OrganiserModel?> GetByEmailAsync(string email)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                OrganiserQueries.GetOrganiserByEmail, connection);

            command.Parameters.AddWithValue("@Email", email);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new OrganiserModel
            {
                OrganiserId = reader.GetInt32(reader.GetOrdinal("organiser_id")),
                CompanyName = reader.GetString(reader.GetOrdinal("company_name")),
                ContactName = reader.IsDBNull(reader.GetOrdinal("contact_name"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("contact_name")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            };
        }

        public async Task<List<OrganiserEventListItemModel>>
            GetEventsByOrganiserIdAsync(int organiserId)
        {
            var results = new List<OrganiserEventListItemModel>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                OrganiserQueries.GetEventsByOrganiserId, connection);

            command.Parameters.AddWithValue("@OrganiserId", organiserId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new OrganiserEventListItemModel
                {
                    EventId = reader.GetInt32(reader.GetOrdinal("event_id")),
                    EventName = reader.GetString(reader.GetOrdinal("event_name")),

                    Environment = reader.GetInt32(reader.GetOrdinal("indoors_outdoors")),

                    Description = reader.IsDBNull(reader.GetOrdinal("description"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("description")),

                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("image_url")),

                    MinPrice = reader.IsDBNull(reader.GetOrdinal("min_price"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("min_price")),

                    CategoriesText = reader.IsDBNull(reader.GetOrdinal("categories_text"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("categories_text")),

                    AccessibilityText = reader.IsDBNull(reader.GetOrdinal("accessibility_text"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("accessibility_text")),

                    IsCanceled = !reader.IsDBNull(reader.GetOrdinal("canceled"))
                        && reader.GetBoolean(reader.GetOrdinal("canceled"))
                });
            }

            return results;
        }

        public async Task<int> GetNextEventIdAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                OrganiserQueries.GetNextEventId, connection);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task CreateEventAsync(
            int organiserId,
            int eventId,
            CreateEventInputModel input)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction =
                (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                var locationId = await GetNextIdAsync(
                    connection,
                    transaction,
                    OrganiserQueries.GetNextLocationId);

                await InsertLocationAsync(
                    connection, transaction, locationId, input);

                await InsertEventAsync(
                    connection, transaction, eventId, locationId, organiserId, input);

                await InsertTicketsAsync(
                    connection, transaction, eventId, input);

                await InsertAccessibilityOptionsAsync(
                    connection, transaction, eventId, input);

                await InsertCategoriesAsync(
                    connection, transaction, eventId, input);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddEventImagesAsync(
            int eventId,
            List<string> imageUrls)
        {
            if (imageUrls == null || imageUrls.Count == 0)
            {
                return;
            }

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction =
                (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                var nextEventImageId = await GetNextIdAsync(
                    connection,
                    transaction,
                    OrganiserQueries.GetNextEventImageId);

                for (var i = 0; i < imageUrls.Count; i++)
                {
                    await using var command = new SqlCommand(
                        OrganiserQueries.InsertEventImage,
                        connection,
                        transaction);

                    command.Parameters.AddWithValue(
                        "@EventImageId", nextEventImageId++);

                    command.Parameters.AddWithValue("@EventId", eventId);
                    command.Parameters.AddWithValue("@ImageUrl", imageUrls[i]);
                    command.Parameters.AddWithValue("@IsPrimary", i == 0);
                    command.Parameters.AddWithValue("@SortOrder", i);

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CreateEventInputModel?> GetEventForEditAsync(
            int organiserId,
            int eventId)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                OrganiserQueries.GetEventForEdit,
                connection);

            command.Parameters.AddWithValue("@OrganiserId", organiserId);
            command.Parameters.AddWithValue("@EventId", eventId);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            int env = reader.GetInt32(reader.GetOrdinal("indoors_outdoors"));

            CreateEventInputModel eventData = new()
            {
                EventName = reader.GetString(reader.GetOrdinal("event_name")),

                EventType = GetNullableString(reader, "event_type"),

                ShortDescription = GetNullableString(
                    reader, "short_description"),

                StartDateTime = GetNullableDateTime(
                    reader, "start_datetime"),

                EndDateTime = GetNullableDateTime(
                    reader, "end_datetime"),

                Notes = GetNullableString(reader, "notes"),

                IsIndoor = EventEnvironment.IsIndoors(env),
                IsOutdoor = EventEnvironment.IsOutdoors(env),
                IsOnline = EventEnvironment.IsOnline(env),

                Description = GetNullableString(reader, "description"),

                Latitude = reader.IsDBNull(reader.GetOrdinal("latitude"))
                    ? null
                    : reader.GetDecimal(reader.GetOrdinal("latitude")),

                Longitude = reader.IsDBNull(reader.GetOrdinal("longitude"))
                    ? null
                    : reader.GetDecimal(reader.GetOrdinal("longitude")),

                HasStandingPlaces =
                    GetNullableBoolean(reader, "has_standing_places"),

                HasSittingPlaces =
                    GetNullableBoolean(reader, "has_sitting_places"),

                DisabledParkingAvailable =
                    GetNullableBoolean(reader, "disabled_parking_available"),

                DisabledToiletAvailable =
                    GetNullableBoolean(reader, "disabled_toilet_available"),

                WheelchairAccessibleToilet =
                    GetNullableBoolean(reader, "wheelchair_accessible_toilet")
            };

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    string imageUrl = GetNullableString(reader, "image_url");
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        eventData.ExistingImageUrls.Add(imageUrl);
                    }
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    EventLocationInputModel location = new()
                    {
                        Name = GetNullableString(reader, "location_name"),
                        City = GetNullableString(reader, "city"),
                        Street = GetNullableString(reader, "street"),
                        Number = GetNullableString(reader, "house_number"),
                        Addition = GetNullableString(reader, "house_number_addition"),
                        PostalCode = GetNullableString(reader, "postal_code"),
                        Latitude = GetNullableDecimal(reader, "latitude"),
                        Longitude = GetNullableDecimal(reader, "longitude")
                    };

                    eventData.Locations.Add(location);
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    string description = GetNullableString(reader, "description");
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        const string customPrefix = "Overige: ";
                        if (description.StartsWith(customPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            eventData.HasCustomAccessibilityOptions = true;
                            eventData.CustomAccessibilityOptionDescriptions = description[customPrefix.Length..];
                        }
                        else
                        {
                            eventData.AccessibilityOptionDescriptions.Add(description);
                        }
                    }
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    eventData.Periods.Add(new EventPeriodInputModel
                    {
                        Name = GetNullableString(reader, "period_name"),
                        StartDate = GetNullableDateTime(reader, "start_date"),
                        EndDate = GetNullableDateTime(reader, "end_date"),
                        StartTime = GetNullableTimeSpan(reader, "start_time"),
                        EndTime = GetNullableTimeSpan(reader, "end_time")
                    });
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    DateTime? start = GetNullableDateTime(reader, "start_datetime");
                    DateTime? end = GetNullableDateTime(reader, "end_datetime");

                    eventData.Tickets.Add(new CreateTicketInputModel
                    {
                        Name = GetNullableString(reader, "ticket_type"),
                        Price = GetNullableDecimal(reader, "price") ?? 0,
                        TypeName = GetNullableString(reader, "ticket_date_type"),
                        Date = start?.Date,
                        SlotStart = start?.TimeOfDay,
                        SlotEnd = end?.TimeOfDay
                    });
                }
            }

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                eventData.CategoryName = GetNullableString(reader, "category_name");
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    string tag = GetNullableString(reader, "tag_name");
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        eventData.Tags.Add(tag);
                    }
                }
            }

            EventLocationInputModel? firstLocation = eventData.Locations.FirstOrDefault();
            if (firstLocation != null)
            {
                eventData.Address = string.Join(", ", new[]
                {
                    firstLocation.Street,
                    firstLocation.Number,
                    firstLocation.Addition,
                    firstLocation.PostalCode,
                    firstLocation.City
                }.Where(value => !string.IsNullOrWhiteSpace(value)));
            }

            return eventData;
        }

        public async Task UpdateEventAsync(
            int organiserId,
            int eventId,
            CreateEventInputModel input)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction =
                (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                await using (var eventCommand = new SqlCommand(
                    OrganiserQueries.UpdateEvent,
                    connection,
                    transaction))
                {
                    eventCommand.Parameters.AddWithValue(
                        "@OrganiserId", organiserId);

                    eventCommand.Parameters.AddWithValue(
                        "@EventId", eventId);

                    eventCommand.Parameters.AddWithValue(
                        "@EventName", input.EventName);

                    eventCommand.Parameters.AddWithValue(
                        "@IndoorsOutdoors",
                        EventEnvironment.GetFlags(input.IsIndoor, input.IsOutdoor, input.IsOnline));

                    eventCommand.Parameters.AddWithValue(
                        "@Description",
                        string.IsNullOrWhiteSpace(input.Description)
                            ? DBNull.Value
                            : input.Description);

                    eventCommand.Parameters.AddWithValue(
                        "@HasStandingPlaces",
                        (object?)input.HasStandingPlaces ?? DBNull.Value);

                    eventCommand.Parameters.AddWithValue(
                        "@HasSittingPlaces",
                        (object?)input.HasSittingPlaces ?? DBNull.Value);

                    eventCommand.Parameters.AddWithValue(
                        "@DisabledParkingAvailable",
                        (object?)input.DisabledParkingAvailable ?? DBNull.Value);

                    eventCommand.Parameters.AddWithValue(
                        "@DisabledToiletAvailable",
                        (object?)input.DisabledToiletAvailable ?? DBNull.Value);

                    eventCommand.Parameters.AddWithValue(
                        "@WheelchairAccessibleToilet",
                        (object?)input.WheelchairAccessibleToilet ?? DBNull.Value);

                    AddEventDetailsParameters(eventCommand, input);

                    await eventCommand.ExecuteNonQueryAsync();
                }

                await using (var locationCommand = new SqlCommand(
                    OrganiserQueries.UpdateEventLocation,
                    connection,
                    transaction))
                {
                    locationCommand.Parameters.AddWithValue(
                        "@OrganiserId", organiserId);

                    locationCommand.Parameters.AddWithValue(
                        "@EventId", eventId);

                    var latitudeParameter = locationCommand.Parameters.Add(
                        "@Latitude", SqlDbType.Decimal);

                    latitudeParameter.Precision = 9;
                    latitudeParameter.Scale = 6;
                    latitudeParameter.Value =
                        Math.Round(input.Latitude!.Value, 6);

                    var longitudeParameter = locationCommand.Parameters.Add(
                        "@Longitude", SqlDbType.Decimal);

                    longitudeParameter.Precision = 9;
                    longitudeParameter.Scale = 6;
                    longitudeParameter.Value =
                        Math.Round(input.Longitude!.Value, 6);

                    await locationCommand.ExecuteNonQueryAsync();
                }

                EventLocationInputModel? location = input.Locations.FirstOrDefault();
                if (location != null)
                {
                    await using var locationsCommand = new SqlCommand(
                        OrganiserQueries.UpdateEventLocations,
                        connection,
                        transaction);

                    locationsCommand.Parameters.AddWithValue("@OrganiserId", organiserId);
                    locationsCommand.Parameters.AddWithValue("@EventId", eventId);
                    locationsCommand.Parameters.AddWithValue("@LocationName", (object?)location.Name ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@City", (object?)location.City ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@Street", (object?)location.Street ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@HouseNumber", (object?)location.Number ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@HouseNumberAddition", (object?)location.Addition ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@PostalCode", (object?)location.PostalCode ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@Latitude", (object?)location.Latitude ?? DBNull.Value);
                    locationsCommand.Parameters.AddWithValue("@Longitude", (object?)location.Longitude ?? DBNull.Value);
                    await locationsCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelEventAsync(int organiserId, int eventId)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                OrganiserQueries.CancelEvent,
                connection);

            command.Parameters.AddWithValue("@OrganiserId", organiserId);
            command.Parameters.AddWithValue("@EventId", eventId);

            await command.ExecuteNonQueryAsync();
        }

        private static async Task<int> GetNextIdAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            string sql)
        {
            await using var command = new SqlCommand(
                sql, connection, transaction);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static async Task InsertLocationAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int locationId,
            CreateEventInputModel input)
        {
            await using var command = new SqlCommand(
                OrganiserQueries.InsertLocation,
                connection,
                transaction);

            var latitude = Math.Round(input.Latitude!.Value, 6);
            var longitude = Math.Round(input.Longitude!.Value, 6);

            command.Parameters.AddWithValue("@LocationId", locationId);

            var latitudeParameter = command.Parameters.Add(
                "@Latitude", SqlDbType.Decimal);

            latitudeParameter.Precision = 9;
            latitudeParameter.Scale = 6;
            latitudeParameter.Value = latitude;

            var longitudeParameter = command.Parameters.Add(
                "@Longitude", SqlDbType.Decimal);

            longitudeParameter.Precision = 9;
            longitudeParameter.Scale = 6;
            longitudeParameter.Value = longitude;

            await command.ExecuteNonQueryAsync();
        }

        private static async Task InsertEventAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int eventId,
            int locationId,
            int organiserId,
            CreateEventInputModel input)
        {
            await using var command = new SqlCommand(
                OrganiserQueries.InsertEvent,
                connection,
                transaction);

            command.Parameters.AddWithValue("@EventId", eventId);
            command.Parameters.AddWithValue("@EventName", input.EventName);

            command.Parameters.AddWithValue(
                "@IndoorsOutdoors",
                EventEnvironment.GetFlags(input.IsIndoor, input.IsOutdoor, input.IsOnline));

            command.Parameters.AddWithValue(
                "@HasStandingPlaces",
                (object?)input.HasStandingPlaces ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@HasSittingPlaces",
                (object?)input.HasSittingPlaces ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@DisabledParkingAvailable",
                (object?)input.DisabledParkingAvailable ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@DisabledToiletAvailable",
                (object?)input.DisabledToiletAvailable ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@WheelchairAccessibleToilet",
                (object?)input.WheelchairAccessibleToilet ?? DBNull.Value);

            command.Parameters.AddWithValue("@LocationId", locationId);

            command.Parameters.AddWithValue(
                "@Description",
                string.IsNullOrWhiteSpace(input.Description)
                    ? DBNull.Value
                    : input.Description);

            command.Parameters.AddWithValue("@OrganiserId", organiserId);

            AddEventDetailsParameters(command, input);

            await command.ExecuteNonQueryAsync();
        }

        private static async Task InsertTicketsAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int eventId,
            CreateEventInputModel input)
        {
            if (input.Tickets == null || input.Tickets.Count == 0)
            {
                return;
            }

            var nextTicketPriceId = await GetNextIdAsync(
                connection,
                transaction,
                OrganiserQueries.GetNextTicketPriceId);

            foreach (var ticket in input.Tickets)
            {
                await using var command = new SqlCommand(
                    OrganiserQueries.InsertTicketPrice,
                    connection,
                    transaction);

                command.Parameters.AddWithValue(
                    "@EventTicketPriceId", nextTicketPriceId++);

                command.Parameters.AddWithValue(
                    "@TicketType", ticket.Name);

                var priceParameter = command.Parameters.Add(
                    "@Price", SqlDbType.Decimal);

                priceParameter.Precision = 10;
                priceParameter.Scale = 2;
                priceParameter.Value = Math.Round(ticket.Price, 2);

                command.Parameters.AddWithValue("@EventId", eventId);

                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertAccessibilityOptionsAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int eventId,
            CreateEventInputModel input)
        {
            if (input.AccessibilityOptionDescriptions == null ||
                input.AccessibilityOptionDescriptions.Count == 0)
            {
                return;
            }

            var nextAccessibilityId = await GetNextIdAsync(
                connection,
                transaction,
                OrganiserQueries.GetNextAccessibilityOptionId);

            foreach (var option in input.AccessibilityOptionDescriptions
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await using var command = new SqlCommand(
                    OrganiserQueries.InsertAccessibilityOption,
                    connection,
                    transaction);

                command.Parameters.AddWithValue(
                    "@AccessibilityOptionId", nextAccessibilityId++);

                command.Parameters.AddWithValue(
                    "@Description", option.Trim());

                command.Parameters.AddWithValue("@EventId", eventId);

                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertCategoriesAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int eventId,
            CreateEventInputModel input)
        {
            if (input.Category == null)
            {
                return;
            }

            int categoryId;

            await using (var findCommand = new SqlCommand(
                OrganiserQueries.FindCategoryByName,
                connection,
                transaction))
            {
                findCommand.Parameters.AddWithValue(
                    "@CategoryName", input.Category.Name());

                var existingId = await findCommand.ExecuteScalarAsync();

                if (existingId != null)
                {
                    categoryId = Convert.ToInt32(existingId);
                }
                else
                {
                    return;
                }
            }

            await using var linkCommand = new SqlCommand(
                   OrganiserQueries.LinkEventCategory,
                   connection,
                   transaction);

            linkCommand.Parameters.AddWithValue("@CategoryId", categoryId);
            linkCommand.Parameters.AddWithValue("@EventId", eventId);

            await linkCommand.ExecuteNonQueryAsync();
        }

        // Nieuwe velden voor INSERT en UPDATE.
        private static void AddEventDetailsParameters(
            SqlCommand command,
            CreateEventInputModel input)
        {
            command.Parameters.Add(
                "@EventType", SqlDbType.NVarChar, 50).Value =
                string.IsNullOrWhiteSpace(input.EventType)
                    ? DBNull.Value
                    : input.EventType.Trim();

            command.Parameters.Add(
                "@ShortDescription", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(input.ShortDescription)
                    ? DBNull.Value
                    : input.ShortDescription.Trim();

            command.Parameters.Add(
                "@StartDateTime", SqlDbType.DateTime2).Value =
                (object?)input.StartDateTime ?? DBNull.Value;

            command.Parameters.Add(
                "@EndDateTime", SqlDbType.DateTime2).Value =
                (object?)input.EndDateTime ?? DBNull.Value;

            command.Parameters.Add(
                "@Notes", SqlDbType.NVarChar, -1).Value =
                string.IsNullOrWhiteSpace(input.Notes)
                    ? DBNull.Value
                    : input.Notes.Trim();
        }

        // Hulpmethodes voor lege databasewaarden.
        private static string GetNullableString(
            SqlDataReader reader,
            string columnName)
        {
            var index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? string.Empty
                : reader.GetString(index);
        }

        private static DateTime? GetNullableDateTime(
            SqlDataReader reader,
            string columnName)
        {
            var index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetDateTime(index);
        }

        private static bool? GetNullableBoolean(
            SqlDataReader reader,
            string columnName)
        {
            var index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetBoolean(index);
        }

        private static decimal? GetNullableDecimal(
            SqlDataReader reader,
            string columnName)
        {
            var index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetDecimal(index);
        }

        private static TimeSpan? GetNullableTimeSpan(
            SqlDataReader reader,
            string columnName)
        {
            var index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetTimeSpan(index);
        }
    }
}
