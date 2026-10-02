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

## Promociones

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/admin/promotions` | Todas las promociones, activas e inactivas. |
| POST | `/api/admin/promotions` | Crea una promoción. Body: `{ moduleId?, categoryId?, label, discountPercentage }` — exactamente uno de `moduleId`/`categoryId`, nunca ambos ni ninguno. |
| PUT | `/api/admin/promotions/{id}` | Actualiza nombre, descuento y estado activo. |
| DELETE | `/api/admin/promotions/{id}` | Desactiva la promoción (soft delete). |

Una promoción activa se refleja automáticamente en `GET /api/menu`: cada categoría con `price` bajo el módulo o la categoría promocionada trae `price` ya descontado, más `originalPrice`, `discountPercentage` y `promotionLabel` para que el frontend muestre el precio tachado y el badge. Una promoción de categoría tiene prioridad sobre una de módulo para esa misma categoría.

## Eventos y QR

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/events/qr/{token}` | Público — lo llama la página QR al abrirse. Devuelve `{ id, clientName, eventDateUtc, isRequestWindowOpen }`. `404` si el token no existe, la ventana cerrada se refleja en `isRequestWindowOpen: false` (no en el código de estado). |
| GET | `/api/admin/events` | Próximos eventos (no cancelados), ordenados por fecha. |
| POST | `/api/admin/events` | Crea un evento y emite su token de QR. Responde `{ event, qrCodeDataUrl }` — el PNG del QR ya listo para mostrar/imprimir. |
| GET | `/api/availability` | Público. Devuelve `string[]` de fechas (`YYYY-MM-DD`) con al menos un evento no cancelado **o bloqueadas a mano** — nunca nombres de cliente ni el motivo del bloqueo. Alimenta el calendario de `/agendar` y el aviso de `/contratar`. |

## Calendario (fechas bloqueadas)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/admin/blocked-dates` | Fechas bloqueadas de ayer en adelante, con su motivo. |
| POST | `/api/admin/blocked-dates` | Body: `{ from, to?, reason? }`. Bloquea un día o un rango (máximo 90 días); los días ya bloqueados se saltan. Responde solo los días nuevos. |
| DELETE | `/api/admin/blocked-dates/{id}` | Desbloquea un día. |

## Galería

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/gallery/albums` | Público. Álbumes publicados que tienen al menos una foto, del evento más reciente al más antiguo. |
| GET | `/api/gallery/albums/{slug}` | Público. `{ album, images }`. `404` si no existe **o está en borrador**. |
| GET | `/api/gallery/images/{id}/{large\|thumb}` | Público. El JPEG, con caché de un año (`immutable`): el contenido de un id nunca cambia. |
| GET | `/api/admin/gallery/albums` | Todos los álbumes, borradores incluidos. |
| GET | `/api/admin/gallery/albums/{id}` | Un álbum con sus fotos, aunque sea borrador. |
| POST | `/api/admin/gallery/albums` | Body: `{ title, eventDate?, description? }`. Nace como borrador; el `slug` (URL pública) sale del título y no cambia aunque se edite el título. |
| PUT | `/api/admin/gallery/albums/{id}` | Body: `{ title, eventDate?, description?, isPublished }`. |
| PUT | `/api/admin/gallery/albums/{id}/cover` | Body: `{ imageId }` (una foto de ese álbum) o `{ imageId: null }` para volver a "la primera foto". |
| DELETE | `/api/admin/gallery/albums/{id}` | Borra el álbum, sus fotos y sus archivos. |
| POST | `/api/admin/gallery/albums/{id}/images` | `multipart/form-data`: `image` y `thumbnail` (JPEG, ya redimensionados por el navegador del admin — `frontend/src/lib/gallery.ts`), `width`, `height`, `caption?`. Una foto por petición. Se valida que los bytes sean JPEG de verdad, no el `Content-Type` declarado. |
| DELETE | `/api/admin/gallery/images/{id}` | Borra la foto (y la quita como portada si lo era). |

Los archivos viven fuera de la base, en `Gallery:StoragePath` (`App_Data/gallery` en desarrollo; el volumen `mrkos_gallery` montado en `/app/data/gallery` en el VPS).

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
| POST | `/api/admin/leads/{id}/confirm` | Confirma la cita: crea un `Event` (el mismo que usa el flujo de QR) con la fecha/hora y lugar dados, y marca el lead como `Won`. Responde `{ lead, event }`. El evento creado aparece de inmediato en `GET /api/admin/events` — es el calendario interno. |

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
