
-- EVENT EVOLUTION WIZARD DATABASE-UPGRADE
-- Toevoegingen aan bestaande tabellen.
-- Controleer de geselecteerde database en maak eerst een back-up.

-- EVENEMENT

IF COL_LENGTH('dbo.events', 'event_type') IS NULL
    ALTER TABLE dbo.events
    ADD event_type NVARCHAR(50) NULL;

IF COL_LENGTH('dbo.events', 'short_description') IS NULL
    ALTER TABLE dbo.events
    ADD short_description NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.events', 'start_datetime') IS NULL
    ALTER TABLE dbo.events
    ADD start_datetime DATETIME2 NULL;

IF COL_LENGTH('dbo.events', 'end_datetime') IS NULL
    ALTER TABLE dbo.events
    ADD end_datetime DATETIME2 NULL;

IF COL_LENGTH('dbo.events', 'notes') IS NULL
    ALTER TABLE dbo.events
    ADD notes NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.events', 'is_online') IS NULL
    ALTER TABLE dbo.events
    ADD is_online BIT NOT NULL
        CONSTRAINT DF_events_is_online DEFAULT(0);

-- MEERDERE PERIODES

IF COL_LENGTH('dbo.event_dates', 'period_name') IS NULL
    ALTER TABLE dbo.event_dates
    ADD period_name NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.event_dates', 'start_time') IS NULL
    ALTER TABLE dbo.event_dates
    ADD start_time TIME(0) NULL;

IF COL_LENGTH('dbo.event_dates', 'end_time') IS NULL
    ALTER TABLE dbo.event_dates
    ADD end_time TIME(0) NULL;

-- TICKETTYPES EN DATUMS

IF COL_LENGTH(
    'dbo.event_ticket_prices',
    'ticket_date_type'
) IS NULL
    ALTER TABLE dbo.event_ticket_prices
    ADD ticket_date_type NVARCHAR(30) NULL;

IF COL_LENGTH(
    'dbo.event_ticket_prices',
    'start_datetime'
) IS NULL
    ALTER TABLE dbo.event_ticket_prices
    ADD start_datetime DATETIME2 NULL;

IF COL_LENGTH(
    'dbo.event_ticket_prices',
    'end_datetime'
) IS NULL
    ALTER TABLE dbo.event_ticket_prices
    ADD end_datetime DATETIME2 NULL;

-- MEERDERE LOCATIES

IF OBJECT_ID('dbo.event_locations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.event_locations
    (
        event_location_id INT IDENTITY(1,1) PRIMARY KEY,

        event_id INT NOT NULL
            REFERENCES dbo.events(event_id),

        location_name NVARCHAR(100) NOT NULL,

        location_type NVARCHAR(50) NULL,

        city NVARCHAR(100) NULL,
        street NVARCHAR(150) NULL,
        house_number NVARCHAR(20) NULL,
        house_number_addition NVARCHAR(20) NULL,
        postal_code NVARCHAR(20) NULL,

        latitude DECIMAL(9,6) NULL,
        longitude DECIMAL(9,6) NULL
    );
END;

-- MEERDERE TICKETDAGEN

IF OBJECT_ID('dbo.event_ticket_dates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.event_ticket_dates
    (
        event_ticket_date_id INT IDENTITY(1,1) PRIMARY KEY,

        event_ticket_price_id INT NOT NULL
            REFERENCES dbo.event_ticket_prices(
                event_ticket_price_id
            ),

        ticket_date DATE NOT NULL
    );
END;

-- TAGS

IF OBJECT_ID('dbo.event_tags', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.event_tags
    (
        event_tag_id INT IDENTITY(1,1) PRIMARY KEY,

        event_id INT NOT NULL
            REFERENCES dbo.events(event_id),

        tag_name NVARCHAR(100) NOT NULL
    );
END;

SELECT 'Klaar' AS Migratiestatus;
