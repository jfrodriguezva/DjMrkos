# Despliegue en un VPS

Todo el stack corre en contenedores: SQL Server, el migrador de esquema (`DjMrkos.Migrator`), la API (`DjMrkos.Api`) y Caddy sirviendo el frontend ya compilado + reverse proxy hacia la API con HTTPS automático (Let's Encrypt). No se necesita Nginx, certbot, ni IIS por separado.

## 1. Prepara el VPS

- Un VPS con Ubuntu 22.04+ (o cualquier distro con Docker), mínimo 2 vCPU / 4 GB RAM (SQL Server es el que más pide).
- Un dominio con su registro **A** apuntando a la IP pública del VPS — Caddy necesita esto para poder emitir el certificado con Let's Encrypt.
- Puertos **80** y **443** abiertos en el firewall del VPS (`ufw allow 80,443/tcp` o el equivalente de tu proveedor).

Instala Docker Engine + el plugin de Compose (v2, el comando es `docker compose`, no `docker-compose`):

```bash
curl -fsSL https://get.docker.com | sh
```

## 2. Trae el código y configura los secretos

```bash
git clone <tu-repo> djmrkos && cd djmrkos
cp deploy/.env.example deploy/.env
```

Edita `deploy/.env` — `DOMAIN`, una contraseña fuerte para `MSSQL_SA_PASSWORD`, y genera los dos secretos con `openssl rand -base64 48`:

```bash
openssl rand -base64 48   # → QR_SIGNING_KEY
openssl rand -base64 48   # → ADMIN_API_KEY
```

`deploy/.env` nunca se sube al repo (ya está en `.gitignore`).

## 3. Levanta el stack

```bash
docker compose -f deploy/docker-compose.prod.yml --env-file deploy/.env up -d --build
```

Orden de arranque (automático, vía `depends_on` con `condition`):

1. `sqlserver` — espera a pasar su healthcheck.
2. `migrator` — aplica los scripts de `backend/src/DjMrkos.Migrator/Scripts/` una sola vez y termina (`restart: "no"`); DbUp lleva su propio registro, así que en despliegues siguientes es un no-op si no hay scripts nuevos.
3. `api` — arranca una vez que `migrator` terminó con éxito.
4. `web` (Caddy) — sirve el build de React y le hace reverse proxy a `api:8080` en `/api/*` y `/hubs/*` bajo el mismo dominio (por eso no hace falta configurar CORS ni una URL base en el frontend — ver `docs/ARCHITECTURE.md`).

La primera vez, Caddy tarda unos segundos en obtener el certificado de Let's Encrypt la primera vez que alguien visita `https://tudominio.com`.

## 4. Verifica

- `https://tudominio.com` — el portal público.
- `https://tudominio.com/admin/login` — con el `ADMIN_API_KEY` que pusiste en `.env`.
- `docker compose -f deploy/docker-compose.prod.yml logs -f api` — logs de la API si algo no arranca.

## 5. Actualizar a una versión nueva

```bash
git pull
docker compose -f deploy/docker-compose.prod.yml --env-file deploy/.env up -d --build
```

Vuelve a correr `migrator` automáticamente; solo aplica los scripts SQL que agregaste desde el último deploy.

## 6. Respaldo de la base de datos

El volumen `djmrkos-sqlserver-data` es la única fuente de verdad — no hay backups automáticos configurados. Como mínimo, agrega un cron en el VPS que corra `sqlcmd`/`BACKUP DATABASE` dentro del contenedor `sqlserver` y copie el `.bak` fuera del VPS (a S3, Backblaze, etc.). Sin esto, perder el VPS es perder los datos.

## Notas

- El puerto 1433 de SQL Server **no** se publica al host ni a internet — solo es alcanzable desde `api`/`migrator` dentro de la red interna de Docker que crea Compose.
- Swagger (`/swagger`) solo se expone cuando `ASPNETCORE_ENVIRONMENT=Development` — en producción (`Production`, el valor que pone este compose) queda apagado por defecto.
- `Telegram__BotToken`/`Telegram__ChatId` son opcionales — ver la sección de alertas al DJ en el `README.md` de la raíz para configurarlos.
- Si el dominio lo administras en Zoho y también quieres correo (`hola@tudominio.com`) ahí mismo, ver [ZOHO_MAIL_SETUP.md](ZOHO_MAIL_SETUP.md) — el registro A de este despliegue y los MX de correo conviven sin conflicto.
- Si Zoho solo te ofrece planes de pago (les pasó a los mantenedores de este repo) o prefieres no pagar nada, ver [EMAIL_GRATIS_CLOUDFLARE.md](EMAIL_GRATIS_CLOUDFLARE.md) — recibir/enviar como `hola@tudominio.com` gratis, vía Cloudflare Email Routing + Gmail.
