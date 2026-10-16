
using System.Data;
using Microsoft.Data.SqlClient;
using EventApp.OrganiserPortal.Models;

namespace EventApp.OrganiserPortal.Repositories;

public sealed class EventWizardWriter
{
    private readonly string _connectionString;

    public EventWizardWriter(IConfiguration config)
    {
        _connectionString =
            config.GetConnectionString("db-eventapp")
            ?? throw new InvalidOperationException(
                "Connection string db-eventapp ontbreekt.");
    }

    public async Task<int> SaveAsync(
        int organiserId,
        CreateEventInputModel input)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();

        await using var tx =
            (SqlTransaction)await db.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            // Voert INSERT of UPDATE uit.
            async Task Run(
                string sql,
                params (string Name, object? Value)[] values)
            {
                await using var cmd = new SqlCommand(sql, db, tx);

                foreach (var (name, value) in values)
                {
                    cmd.Parameters.AddWithValue(
                        name,
                        value ?? DBNull.Value);
                }

                await cmd.ExecuteNonQueryAsync();
            }

            // Haalt één waarde op.
            async Task<object?> Scalar(
                string sql,
                params (string Name, object? Value)[] values)
            {
                await using var cmd = new SqlCommand(sql, db, tx);

                foreach (var (name, value) in values)
                {
                    cmd.Parameters.AddWithValue(
                        name,
                        value ?? DBNull.Value);
                }

                return await cmd.ExecuteScalarAsync();
            }

            // Volgend ID uit bestaande database.
            // Alleen aanroepen met vaste tabel- en kolomnamen.
            async Task<int> Next(
                string table,
                string idColumn)
            {
                var result = await Scalar(
                    $"SELECT ISNULL(MAX({idColumn}),0)+1 " +
                    $"FROM dbo.{table} WITH (UPDLOCK,HOLDLOCK)");

                return Convert.ToInt32(result);
            }

            int eventId = await Next("events", "event_id");
            int locationId = await Next("locations", "location_id");

            var first = input.Locations[0];

            // 1. Hoofdlocatie voor de bestaande foreign key.
            await Run(
                @"INSERT INTO dbo.locations
                  (location_id,latitude,longitude)
                  VALUES(@id,@lat,@lng)",
                ("@id", locationId),
                ("@lat", Math.Round(first.Latitude!.Value, 6)),
                ("@lng", Math.Round(first.Longitude!.Value, 6)));

            // Bestaande kolom kent 1=Binnen, 2=Buiten, 3=Beide.
            // Online wordt apart bewaard.
            int environment =
                input.IsIndoor && input.IsOutdoor ? 3 :
                input.IsIndoor ? 1 :
                input.IsOutdoor ? 2 : 3;

            var minStart = input.Periods.Min(p =>
                p.StartDate!.Value.Date.Add(p.StartTime!.Value));

            var maxEnd = input.Periods.Max(p =>
                p.EndDate!.Value.Date.Add(p.EndTime!.Value));

            // 2. Evenement
            await Run(
                @"INSERT INTO dbo.events
                (
                    event_id,
                    event_name,
                    event_type,
                    short_description,
                    indoors_outdoors,
                    location_id,
                    description,
                    notes,
                    organiser_id,
                    canceled,
                    start_datetime,
                    end_datetime,
                    is_online
                )
                VALUES
                (
                    @id,@name,@type,@short,@environment,
                    @location,@description,@notes,@organiser,
                    0,@start,@end,@online
                )",
                ("@id", eventId),
                ("@name", input.EventName.Trim()),
                ("@type", input.EventType),
                ("@short", input.ShortDescription),
                ("@environment", environment),
                ("@location", locationId),
                ("@description", input.Description.Trim()),
                ("@notes", input.Notes),
                ("@organiser", organiserId),
                ("@start", minStart),
                ("@end", maxEnd),
                ("@online", input.IsOnline));

            // 3. Meerdere periodes
            foreach (var period in input.Periods)
            {
                await Run(
                    @"INSERT INTO dbo.event_dates
                    (
                        event_id,
                        start_date,
                        end_date,
                        period_name,
                        start_time,
                        end_time
                    )
                    VALUES(@id,@sd,@ed,@name,@st,@et)",
                    ("@id", eventId),
                    ("@sd", period.StartDate!.Value.Date),
                    ("@ed", period.EndDate!.Value.Date),
                    ("@name", period.Name),
                    ("@st", period.StartTime!.Value),
                    ("@et", period.EndTime!.Value));
            }

            // 4. Alle locaties
            foreach (var location in input.Locations)
            {
                await Run(
                    @"INSERT INTO dbo.event_locations
                    (
                        event_id,
                        location_name,
                        location_type,
                        city,
                        street,
                        house_number,
                        house_number_addition,
                        postal_code,
                        latitude,
                        longitude
                    )
                    VALUES
                    (
                        @id,@name,NULL,@city,@street,
                        @number,@addition,@postal,@lat,@lng
                    )",
                    ("@id", eventId),
                    ("@name", location.Name),
                    ("@city", location.City),
                    ("@street", location.Street),
                    ("@number", location.Number),
                    ("@addition", location.Addition),
                    ("@postal", location.PostalCode),
                    ("@lat", location.Latitude),
                    ("@lng", location.Longitude));
            }

