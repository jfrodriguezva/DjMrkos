# Despliegue en el VPS compartido con Lawyer

DJ MrKos vive en `~/Mrkos`, separado de `~/Lawyer`. No levanta SQL Server ni publica puertos: se une a la red `lawyer_default`, usa `lawyer-sqlserver-1` con su propio login y `lawyer-nginx` le da el HTTPS con el certbot de Lawyer.

| Contenedor | Qué hace |
|---|---|
| `mrkos-migrator` | Aplica los scripts SQL pendientes y termina |
| `mrkos-api` | API .NET (puerto 8080, solo en la red interna) |
| `mrkos-web` | Caddy en HTTP: sirve el frontend y hace proxy de `/api` y `/hubs` a la API |

```
~/Mrkos/
  docker-compose.yml   copiado del repo por deploy.sh
  .env                 secretos (lo crea deploy.sh la primera vez)
  nginx/mrkos.conf     config que lee lawyer-nginx (la genera deploy.sh)
  DjMrkos/             el repo
```

## Primera vez

1. **Base de datos:** ejecuta `deploy/sql/00_CreateDatabaseAndLogin.sql` como `sa` (una sola vez). Las tablas las crea `mrkos-migrator`.
2. **Dominio:** su registro A debe apuntar a la IP del VPS.
3. **Deploy:**
   ```bash
   git clone https://github.com/jfrodriguezva/DjMrkos.git ~/Mrkos/DjMrkos
   bash ~/Mrkos/DjMrkos/deploy/vps-compartido/deploy.sh
   ```
   La primera corrida crea `~/Mrkos/.env` con las claves ya generadas y se detiene. Llena `DOMAIN` y `DB_PASSWORD` y vuelve a correrlo.
4. **Nginx de Lawyer (una sola vez):** el script te pide agregar una línea en `~/Lawyer/docker-compose.override.yml` para que `lawyer-nginx` lea `~/Mrkos/nginx/mrkos.conf`, y recrear solo nginx. Vuelve a correr el script: pide el certificado y activa HTTPS.

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
