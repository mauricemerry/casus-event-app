namespace EventApp.OrganiserPortal.Queries
{
    public static class OrganiserQueries
    {
        public const string GetOrganiserByEmail = @"
SELECT TOP 1
    o.organiser_id,
    o.company_name,
    oa.contact_name,
    oa.email,
    oa.password_hash,
    oa.is_active
FROM dbo.organiser_accounts oa
INNER JOIN dbo.organisers o
    ON o.organiser_id = oa.organiser_id
WHERE oa.email = @Email
  AND oa.is_active = 1
  AND o.is_active = 1;";

        public const string GetEventsByOrganiserId = @"
SELECT
    e.event_id,
    e.event_name,
    e.indoors_outdoors,
    e.description,
    e.canceled,
    img.image_url,
    price.min_price,
    cat.categories_text,
    acc.accessibility_text
FROM dbo.events e
LEFT JOIN
(
    SELECT ei.event_id, ei.image_url
    FROM dbo.event_images ei
    WHERE ei.is_primary = 1
) img
    ON img.event_id = e.event_id
LEFT JOIN
(
    SELECT etp.event_id, MIN(etp.price) AS min_price
    FROM dbo.event_ticket_prices etp
    GROUP BY etp.event_id
) price
    ON price.event_id = e.event_id
LEFT JOIN
(
    SELECT
        eec.event_id,
        STRING_AGG(ec.category_name, ', ') AS categories_text
    FROM dbo.event_event_categories eec
    INNER JOIN dbo.event_categories ec
        ON ec.category_id = eec.category_id
    GROUP BY eec.event_id
) cat
    ON cat.event_id = e.event_id
LEFT JOIN
(
    SELECT eao.event_id, STRING_AGG(eao.description, ', ') AS accessibility_text
    FROM dbo.event_accessibility_options eao
    GROUP BY eao.event_id
) acc
    ON acc.event_id = e.event_id
WHERE e.organiser_id = @OrganiserId
ORDER BY e.event_id DESC;";

        public const string GetNextEventId = @"
SELECT ISNULL(MAX(event_id), 0) + 1
FROM dbo.events;";

        public const string GetNextLocationId = @"
SELECT ISNULL(MAX(location_id), 0) + 1
FROM dbo.locations;";

        public const string GetNextTicketPriceId = @"
SELECT ISNULL(MAX(event_ticket_price_id), 0) + 1
FROM dbo.event_ticket_prices;";

        public const string GetNextAccessibilityOptionId = @"
SELECT ISNULL(MAX(accessibility_option_id), 0) + 1
FROM dbo.event_accessibility_options;";

        public const string GetNextEventImageId = @"
SELECT ISNULL(MAX(event_image_id), 0) + 1
FROM dbo.event_images;";

        public const string InsertLocation = @"
INSERT INTO dbo.locations (
    location_id,
    latitude,
    longitude
)
VALUES (
    @LocationId,
    @Latitude,
    @Longitude
);";

        public const string InsertEvent = @"
INSERT INTO dbo.events (
    event_id,
    event_name,
    indoors_outdoors,
    has_standing_places,
    has_sitting_places,
    disabled_parking_available,
    disabled_toilet_available,
    wheelchair_accessible_toilet,
    location_id,
    description,
    organiser_id,
    canceled
)
VALUES (
    @EventId,
    @EventName,
    @IndoorsOutdoors,
    @HasStandingPlaces,
    @HasSittingPlaces,
    @DisabledParkingAvailable,
    @DisabledToiletAvailable,
    @WheelchairAccessibleToilet,
    @LocationId,
    @Description,
    @OrganiserId,
    0
);";

        public const string InsertTicketPrice = @"
INSERT INTO dbo.event_ticket_prices (
    event_ticket_price_id,
    ticket_type,
    price,
    event_id
)
VALUES (
    @EventTicketPriceId,
    @TicketType,
    @Price,
    @EventId
);";

        public const string InsertAccessibilityOption = @"
INSERT INTO dbo.event_accessibility_options (
    accessibility_option_id,
    description,
    event_id
)
VALUES (
    @AccessibilityOptionId,
    @Description,
    @EventId
);";

        public const string FindCategoryByName = @"
SELECT TOP 1 category_id
FROM dbo.event_categories
WHERE LOWER(category_name) = LOWER(@CategoryName);";

        public const string InsertCategory = @"
INSERT INTO dbo.event_categories (
    category_name,
    icon_name
)
OUTPUT INSERTED.category_id
VALUES (
    @CategoryName,
    NULL
);";

        public const string LinkEventCategory = @"
IF NOT EXISTS (
    SELECT 1
    FROM dbo.event_event_categories
    WHERE event_id = @EventId
      AND category_id = @CategoryId
)
BEGIN
    INSERT INTO dbo.event_event_categories (
        category_id,
        event_id
    )
    VALUES (
        @CategoryId,
        @EventId
    );
END";

        public const string InsertEventImage = @"
INSERT INTO dbo.event_images (
    event_image_id,
    event_id,
    image_url,
    alt_text,
    is_primary,
    sort_order
)
VALUES (
    @EventImageId,
    @EventId,
    @ImageUrl,
    NULL,
    @IsPrimary,
    @SortOrder
);";

        public const string GetEventForEdit = @"
SELECT TOP 1
    e.event_id,
    e.event_name,
    e.indoors_outdoors,
    e.description,
    e.canceled,
    l.latitude,
    l.longitude,
    e.has_standing_places,
    e.has_sitting_places,
    e.disabled_parking_available,
    e.disabled_toilet_available,
    e.wheelchair_accessible_toilet
FROM dbo.events e
INNER JOIN dbo.locations l
    ON l.location_id = e.location_id
WHERE e.event_id = @EventId
  AND e.organiser_id = @OrganiserId;";

        public const string UpdateEvent = @"
UPDATE e
SET
    e.event_name = @EventName,
    e.indoors_outdoors = @IndoorsOutdoors,
    e.description = @Description,
    e.has_standing_places = @HasStandingPlaces,
    e.has_sitting_places = @HasSittingPlaces,
    e.disabled_parking_available = @DisabledParkingAvailable,
    e.disabled_toilet_available = @DisabledToiletAvailable,
    e.wheelchair_accessible_toilet = @WheelchairAccessibleToilet
FROM dbo.events e
WHERE e.event_id = @EventId
  AND e.organiser_id = @OrganiserId;";

        public const string UpdateEventLocation = @"
UPDATE l
SET
    l.latitude = @Latitude,
    l.longitude = @Longitude
FROM dbo.locations l
INNER JOIN dbo.events e
    ON e.location_id = l.location_id
WHERE e.event_id = @EventId
  AND e.organiser_id = @OrganiserId;";

        public const string CancelEvent = @"
UPDATE dbo.events
SET canceled = 1
WHERE event_id = @EventId
  AND organiser_id = @OrganiserId;";
    }
}