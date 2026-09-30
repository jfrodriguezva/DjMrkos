# Despliegue en el VPS compartido

DJ MrKos es una app más del VPS: su carpeta, su compose y su `.env`. SQL Server y el HTTPS son infraestructura compartida en `~/infra` ([jfrodriguezva/vps-infra](https://github.com/jfrodriguezva/vps-infra)).

```
/home/ubuntu/
├── infra/     SQL Server + Caddy (80/443)
├── Lawyer/    otra app
└── Mrkos/
    ├── docker-compose.yml   copiado del repo por deploy.sh
    ├── .env                 secretos de DJ MrKos
    └── DjMrkos/             este repo
```

| Contenedor | Redes | Qué hace |
|---|---|---|
| `mrkos-migrator` | `data` | Aplica los scripts SQL pendientes y termina |
| `mrkos-api` | `mrkos_default`, `data` | API .NET (8080). No está en `edge`: solo `mrkos-web` la ve |
| `mrkos-web` | `mrkos_default`, `edge` | Sirve el frontend y reparte `/api` y `/hubs` a la API |

`infra-caddy` publica `https://djmrkos.com` → `mrkos-web:80` (`~/infra/caddy/sites/djmrkos.caddy`).

## Primera vez

1. `~/infra` levantado, y el registro A de `djmrkos.com` (y `www`) → IP del VPS en Cloudflare, **DNS only**.
2. BD y login (si no existen): `bash ~/infra/scripts/new-app-db.sh djmrkos mrkos`.
3. Deploy:
   ```bash
   git clone https://github.com/jfrodriguezva/DjMrkos.git ~/Mrkos/DjMrkos   # si no está
   bash ~/Mrkos/DjMrkos/deploy/vps/deploy.sh    # 1a vez: crea ~/Mrkos/.env y se detiene
   nano ~/Mrkos/.env                            # DB_PASSWORD
   bash ~/Mrkos/DjMrkos/deploy/vps/deploy.sh
   ```

## Actualizar

```bash
bash ~/Mrkos/DjMrkos/deploy/vps/deploy.sh
```

## Útil

```bash
cd ~/Mrkos
docker compose ps
docker compose logs -f mrkos-api
docker compose logs mrkos-migrator
```
