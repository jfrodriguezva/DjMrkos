-- Galería por evento (álbumes con fotos que sube el DJ desde el panel) y fechas bloqueadas a
-- mano en el calendario público (vacaciones, compromisos personales) — además de los días que
-- ya ocupa un evento.

CREATE TABLE gallery_albums (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    title           NVARCHAR(200) NOT NULL,
    slug            NVARCHAR(220) NOT NULL,
    event_date      DATE NULL,
    description     NVARCHAR(1000) NULL,
    -- No FK on purpose: images already cascade from albums, and a second path back would be a
    -- cycle. GalleryRepository clears it when the cover image is deleted.
    cover_image_id  UNIQUEIDENTIFIER NULL,
    is_published    BIT NOT NULL DEFAULT 0,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE UNIQUE INDEX ux_gallery_albums_slug ON gallery_albums (slug);
CREATE INDEX ix_gallery_albums_published_date ON gallery_albums (is_published, event_date);
GO

CREATE TABLE gallery_images (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    album_id        UNIQUEIDENTIFIER NOT NULL REFERENCES gallery_albums (id) ON DELETE CASCADE,
    caption         NVARCHAR(300) NULL,
    width           INT NOT NULL,
    height          INT NOT NULL,
    size_bytes      BIGINT NOT NULL,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE INDEX ix_gallery_images_album ON gallery_images (album_id, created_at_utc);
GO

CREATE TABLE blocked_dates (
    id              UNIQUEIDENTIFIER PRIMARY KEY,
    blocked_date    DATE NOT NULL,
    reason          NVARCHAR(200) NULL,
    created_at_utc  DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET)
);

CREATE UNIQUE INDEX ux_blocked_dates_date ON blocked_dates (blocked_date);
