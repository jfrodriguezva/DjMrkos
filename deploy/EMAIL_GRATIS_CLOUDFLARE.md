# Correo gratis para djmrkos.com sin pagar Zoho (Cloudflare Email Routing + Gmail)

Alternativa cuando Zoho Mail solo ofrece planes de pago (ver [ZOHO_MAIL_SETUP.md](ZOHO_MAIL_SETUP.md)) o no quieres dar tarjeta. Esto es $0, sin límite de alias, sin tarjeta — la contra es que no tienes un webmail propio: todo vive dentro de tu Gmail normal.

## Qué logra

- Recibir en tu Gmail cualquier correo mandado a `hola@djmrkos.com`.
- Responder/enviar correos que se ven "de" `hola@djmrkos.com`, sin salir de Gmail.

## Requisitos

- `djmrkos.com` ya está en Cloudflare (confirmado).
- Una cuenta Gmail donde quieras que llegue todo.

## Parte 1 — Activar Cloudflare Email Routing

1. [dash.cloudflare.com](https://dash.cloudflare.com) → `djmrkos.com` → menú lateral **Email** → **Email Routing**.
2. **Get started** / **Enable Email Routing**.
3. Cloudflare te ofrece agregar sus propios registros MX/TXT automáticamente — acepta ("Add records and enable"), no hay que copiarlos a mano.
   - **Si ya tenías puestos los MX de Zoho** (`mx.zoho.com`, `mx2.zoho.com`, `mx3.zoho.com`) de un intento anterior, bórralos primero en **DNS → Records** — solo puede haber un proveedor de MX activo a la vez, y Cloudflare te va a avisar del conflicto si no lo haces.
4. En **Destination addresses**, agrega tu Gmail. Cloudflare te manda un correo de verificación ahí — confírmalo.
5. En **Routing rules** (o **Custom addresses**), crea una regla:
   - Dirección: `hola@djmrkos.com` (o un **Catch-all** si quieres que *cualquier cosa*@djmrkos.com también llegue).
   - Acción: **Send to** → tu Gmail ya verificado.
6. Guarda. Desde ese momento, todo lo que llegue a `hola@djmrkos.com` aparece en tu bandeja de Gmail normal.

## Parte 2 — Enviar "como" hola@djmrkos.com desde Gmail

1. En Gmail → ⚙️ **Configuración** → **Ver toda la configuración** → pestaña **Cuentas y importación**.
2. En "Enviar correo como" → **Agregar otra dirección de correo electrónico**.
3. Pon el nombre a mostrar y `hola@djmrkos.com` → Siguiente.
4. Marca la opción **"Tratar como alias"** — así Gmail no te pide un servidor SMTP propio, solo verifica que el correo es tuyo mandando un código/link a `hola@djmrkos.com`, que te llega a tu Gmail gracias al reenvío de la Parte 1.
5. Confirma ese código. Listo: al redactar un correo nuevo, el campo "De:" ya te deja elegir `hola@djmrkos.com`.

## Límites honestos de esta alternativa

- No hay webmail ni app propia — todo vive dentro de tu cuenta de Gmail de siempre.
- No hay copia "en el dominio" de los correos — quedan donde cualquier correo que recibes en Gmail.
- Si más adelante el proyecto crece y necesitas varias cuentas reales para distintas personas del equipo, ahí sí conviene pagar algo como Zoho Mail Lite (~Mex$22.50/usuario/mes) — pero para un solo `hola@djmrkos.com` de un negocio chico, esto es suficiente y no cuesta nada.
