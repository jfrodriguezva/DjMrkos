# Arquitectura

## Capas (Clean Architecture)

```
DjMrkos.Api  →  DjMrkos.Infrastructure  →  DjMrkos.Application  →  DjMrkos.Domain
```

La regla de dependencia solo apunta hacia adentro. `Domain` no referencia ningún paquete NuGet ni ningún otro proyecto — son entidades con comportamiento (`Module`, `Category`, `Event`, `SongRequest`, `Testimonial`, `Lead`) y sus invariantes, sin saber que Dapper, PostgreSQL o HTTP existen.

- **Domain** — Entidades con métodos que protegen sus propias reglas (`Event.IsQrWindowOpen`, `Module.Rename`, `SongRequest.MarkPlayed`). Nada de setters públicos ni anemia de modelo.
- **Application** — Casos de uso con **CQRS vía MediatR**: cada `Command`/`Query` vive junto a su `Handler` y su `Validator` (FluentValidation) en la misma carpeta por feature (`Modules/`, `Events/`, `SongRequests/`, …). Define los **puertos** (`IModuleRepository`, `IEventRepository`, `ISongRequestNotifier`, `IQrTokenService`, …) que Infrastructure implementa — inversión de dependencias real, no solo de nombre.
- **Infrastructure** — Implementa los puertos: repositorios con Dapper, el hub de SignalR, el servicio de QR, el pipeline de resiliencia de Polly.
- **Api** — Minimal API organizada por feature en `Endpoints/`, middleware de excepciones, autenticación por API key, Swagger.

Un `ValidationBehavior<TRequest,TResponse>` (pipeline de MediatR) corre todos los validadores registrados antes de que cualquier handler se ejecute — ningún handler valida su propio input.

## Resiliencia con Polly

Todo acceso a PostgreSQL pasa por `IResilientDbExecutor` (`Infrastructure/Persistence/ResilientDbExecutor.cs`), que envuelve cada llamada de Dapper en un pipeline de Polly v8 compuesto, de afuera hacia adentro:

1. **Retry** — hasta 3 intentos con backoff exponencial y jitter, solo para `NpgsqlException` marcadas como transitorias (`ex.IsTransient`). Un error de SQL mal escrito o una violación de constraint **no** se reintenta.
2. **Circuit breaker** — se abre tras una ráfaga de fallos (≥50% de fallos con un mínimo de 8 llamadas en 30s) y lo mantiene abierto 15s, para que una base de datos caída falle rápido en vez de acumular timeouts.
3. **Timeout** — 5 segundos por intento individual.

Esto no es decorativo: bajar la base de datos localmente y pegarle a `/api/menu` produce, en los logs, los reintentos con su backoff y luego un 500 limpio devuelto por el middleware — no una excepción sin manejar ni un cuelgue del proceso.

## El flujo del QR (la pieza central)

1. Al crear un evento (`CreateEventCommand`), se genera primero el `Id` del evento y **después** se emite el token: `IQrTokenService.IssueToken(eventId)` firma `eventId + nonce aleatorio` con HMAC-SHA256. El token no es reversible — no expone el id del evento, y la única forma de resolverlo es buscándolo en la base de datos.
2. `Event.Schedule(...)` calcula la ventana de validez del QR: **2 horas antes** del evento hasta **6 horas después** — suficiente para montaje y encores, pero no para que alguien reviva el QR días después.
3. Cuando un invitado escanea el QR y envía una canción (`CreateSongRequestCommand`), el handler valida en este orden: (a) el evento existe, (b) `event.IsQrWindowOpen(now)`, (c) el *fingerprint* del dispositivo no superó el límite de tasa (5 solicitudes / 10 minutos), y solo entonces crea la solicitud y notifica por SignalR.
4. El *fingerprint* (`SongRequestsEndpoints.ComputeFingerprint`) es un hash SHA-256 de IP + User-Agent — identifica "este teléfono", no a la persona.

## Tiempo real

`SongRequestHub` (SignalR) agrupa las conexiones por evento (`event:{eventId}`). El panel admin se une al grupo del evento que está viendo; los invitados **nunca** se conectan al hub — solo hacen `POST` HTTP. Cuando se crea o actualiza una solicitud, `SignalRSongRequestNotifier` (que implementa `ISongRequestNotifier`, el puerto que define Application) transmite el evento a ese grupo.

## Autenticación del panel admin

v1 usa una única API key compartida (`ApiKeyAuthenticationHandler`, header `X-Api-Key`), comparada en tiempo constante. Es una decisión deliberada: el DJ es el único administrador. **Antes de agregar un segundo admin**, esto debe migrarse a JWT + una tabla de usuarios (o un proveedor como Entra ID/Auth0) — la interfaz de autenticación ya está aislada en un solo archivo (`Security/ApiKeyAuthenticationHandler.cs`) para que ese cambio no toque el resto de la API.

## Por qué Dapper (y no EF Core)

El modelo de datos de este dominio es deliberadamente simple (seis tablas, relaciones poco profundas) y el valor está en controlar exactamente el SQL que corre en el flujo de mayor tráfico (crear una solicitud de canción durante un evento en vivo). Dapper da ese control sin el overhead de un `DbContext` y su *change tracking*. El costo que normalmente se paga por eso — migraciones — se cubre con **DbUp** (`DjMrkos.Migrator`), que aplica scripts SQL numerados una sola vez cada uno. Ver [DATABASE.md](DATABASE.md).

## Por qué Minimal API (y no controladores)

Los endpoints están organizados por feature en `Api/Endpoints/*.cs`, cada uno como un método de extensión `MapXEndpoints`. Cada endpoint es una función de una línea que arma el comando/query y lo manda a MediatR — la lógica real vive en Application, nunca en el endpoint.
