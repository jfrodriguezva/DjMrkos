-- DJ MrKos — esquema inicial.
-- Dapper no trae su propio migrador; DbUp aplica estos scripts, en orden, una sola vez cada uno.

CREATE TABLE modules (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    name            NVARCHAR(200) NOT NULL,
    slug            NVARCHAR(200) NOT NULL,
    icon            NVARCHAR(100) NULL,
    display_order   INT NOT NULL DEFAULT 0,
    is_active       BIT NOT NULL DEFAULT 1,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE UNIQUE INDEX ux_modules_slug ON modules (slug);
CREATE INDEX ix_modules_active_order ON modules (is_active, display_order);
GO

CREATE TABLE categories (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    module_id       UNIQUEIDENTIFIER NOT NULL REFERENCES modules (id) ON DELETE CASCADE,
    name            NVARCHAR(200) NOT NULL,
    slug            NVARCHAR(200) NOT NULL,
    description     NVARCHAR(MAX) NULL,
    image_url       NVARCHAR(500) NULL,
    display_order   INT NOT NULL DEFAULT 0,
    is_active       BIT NOT NULL DEFAULT 1,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE UNIQUE INDEX ux_categories_module_slug ON categories (module_id, slug);
CREATE INDEX ix_categories_module_active_order ON categories (module_id, is_active, display_order);
GO

CREATE TABLE events (
    id                  UNIQUEIDENTIFIER PRIMARY KEY,
    client_name         NVARCHAR(200) NOT NULL,
    location            NVARCHAR(300) NULL,
    event_date_utc      DATETIMEOFFSET NOT NULL,
    status              INT NOT NULL DEFAULT 0,
    qr_token            NVARCHAR(200) NOT NULL,
    qr_valid_from_utc   DATETIMEOFFSET NOT NULL,
    qr_valid_until_utc  DATETIMEOFFSET NOT NULL,
    created_at_utc      DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE UNIQUE INDEX ux_events_qr_token ON events (qr_token);
CREATE INDEX ix_events_date ON events (event_date_utc);
GO

CREATE TABLE song_requests (
    id                      UNIQUEIDENTIFIER PRIMARY KEY,
    event_id                UNIQUEIDENTIFIER NOT NULL REFERENCES events (id) ON DELETE CASCADE,
    song_title              NVARCHAR(300) NOT NULL,
    artist                  NVARCHAR(300) NULL,
    requester_name          NVARCHAR(200) NULL,
    dedication              NVARCHAR(MAX) NULL,
    requester_fingerprint   NVARCHAR(200) NOT NULL,
    status                  INT NOT NULL DEFAULT 0,
    created_at_utc          DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE INDEX ix_song_requests_event ON song_requests (event_id, created_at_utc DESC);
CREATE INDEX ix_song_requests_rate_limit ON song_requests (event_id, requester_fingerprint, created_at_utc);
GO

CREATE TABLE testimonials (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    client_name     NVARCHAR(200) NOT NULL,
    event_id        UNIQUEIDENTIFIER NULL REFERENCES events (id) ON DELETE SET NULL,
    rating          INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment         NVARCHAR(MAX) NOT NULL,
    is_approved     BIT NOT NULL DEFAULT 0,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE INDEX ix_testimonials_approved ON testimonials (is_approved, created_at_utc DESC);
GO

CREATE TABLE leads (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    name            NVARCHAR(200) NOT NULL,
    email           NVARCHAR(320) NOT NULL,
    phone           NVARCHAR(30) NULL,
    event_date      DATE NULL,
    message         NVARCHAR(MAX) NOT NULL,
    status          INT NOT NULL DEFAULT 0,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE INDEX ix_leads_created ON leads (created_at_utc DESC);
