# Referencia de la API

Base URL en desarrollo: `http://localhost:5027`. Swagger interactivo en `/swagger`.

Los endpoints bajo `/api/admin/*` requieren el header `X-Api-Key: <clave>` (ver [ARCHITECTURE.md](ARCHITECTURE.md#autenticación-del-panel-admin)). El resto son públicos.

## Menú configurable

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/menu` | Módulos activos con sus categorías activas, en orden — esto es todo el menú público. |
| GET | `/api/modules/{moduleId}/categories` | Categorías de un módulo (admin y público comparten esta lectura). |
| GET | `/api/admin/modules` | Todos los módulos, incluidos los inactivos. |
| POST | `/api/admin/modules` | Crea un módulo. |
| PUT | `/api/admin/modules/{id}` | Actualiza nombre, ícono, orden y estado activo. |
| DELETE | `/api/admin/modules/{id}` | Desactiva el módulo (soft delete — ver [DATABASE.md](DATABASE.md)). |
| POST | `/api/admin/modules/reorder` | Aplica un nuevo orden completo (`{ orderedModuleIds: string[] }`). |
| POST | `/api/admin/categories` | Crea una categoría bajo un módulo. Body incluye `price` (decimal, opcional — `null` = incluido/a cotizar). |
| PUT | `/api/admin/categories/{id}` | Actualiza una categoría, incluido `price`. |
| DELETE | `/api/admin/categories/{id}` | Desactiva la categoría. |

`price` viaja también en `GET /api/menu` (dentro de cada categoría) — es lo que alimenta el cotizador tipo carrito del frontend sin un endpoint aparte. Ver [FRONTEND.md](FRONTEND.md#cotizador-tipo-carrito).

## Eventos y QR

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/events/qr/{token}` | Público — lo llama la página QR al abrirse. Devuelve `{ id, clientName, eventDateUtc, isRequestWindowOpen }`. `404` si el token no existe, la ventana cerrada se refleja en `isRequestWindowOpen: false` (no en el código de estado). |
| GET | `/api/admin/events` | Próximos eventos (no cancelados), ordenados por fecha. |
| POST | `/api/admin/events` | Crea un evento y emite su token de QR. Responde `{ event, qrCodeDataUrl }` — el PNG del QR ya listo para mostrar/imprimir. |

## Solicitudes de canciones

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/events/qr/{token}/song-requests` | Público. Body: `{ songTitle, artist?, requesterName?, dedication? }`. `410` si el QR no está en su ventana activa, `429` si el dispositivo excedió el límite de tasa. |
| GET | `/api/admin/events/{eventId}/song-requests` | Todas las solicitudes de un evento, más recientes primero. |
| PATCH | `/api/admin/song-requests/{id}/status` | Body: `{ action: 0 \| 1 \| 2 }` → `0` = mover a cola, `1` = marcar tocada, `2` = rechazar. |

Cada creación/actualización también se transmite por SignalR — ver [FRONTEND.md](FRONTEND.md#tiempo-real).

## Testimonios

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/testimonials` | Público — solo los aprobados. |
| POST | `/api/testimonials` | Público. Body: `{ clientName, eventId?, rating (1-5), comment }`. Queda **sin aprobar** hasta revisión. |
| GET | `/api/admin/testimonials` | Todos, incluidos los pendientes. |
| POST | `/api/admin/testimonials/{id}/approve` | Publica el testimonio. |

## Cotizaciones (leads)

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/leads` | Público — el formulario de contacto. Body: `{ name, email, phone?, eventDate?, message }`. |
| GET | `/api/admin/leads` | Listado completo, más recientes primero. |

## Errores

Todas las respuestas de error usan `application/problem+json`:

```json
{ "title": "Escribe el nombre de la canción.", "status": 400, "errors": { "SongTitle": ["..."] } }
```

| Código | Cuándo |
|---|---|
| 400 | Validación (FluentValidation) o una regla de dominio violada. |
| 401 | Falta `X-Api-Key` o es inválida, en una ruta `/api/admin/*`. |
| 404 | El recurso no existe. |
| 410 | El QR existe pero su ventana de solicitudes ya cerró. |
| 429 | Límite de tasa de solicitudes de canción excedido. |
| 500 | Error no controlado (ver logs — el middleware nunca expone el detalle interno). |
