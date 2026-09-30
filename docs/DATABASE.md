# Base de datos

SQL Server. Sin ORM de por medio — el esquema vive en SQL plano en `backend/src/DjMrkos.Migrator/Scripts/`, aplicado por **DbUp** (no hay migraciones de EF Core; ver [ARCHITECTURE.md](ARCHITECTURE.md#por-qué-dapper-y-no-ef-core)).

## Tablas

```mermaid
erDiagram
    modules ||--o{ categories : "tiene"
    events ||--o{ song_requests : "recibe"
    events ||--o{ testimonials : "opcional"
    modules ||--o{ promotions : "opcional"
    categories ||--o{ promotions : "opcional"

    modules {
        uniqueidentifier id PK
        nvarchar name
        nvarchar slug
        nvarchar icon
        int display_order
        bit is_active
    }
    categories {
        uniqueidentifier id PK
        uniqueidentifier module_id FK
        nvarchar name
        nvarchar slug
        nvarchar description
        nvarchar image_url
        numeric price "nullable — null = incluido/a cotizar"
        int display_order
        bit is_active
    }
    events {
        uniqueidentifier id PK
        nvarchar client_name
        nvarchar location
        datetimeoffset event_date_utc
        int status
        nvarchar qr_token UK
        datetimeoffset qr_valid_from_utc
        datetimeoffset qr_valid_until_utc
    }
    song_requests {
        uniqueidentifier id PK
        uniqueidentifier event_id FK
        nvarchar song_title
        nvarchar artist
        nvarchar requester_name
        nvarchar dedication
        nvarchar requester_fingerprint
        int status
    }
    testimonials {
        uniqueidentifier id PK
        nvarchar client_name
        uniqueidentifier event_id FK
        int rating
        nvarchar comment
        bit is_approved
    }
    leads {
        uniqueidentifier id PK
        nvarchar name
        nvarchar email
        nvarchar phone
        date event_date
        nvarchar message
        int status
    }
    promotions {
        uniqueidentifier id PK
        uniqueidentifier module_id FK "nullable — exactamente uno de module_id/category_id"
        uniqueidentifier category_id FK "nullable"
        nvarchar label
        numeric discount_percentage
        bit is_active
    }
```

Columnas en `snake_case` — Dapper las mapea a las propiedades `PascalCase` de C# gracias a una sola línea en el arranque: `Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true` (`Infrastructure/DependencyInjection.cs`).

## Decisiones de modelado

- **`categories.module_id` es `NOT NULL`** — una categoría siempre pertenece a exactamente un módulo. Esa es la única regla de jerarquía que sostiene todo el menú configurable; no existe un tercer nivel en la base (un "ítem" dentro de una categoría queda documentado como extensión futura en el modelo de Application, no implementado en v1).
- **Soft delete en `modules` y `categories`** (`is_active = false`, sin `DELETE`) — un módulo desactivado puede tener categorías que a su vez referencian solicitudes de canciones históricas indirectamente vía el catálogo mostrado en su momento. Borrar la fila rompería esa trazabilidad sin ganar nada.
- **`events.qr_token` es único** y las columnas `qr_valid_from_utc` / `qr_valid_until_utc` viven en la fila del evento, no calculadas al vuelo desde `event_date_utc` en cada consulta — así el negocio puede, en el futuro, ajustar la ventana de un evento puntual sin tocar código.
- **`song_requests.requester_fingerprint`** no es un dato personal — es un hash SHA-256 de IP + User-Agent, usado únicamente por el limitador de tasa (`ix_song_requests_rate_limit`). No hay forma de revertirlo a una identidad.
- **`categories.price`** es nullable a propósito: `NULL` significa "va incluido al contratar el módulo, no es un artículo independiente" (p. ej. los géneros musicales del DJ), mientras que un valor es el precio "desde" que el cotizador del frontend usa para armar el presupuesto — ver [FRONTEND.md](FRONTEND.md#cotizador-tipo-carrito).
- **Índices** pensados para las dos consultas calientes: el menú público (`ix_modules_active_order`, `ix_categories_module_active_order`) y la cola en vivo de un evento (`ix_song_requests_event`).
- **`promotions`** solo puede apuntar a un módulo completo o a una categoría, nunca a ambos ni a ninguno — reforzado tanto por `Promotion.Create` en el dominio como por el `CHECK ck_promotions_exactly_one_target` en la base, así que un dato corrupto no puede colarse ni siquiera por un `INSERT` manual.

## Nota de compatibilidad con Dapper

Los repositorios mapean cada fila a un `record` con **solo propiedades `init`** (sin constructor posicional). Esto no es estilo — un `record` con constructor posicional fuerza a Dapper a mapear por posición de parámetro, que espera un nombre literal como `display_order`, y `DefaultTypeMap.MatchNamesWithUnderscores` nunca entra en juego para ese camino. Con propiedades `init`, Dapper mapea por *setter* y sí resuelve `display_order` → `DisplayOrder`. Si agregas un repositorio nuevo, sigue el mismo patrón (ver cualquier `*Row` en `Infrastructure/Repositories/`).

## Aplicar el esquema

```bash
dotnet run --project backend/src/DjMrkos.Migrator -- "Server=localhost,1433;Database=djmrkos;User Id=sa;Password=djmrkos_dev_P@ss1;TrustServerCertificate=True"
```

`0001_InitialSchema.sql` crea las tablas. `0002_SeedMenu.sql` y `0003_CategoryPricingAndEntertainment.sql` sembraron un catálogo de ejemplo inicial; `0004_FullServiceCatalog.sql` lo **reemplaza por completo** con el catálogo real del negocio — 7 módulos y 44 categorías con precio (Personal y Staff, Cabina, Iluminación, Efectos Especiales, Pantallas y Proyección, Audio Profesional, Personajes y Shows). Es seguro vaciar y repoblar `modules`/`categories` en una sola migración porque ninguna otra tabla las referencia por clave foránea.

Agregar un cambio de esquema es agregar un archivo `000N_Descripcion.sql` nuevo en `Scripts/` — nunca edites uno que ya corrió; DbUp lleva su propio registro de qué scripts ya se aplicaron y nunca reaplica uno.
