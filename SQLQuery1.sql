
/*
    EVENT EVOLUTION
    Database-uitbreiding voor evenement aanmaken

    Bestaande tabellen en gegevens blijven behouden.
    Nieuwe onderdelen worden alleen aangemaakt
    wanneer ze nog niet bestaan.
*/

-- ==========================================
-- 1. BASISGEGEVENS
-- ==========================================

IF COL_LENGTH('dbo.events', 'event_type') IS NULL
BEGIN
    ALTER TABLE dbo.events
    ADD event_type NVARCHAR(50) NULL;
END;

IF COL_LENGTH('dbo.events', 'short_description') IS NULL
BEGIN
    ALTER TABLE dbo.events
    ADD short_description NVARCHAR(500) NULL;
END;


-- ==========================================
-- 2. DATUM EN TIJD
-- ==========================================

IF COL_LENGTH('dbo.events', 'start_datetime') IS NULL
BEGIN
    ALTER TABLE dbo.events
    ADD start_datetime DATETIME2 NULL;
END;

IF COL_LENGTH('dbo.events', 'end_datetime') IS NULL
BEGIN
    ALTER TABLE dbo.events
    ADD end_datetime DATETIME2 NULL;
END;


-- ==========================================
-- 3. EXTRA INFORMATIE
-- ==========================================

IF COL_LENGTH('dbo.events', 'notes') IS NULL
BEGIN
    ALTER TABLE dbo.events
    ADD notes NVARCHAR(MAX) NULL;
END;


-- ==========================================
-- 4. MEERDERE EVENEMENTLOCATIES
-- ==========================================

IF OBJECT_ID('dbo.event_locations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.event_locations
    (
        event_location_id INT IDENTITY(1,1) NOT NULL
            PRIMARY KEY,

        event_id INT NOT NULL,

        location_name NVARCHAR(100) NOT NULL,

        location_type NVARCHAR(50) NULL,

        city NVARCHAR(100) NULL,

        street NVARCHAR(150) NULL,

        house_number NVARCHAR(20) NULL,

        house_number_addition NVARCHAR(20) NULL,

        postal_code NVARCHAR(20) NULL,

        latitude DECIMAL(9,6) NULL,

        longitude DECIMAL(9,6) NULL,

        CONSTRAINT FK_event_locations_events
            FOREIGN KEY (event_id)
            REFERENCES dbo.events(event_id)
    );
END;


-- ==========================================
-- 5. TICKETDATUMS EN TIJDSLOTEN
-- ==========================================

IF COL_LENGTH('dbo.event_ticket_prices', 'ticket_date_type') IS NULL
BEGIN
    ALTER TABLE dbo.event_ticket_prices
    ADD ticket_date_type NVARCHAR(30) NULL;
END;

IF COL_LENGTH('dbo.event_ticket_prices', 'start_datetime') IS NULL
BEGIN
    ALTER TABLE dbo.event_ticket_prices
    ADD start_datetime DATETIME2 NULL;
END;

IF COL_LENGTH('dbo.event_ticket_prices', 'end_datetime') IS NULL
BEGIN
    ALTER TABLE dbo.event_ticket_prices
    ADD end_datetime DATETIME2 NULL;
END;


-- ==========================================
-- 6. MEERDERE DAGEN PER TICKET
-- ==========================================

IF OBJECT_ID('dbo.event_ticket_dates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.event_ticket_dates
    (
        event_ticket_date_id INT IDENTITY(1,1) NOT NULL
            PRIMARY KEY,

        event_ticket_price_id INT NOT NULL,

        ticket_date DATE NOT NULL,

        CONSTRAINT FK_event_ticket_dates_prices
            FOREIGN KEY (event_ticket_price_id)
            REFERENCES dbo.event_ticket_prices(event_ticket_price_id)
    );
END;


-- ==========================================
-- 7. CONTROLE
-- ==========================================

SELECT
    COLUMN_NAME,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'events'
ORDER BY ORDINAL_POSITION;

SELECT
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME IN
  (
      'event_locations',
      'event_ticket_dates'
  );
