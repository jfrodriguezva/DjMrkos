-- DJ MrKos — esquema inicial.
-- Dapper no trae su propio migrador; DbUp aplica estos scripts, en orden, una sola vez cada uno.

CREATE TABLE modules (
    id              UUID PRIMARY KEY,
    name            TEXT NOT NULL,
    slug            TEXT NOT NULL,
    icon            TEXT NULL,
    display_order   INT NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX ux_modules_slug ON modules (slug);
CREATE INDEX ix_modules_active_order ON modules (is_active, display_order);

CREATE TABLE categories (
    id              UUID PRIMARY KEY,
    module_id       UUID NOT NULL REFERENCES modules (id) ON DELETE CASCADE,
    name            TEXT NOT NULL,
    slug            TEXT NOT NULL,
    description     TEXT NULL,
    image_url       TEXT NULL,
    display_order   INT NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX ux_categories_module_slug ON categories (module_id, slug);
CREATE INDEX ix_categories_module_active_order ON categories (module_id, is_active, display_order);

CREATE TABLE events (
    id                  UUID PRIMARY KEY,
    client_name         TEXT NOT NULL,
    location            TEXT NULL,
    event_date_utc      TIMESTAMPTZ NOT NULL,
    status              INT NOT NULL DEFAULT 0,
    qr_token            TEXT NOT NULL,
    qr_valid_from_utc   TIMESTAMPTZ NOT NULL,
    qr_valid_until_utc  TIMESTAMPTZ NOT NULL,
    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX ux_events_qr_token ON events (qr_token);
CREATE INDEX ix_events_date ON events (event_date_utc);

CREATE TABLE song_requests (
    id                      UUID PRIMARY KEY,
    event_id                UUID NOT NULL REFERENCES events (id) ON DELETE CASCADE,
    song_title              TEXT NOT NULL,
    artist                  TEXT NULL,
    requester_name          TEXT NULL,
    dedication              TEXT NULL,
    requester_fingerprint   TEXT NOT NULL,
    status                  INT NOT NULL DEFAULT 0,
    created_at_utc          TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX ix_song_requests_event ON song_requests (event_id, created_at_utc DESC);
CREATE INDEX ix_song_requests_rate_limit ON song_requests (event_id, requester_fingerprint, created_at_utc);

CREATE TABLE testimonials (
    id              UUID PRIMARY KEY,
    client_name     TEXT NOT NULL,
    event_id        UUID NULL REFERENCES events (id) ON DELETE SET NULL,
    rating          INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment         TEXT NOT NULL,
    is_approved     BOOLEAN NOT NULL DEFAULT FALSE,
    created_at_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX ix_testimonials_approved ON testimonials (is_approved, created_at_utc DESC);

CREATE TABLE leads (
    id              UUID PRIMARY KEY,
    name            TEXT NOT NULL,
    email           TEXT NOT NULL,
    phone           TEXT NULL,
    event_date      DATE NULL,
    message         TEXT NOT NULL,
    status          INT NOT NULL DEFAULT 0,
    created_at_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX ix_leads_created ON leads (created_at_utc DESC);
