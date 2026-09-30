# DJ MrKos

Portal web para DJ MrKos: catálogo de servicios **configurable** (Módulos → Categorías, con precio), un embudo de reserva **Agendar → Cotizar → Contratar**, solicitud de canciones en vivo por **código QR**, panel de administración, y todo el contenido de marca (misión, visión, objetivos, paleta).

- **Backend:** .NET 10 · Clean Architecture · Dapper · Polly · SignalR
- **Frontend:** React 19 · Vite · TypeScript · React Query
- **Base de datos:** SQL Server, versionada con DbUp (no EF Core migrations)

## Estructura del repositorio

```
DjMrkos/
├── backend/
│   ├── src/
│   │   ├── DjMrkos.Domain/          # Entidades y reglas de negocio, sin dependencias externas
│   │   ├── DjMrkos.Application/     # Casos de uso (CQRS con MediatR), DTOs, interfaces (puertos)
│   │   ├── DjMrkos.Infrastructure/  # Dapper + Polly, SignalR, QR, implementación de los puertos
│   │   ├── DjMrkos.Api/             # Minimal API, auth por API key, Swagger, Program.cs
│   │   └── DjMrkos.Migrator/        # Consola DbUp — aplica backend/src/DjMrkos.Migrator/Scripts/*.sql
│   └── tests/
│       └── DjMrkos.Application.UnitTests/
├── frontend/                        # React + Vite — portal público, página QR y panel admin
├── database/                        # (reservado para scripts ad-hoc / backups)
├── docker-compose.yml                # PostgreSQL para desarrollo local
└── docs/
    ├── ARCHITECTURE.md
    ├── API.md
    ├── DATABASE.md
    └── FRONTEND.md
```

## Levantar el proyecto localmente

### 1. Base de datos

```bash
docker compose up -d
```

Levanta SQL Server en `localhost:1433` con las credenciales que ya están en `backend/src/DjMrkos.Api/appsettings.Development.json` (usuario `sa`, ver `MSSQL_SA_PASSWORD` en `docker-compose.yml`).

### 2. Aplicar el esquema (DbUp)

```bash
dotnet run --project backend/src/DjMrkos.Migrator -- "Server=localhost,1433;Database=djmrkos;User Id=sa;Password=djmrkos_dev_P@ss1;TrustServerCertificate=True"
```

Esto crea las tablas y siembra el catálogo real del negocio: 7 módulos y 44 servicios con precio (Personal y Staff, Cabina, Iluminación, Efectos Especiales, Pantallas y Proyección, Audio Profesional, Personajes y Shows).

### 3. Backend

```bash
dotnet run --project backend/src/DjMrkos.Api
```

Por defecto corre en `http://localhost:5027` (perfil `http` de `launchSettings.json`), con Swagger en `/swagger`.

### 4. Frontend

```bash
cd frontend
npm install
npm run dev
```

Corre en `http://localhost:5173` y hace proxy de `/api` y `/hubs` hacia el backend (`vite.config.ts`).

## Probar el flujo completo

**Solicitud de canciones en vivo:**
1. Entra al panel: `http://localhost:5173/admin/login` con la API key de desarrollo `dev-admin-key`.
2. En la pestaña **Eventos**, crea un evento — se genera su código QR al instante.
3. Abre `http://localhost:5173/evento/{token}` (el token que se generó) en otra pestaña o en tu celular — pide una canción.
4. Vuelve al panel, pestaña **Cola en vivo**: la solicitud aparece en tiempo real vía SignalR.

**Embudo de reserva (Agendar → Cotizar → Contratar):**
1. Abre `http://localhost:5173/agendar`, elige una fecha disponible en el calendario.
2. Ve a `/cotizador` y arrastra (o toca **+**) los servicios que quieras — la fecha elegida se conserva.
3. Ve a `/contratar`: verás el mismo presupuesto y fecha, listos para confirmar.
4. En el panel admin, pestaña **Cotizaciones**, la solicitud aparece etiquetada **Contratación**, **Cotización** o **Agendar** según de dónde vino.

## Alerta al DJ por nueva solicitud de canción (opcional, gratis)

El panel admin ya muestra las solicitudes en tiempo real vía SignalR, pero eso solo sirve si el panel está abierto. Para recibir un aviso en el celular aunque el panel esté cerrado:

1. Habla con [@BotFather](https://t.me/BotFather) en Telegram, crea un bot con `/newbot` y copia el token que te da.
2. Escríbele un mensaje a tu bot nuevo (cualquier cosa) y abre `https://api.telegram.org/bot<TU_TOKEN>/getUpdates` en el navegador — ahí aparece tu `chat.id`.
3. Pon ambos valores en `Telegram:BotToken` y `Telegram:ChatId` en `appsettings.Development.json` (o como variables de entorno en producción).

Sin esto configurado, la app funciona igual — simplemente no manda la alerta.

## Despliegue en producción (VPS, todo en Docker)

Ver [deploy/README.md](deploy/README.md) — SQL Server + migrador + API + Caddy (frontend + reverse proxy con HTTPS automático), todo con `docker compose -f deploy/docker-compose.prod.yml`.

Si el VPS ya tiene otro proyecto con su propio SQL Server y nginx (caso actual, junto a Lawyer), usa [deploy/vps-compartido/README.md](deploy/vps-compartido/README.md): solo agrega API + frontend + migrador y reutiliza la BD y el HTTPS existentes.

## Documentación

- [Arquitectura](docs/ARCHITECTURE.md) — capas, resiliencia con Polly, seguridad del QR, auth admin
- [API](docs/API.md) — referencia de endpoints
- [Base de datos](docs/DATABASE.md) — esquema, migraciones, decisiones de modelado
- [Frontend](docs/FRONTEND.md) — sistema de diseño, estructura, tiempo real
- [Despliegue](deploy/README.md) — VPS, Docker, HTTPS
