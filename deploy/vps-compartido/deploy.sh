#!/usr/bin/env bash
# Deploys (or updates) DJ MrKos on the VPS it shares with Lawyer. Safe to re-run: every
# step checks what is already in place and only does what is missing.
#
#   bash ~/Mrkos/DjMrkos/deploy/vps-compartido/deploy.sh
#
# What it does:
#   1. Clones or pulls the repo into ~/Mrkos/DjMrkos and copies docker-compose.yml to ~/Mrkos.
#   2. Creates ~/Mrkos/.env on the first run (with the two secrets already generated) and
#      stops so you can fill DOMAIN and DB_PASSWORD.
#   3. Builds and starts mrkos-migrator -> mrkos-api -> mrkos-web on Lawyer's network.
#   4. Plugs the domain into lawyer-nginx: HTTP first, then the Let's Encrypt certificate
#      through Lawyer's certbot, then HTTPS. Every nginx change is checked with `nginx -t`
#      and rolled back if it fails, so a bad config can never take Lawyer down.
#
# Everything lives inside main(), so bash parses the whole script before running it — the
# `git pull` in step 1 may rewrite this very file mid-run.

set -euo pipefail

main() {
    local MRKOS_DIR="${MRKOS_DIR:-$HOME/Mrkos}"
    local LAWYER_DIR="${LAWYER_DIR:-$HOME/Lawyer}"
    local REPO_URL="https://github.com/jfrodriguezva/DjMrkos.git"
    local NGINX="lawyer-nginx-1"

    local SRC="$MRKOS_DIR/DjMrkos"
    local TPL="$SRC/deploy/vps-compartido"
    local ENV_FILE="$MRKOS_DIR/.env"
    local NGINX_CONF="$MRKOS_DIR/nginx/mrkos.conf"

    # ---------------------------------------------------------------- 1. code
    step "Código en $SRC"
    mkdir -p "$MRKOS_DIR/nginx"
    if [ -d "$SRC/.git" ]; then
        git -C "$SRC" pull --ff-only
    else
        git clone "$REPO_URL" "$SRC"
    fi
    cp "$TPL/docker-compose.yml" "$MRKOS_DIR/docker-compose.yml"

    # ---------------------------------------------------------------- 2. .env
    if [ ! -f "$ENV_FILE" ]; then
        step "Creando $ENV_FILE"
        cp "$TPL/.env.example" "$ENV_FILE"
        chmod 600 "$ENV_FILE"
        sed -i "s|^QR_SIGNING_KEY=.*|QR_SIGNING_KEY=$(openssl rand -base64 48 | tr -d '\n')|" "$ENV_FILE"
        sed -i "s|^ADMIN_API_KEY=.*|ADMIN_API_KEY=$(openssl rand -base64 48 | tr -d '\n')|" "$ENV_FILE"
        cat <<EOF

Ya generé QR_SIGNING_KEY y ADMIN_API_KEY. Ahora edita el archivo:

    nano $ENV_FILE

y llena DOMAIN y DB_PASSWORD (revisa DB_USER). Luego vuelve a correr este script.
EOF
        exit 0
    fi

    local DOMAIN DB_PASSWORD LAWYER_NETWORK
    DOMAIN="$(env_get "$ENV_FILE" DOMAIN)"
    DB_PASSWORD="$(env_get "$ENV_FILE" DB_PASSWORD)"
    LAWYER_NETWORK="$(env_get "$ENV_FILE" LAWYER_NETWORK)"
    [ -n "$DOMAIN" ]      || fail "Falta DOMAIN en $ENV_FILE"
    [ -n "$DB_PASSWORD" ] || fail "Falta DB_PASSWORD en $ENV_FILE"

    # ---------------------------------------------------------------- pre-checks
    step "Revisando el entorno"
    docker network inspect "$LAWYER_NETWORK" >/dev/null 2>&1 \
        || fail "No existe la red '$LAWYER_NETWORK'. Revisa LAWYER_NETWORK en $ENV_FILE (docker network ls)."
    docker inspect "$NGINX" >/dev/null 2>&1 \
        || fail "No encuentro el contenedor $NGINX. ¿Está levantado Lawyer?"

    local public_ip domain_ip
    public_ip="$(curl -4 -fsS --max-time 5 https://api.ipify.org || true)"
    domain_ip="$(getent ahostsv4 "$DOMAIN" | awk 'NR==1{print $1}' || true)"
    if [ -z "$domain_ip" ]; then
        warn "$DOMAIN todavía no resuelve a ninguna IP; el certificado va a fallar hasta que el registro A exista."
    elif [ -n "$public_ip" ] && [ "$domain_ip" != "$public_ip" ]; then
        warn "$DOMAIN apunta a $domain_ip pero este VPS es $public_ip; el certificado va a fallar."
    else
        echo "$DOMAIN -> $domain_ip ✓"
    fi

    # ---------------------------------------------------------------- 3. containers
    step "Construyendo y levantando mrkos-migrator, mrkos-api y mrkos-web"
    if ! (cd "$MRKOS_DIR" && docker compose up -d --build); then
        echo
        (cd "$MRKOS_DIR" && docker compose logs --tail 40 mrkos-migrator mrkos-api) || true
        fail "Algo no levantó; arriba están los últimos logs del migrador y la API."
    fi
    (cd "$MRKOS_DIR" && docker compose logs mrkos-migrator | tail -3)

    # ---------------------------------------------------------------- 4. nginx + https
    step "Conectando $DOMAIN al nginx de Lawyer"
    if [ ! -f "$NGINX_CONF" ]; then
        render "$TPL/nginx-http.conf.template" "$DOMAIN" > "$NGINX_CONF"
    fi

    if ! docker exec "$NGINX" test -f /etc/nginx/conf.d/mrkos.conf; then
        cat <<EOF

Falta un paso único en Lawyer: que su nginx lea $NGINX_CONF.
Agrega esto en $LAWYER_DIR/docker-compose.override.yml (junto a lo que ya tenga):

services:
  nginx:
    volumes:
      - $NGINX_CONF:/etc/nginx/conf.d/mrkos.conf:ro

y recrea solo nginx (unos segundos sin servicio):

    cd $LAWYER_DIR && docker compose up -d nginx

Luego vuelve a correr este script.
EOF
        exit 1
    fi

    if ! docker exec "$NGINX" test -f "/etc/letsencrypt/live/$DOMAIN/fullchain.pem"; then
        apply_nginx "$NGINX" "$NGINX_CONF" "$TPL/nginx-http.conf.template" "$DOMAIN"

        step "Pidiendo el certificado de $DOMAIN con el certbot de Lawyer"
        (cd "$LAWYER_DIR" && docker compose run --rm --entrypoint certbot certbot \
            certonly --webroot -w /var/www/certbot -d "$DOMAIN" \
            --non-interactive --agree-tos --register-unsafely-without-email --keep-until-expiring) \
            || fail "Certbot no pudo emitir el certificado. El sitio queda en http://$DOMAIN mientras tanto."
    fi

    apply_nginx "$NGINX" "$NGINX_CONF" "$TPL/nginx-https.conf.template" "$DOMAIN"

    # ---------------------------------------------------------------- verify
    step "Verificando"
    docker exec mrkos-api curl -fsS http://localhost:8080/health >/dev/null && echo "API sana ✓"
    local code
    code="$(curl -s -o /dev/null -w '%{http_code}' --max-time 10 "https://$DOMAIN/" || true)"
    echo "https://$DOMAIN/ -> HTTP $code"

    cat <<EOF

Listo: https://$DOMAIN
Panel del DJ: https://$DOMAIN/admin/login  (la clave es ADMIN_API_KEY en $ENV_FILE)
EOF
}

step() { printf '\n\033[1;36m==> %s\033[0m\n' "$*"; }
warn() { printf '\033[1;33mAVISO: %s\033[0m\n' "$*"; }
fail() { printf '\n\033[1;31mERROR: %s\033[0m\n' "$*" >&2; exit 1; }

# Reads one KEY=value from the .env without sourcing it, so passwords with symbols are safe.
env_get() { { grep -E "^$2=" "$1" || true; } | tail -1 | cut -d= -f2- | tr -d '\r'; }

render() { sed "s/__DOMAIN__/$2/g" "$1"; }

# Writes the nginx config in place (same inode, so the single-file bind mount sees it),
# validates it inside lawyer-nginx and reloads; on a failed `nginx -t` restores the old one.
apply_nginx() {
    local nginx="$1" conf="$2" template="$3" domain="$4" previous
    previous="$(cat "$conf")"
    render "$template" "$domain" > "$conf"
    if ! docker exec "$nginx" nginx -t; then
        printf '%s\n' "$previous" > "$conf"
        fail "La nueva config de nginx no pasó 'nginx -t'; dejé la anterior. Lawyer no se tocó."
    fi
    docker exec "$nginx" nginx -s reload
}

main "$@"
