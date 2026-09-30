# Despliegue en el VPS compartido con Lawyer

DJ MrKos vive en `~/Mrkos`, separado de `~/Lawyer`. No levanta SQL Server ni publica puertos: se une a la red `lawyer_default`, usa `lawyer-sqlserver-1` con su propia BD y login, y `lawyer-nginx` (dueño de 80/443) le hace el HTTPS con **el certificado de DjMrkos**, que vive en `~/Mrkos/certbot/`. Ningún volumen de Lawyer se toca.

| Contenedor | Qué hace |
|---|---|
| `mrkos-migrator` | Aplica los scripts SQL pendientes y termina |
| `mrkos-api` | API .NET (puerto 8080, solo en la red interna) |
| `mrkos-web` | Caddy en HTTP: sirve el frontend y hace proxy de `/api` y `/hubs` a la API |
| `mrkos-certbot` | Renueva el certificado de DjMrkos cada 12 h |

```
~/Mrkos/
  docker-compose.yml   copiado del repo por deploy.sh
  .env                 secretos (lo crea deploy.sh la primera vez)
  nginx/mrkos.conf     config que lee lawyer-nginx (la genera deploy.sh)
  certbot/conf, www    certificados y desafío de Let's Encrypt de DjMrkos
  DjMrkos/             el repo
```

Lo único que se agrega en Lawyer son 3 montajes de solo lectura en `docker-compose.override.yml`:

```yaml
  nginx:
    volumes:
      - /home/ubuntu/Mrkos/nginx/mrkos.conf:/etc/nginx/conf.d/mrkos.conf:ro
      - /home/ubuntu/Mrkos/certbot/conf:/etc/letsencrypt-mrkos:ro
      - /home/ubuntu/Mrkos/certbot/www:/var/www/certbot-mrkos:ro
```

Y en el crontab del usuario, una recarga diaria de `lawyer-nginx` para que tome certificados renovados (de ambos sitios).

## Primera vez

1. **Base de datos:** ejecuta `deploy/sql/00_CreateDatabaseAndLogin.sql` como `sa` (una sola vez). Las tablas las crea `mrkos-migrator`.
2. **Dominio:** su registro A debe apuntar a la IP del VPS.
3. **Deploy:**
   ```bash
   git clone https://github.com/jfrodriguezva/DjMrkos.git ~/Mrkos/DjMrkos
   bash ~/Mrkos/DjMrkos/deploy/vps-compartido/deploy.sh
   ```
   La primera corrida crea `~/Mrkos/.env` con las claves ya generadas y se detiene. Llena `DOMAIN` y `DB_PASSWORD` y vuelve a correrlo.
4. **Nginx de Lawyer (una sola vez):** el script te pide agregar los 3 montajes de arriba en `~/Lawyer/docker-compose.override.yml` y recrear solo nginx. Vuelve a correr el script: pide el certificado y activa HTTPS.

Cada cambio de nginx pasa por `nginx -t` antes de recargar; si falla, se restaura la config anterior y Lawyer sigue igual.

## Actualizar

```bash
bash ~/Mrkos/DjMrkos/deploy/vps-compartido/deploy.sh
```

Hace `git pull`, reconstruye, aplica los scripts SQL nuevos y deja nginx como estaba.

## Útil

```bash
cd ~/Mrkos
docker compose ps
docker compose logs -f mrkos-api
docker compose logs mrkos-migrator
```