            // 5. Tickets
            int ticketId = await Next(
                "event_ticket_prices",
                "event_ticket_price_id");

            foreach (var ticket in input.Tickets)
            {
                bool free =
                    ticket.DateType == "Gratis" ||
                    ticket.TicketType.Trim().Equals(
                        "gratis",
                        StringComparison.OrdinalIgnoreCase);

                decimal price = free ? 0 : ticket.Price;

                DateTime? start = null;
                DateTime? end = null;

                if (ticket.DateType == "Dagticket" &&
                    ticket.TicketDate.HasValue)
                {
                    start = ticket.TicketDate.Value.Date;
                }

                if (ticket.DateType == "Tijdslot" &&
                    ticket.TicketDate.HasValue)
                {
                    start = ticket.TicketDate.Value.Date.Add(
                        ticket.SlotStart!.Value);

                    end = ticket.TicketDate.Value.Date.Add(
                        ticket.SlotEnd!.Value);
                }

                await Run(
                    @"INSERT INTO dbo.event_ticket_prices
                    (
                        event_ticket_price_id,
                        ticket_type,
                        price,
                        event_id,
                        ticket_date_type,
                        start_datetime,
                        end_datetime
                    )
                    VALUES
                    (
                        @tid,@name,@price,@id,@kind,@start,@end
                    )",
                    ("@tid", ticketId),
                    ("@name", ticket.TicketType.Trim()),
                    ("@price", price),
                    ("@id", eventId),
                    ("@kind", ticket.DateType),
                    ("@start", start),
                    ("@end", end));

                // Losse ticketdagen
                if (ticket.DateType == "Periode")
                {
                    foreach (var date in ticket.PeriodDates
                        .Where(d => d.HasValue)
                        .Distinct())
                    {
                        await Run(
                            @"INSERT INTO dbo.event_ticket_dates
                            (event_ticket_price_id,ticket_date)
                            VALUES(@ticket,@date)",
                            ("@ticket", ticketId),
                            ("@date", date!.Value.Date));
                    }
                }

                ticketId++;
            }

            // Zonder tickets automatisch een gratis ticket.
            if (input.Tickets.Count == 0)
            {
                await Run(
                    @"INSERT INTO dbo.event_ticket_prices
                    (
                        event_ticket_price_id,
                        ticket_type,
                        price,
                        event_id,
                        ticket_date_type
                    )
                    VALUES(@ticket,'Gratis',0,@event,'Gratis')",
                    ("@ticket", ticketId),
                    ("@event", eventId));
            }

            // 6. Toegankelijkheid
            int accessibilityId = await Next(
                "event_accessibility_options",
                "accessibility_option_id");

            var accessibility = input.AccessibilityOptions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (input.HasOtherAccessibility &&
                !string.IsNullOrWhiteSpace(
                    input.OtherAccessibilityDescription))
            {
                accessibility.Add(
                    "Overige: " +
                    input.OtherAccessibilityDescription.Trim());
            }

            foreach (string option in accessibility)
            {
                await Run(
                    @"INSERT INTO dbo.event_accessibility_options
                    (accessibility_option_id,description,event_id)
                    VALUES(@id,@text,@event)",
                    ("@id", accessibilityId++),
                    ("@text", option),
                    ("@event", eventId));
            }

            // 7. Categorie
            if (!string.IsNullOrWhiteSpace(input.SelectedCategory))
            {
                object? existing = await Scalar(
                    @"SELECT TOP 1 category_id
                      FROM dbo.event_categories
                      WHERE LOWER(category_name)=LOWER(@name)",
                    ("@name", input.SelectedCategory));

                int categoryId;

                if (existing is null || existing == DBNull.Value)
                {
                    categoryId = Convert.ToInt32(
                        await Scalar(
                            @"INSERT INTO dbo.event_categories
                            (category_name,icon_name)
                            OUTPUT INSERTED.category_id
                            VALUES(@name,NULL)",
                            ("@name", input.SelectedCategory)));
                }
                else
                {
                    categoryId = Convert.ToInt32(existing);
                }

                await Run(
                    @"INSERT INTO dbo.event_event_categories
                    (category_id,event_id)
                    VALUES(@category,@event)",
                    ("@category", categoryId),
                    ("@event", eventId));
            }

            // 8. Tags
            foreach (string tag in input.Tags.Distinct(
                StringComparer.OrdinalIgnoreCase))
            {
                await Run(
                    @"INSERT INTO dbo.event_tags
                    (event_id,tag_name)
                    VALUES(@id,@tag)",
                    ("@id", eventId),
                    ("@tag", tag));
            }

            await tx.CommitAsync();

            return eventId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
