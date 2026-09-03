namespace EventApp.Web.Repositories.Events
{
    public static class EventQueries
    {
        public const string GetAllEvents = @"
SELECT
    e.event_id,
    e.event_name,
    e.description,
    COALESCE(img.image_url, '/images/events/placeholder.jpg') AS image_url,
    COALESCE(price.min_price, CAST(0 AS DECIMAL(10,2))) AS min_price,
    COALESCE(cat.categories, '') AS categories,
    COALESCE(acc.accessibility_text, '') AS accessibility_text
FROM dbo.events e
OUTER APPLY
(
    SELECT TOP 1 ei.image_url
    FROM dbo.event_images ei
    WHERE ei.event_id = e.event_id
    ORDER BY
        ei.is_primary DESC,
        ei.sort_order ASC,
        ei.event_image_id ASC
) img
OUTER APPLY
(
    SELECT MIN(tp.price) AS min_price
    FROM dbo.event_ticket_prices tp
    WHERE tp.event_id = e.event_id
) price
OUTER APPLY
(
    SELECT STRING_AGG(ec.category_name, ', ') AS categories
    FROM dbo.event_categories ec
    JOIN dbo.event_event_categories eec
    ON ec.category_id = eec.category_id
    WHERE eec.event_id = e.event_id
) cat
OUTER APPLY
(
    SELECT STRING_AGG(eao.description, ', ') AS accessibility_text
    FROM dbo.event_accessibility_options eao
    WHERE eao.event_id = e.event_id
) acc
WHERE ISNULL(e.canceled, 0) = 0
ORDER BY e.event_name;";

        // Dezelfde query als GetAllEvents, maar gefilterd op een dynamische lijst van event ID's.
        // De {parameterList} placeholder wordt vervangen door iets als '@id0, @id1, @id2'
        // zodat de query geparametriseerd en veilig blijft (geen SQL injection).
        public static string GetEventsByIds(string parameterList) => $@"
SELECT
    e.event_id,
    e.event_name,
    e.description,
    COALESCE(img.image_url, '/images/events/placeholder.jpg') AS image_url,
    COALESCE(price.min_price, CAST(0 AS DECIMAL(10,2))) AS min_price,
    COALESCE(cat.categories, '') AS categories,
    COALESCE(acc.accessibility_text, '') AS accessibility_text
FROM dbo.events e
OUTER APPLY
(
    SELECT TOP 1 ei.image_url
    FROM dbo.event_images ei
    WHERE ei.event_id = e.event_id
    ORDER BY
        ei.is_primary DESC,
        ei.sort_order ASC,
        ei.event_image_id ASC
) img
OUTER APPLY
(
    SELECT MIN(tp.price) AS min_price
    FROM dbo.event_ticket_prices tp
    WHERE tp.event_id = e.event_id
) price
OUTER APPLY
(
    SELECT STRING_AGG(ec.category_name, ', ') AS categories
    FROM dbo.event_categories ec
    JOIN dbo.event_event_categories eec
    ON ec.category_id = eec.category_id
    WHERE eec.event_id = e.event_id
) cat
OUTER APPLY
(
    SELECT STRING_AGG(eao.description, ', ') AS accessibility_text
    FROM dbo.event_accessibility_options eao
    WHERE eao.event_id = e.event_id
) acc
WHERE e.event_id IN ({parameterList})
  AND ISNULL(e.canceled, 0) = 0
ORDER BY e.event_name;";
    }
}