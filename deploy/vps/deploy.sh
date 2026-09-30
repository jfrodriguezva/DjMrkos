#!/usr/bin/env bash
# Deploys (or updates) DJ MrKos on the shared VPS. Safe to re-run.
#
#   bash ~/Mrkos/DjMrkos/deploy/vps/deploy.sh
#
#   1. Pulls the repo in ~/Mrkos/DjMrkos and copies docker-compose.yml to ~/Mrkos.
#   2. First run only: creates ~/Mrkos/.env with the two secrets generated and stops so
#      you can fill DB_PASSWORD.
#   3. Builds and starts mrkos-migrator -> mrkos-api -> mrkos-web.
#
# It never touches ~/infra or any other app: HTTPS and SQL Server come from ~/infra.
#
# Everything lives inside main(), so bash parses the whole script before running it — the
# `git pull` in step 1 may rewrite this very file mid-run.
set -euo pipefail

main() {
    local MRKOS_DIR="${MRKOS_DIR:-$HOME/Mrkos}"
    local SRC="$MRKOS_DIR/DjMrkos"
    local TPL="$SRC/deploy/vps"
    local ENV_FILE="$MRKOS_DIR/.env"

    step "Code in $SRC"
    git -C "$SRC" pull --ff-only
    cp "$TPL/docker-compose.yml" "$MRKOS_DIR/docker-compose.yml"

    if [ ! -f "$ENV_FILE" ]; then
        step "Creating $ENV_FILE"
        cp "$TPL/.env.example" "$ENV_FILE"
        chmod 600 "$ENV_FILE"
        sed -i "s|^QR_SIGNING_KEY=.*|QR_SIGNING_KEY=$(openssl rand -base64 48 | tr -d '\n')|" "$ENV_FILE"
        sed -i "s|^ADMIN_API_KEY=.*|ADMIN_API_KEY=$(openssl rand -base64 48 | tr -d '\n')|" "$ENV_FILE"
        printf '\nQR_SIGNING_KEY and ADMIN_API_KEY generated. Fill DB_PASSWORD (and check DOMAIN):\n\n    nano %s\n\nthen run this script again.\n' "$ENV_FILE"
        exit 0
    fi

    local key
    for key in DOMAIN DB_USER DB_PASSWORD QR_SIGNING_KEY ADMIN_API_KEY; do
        [ -n "$(env_get "$ENV_FILE" "$key")" ] || fail "$key is empty in $ENV_FILE"
    done

    step "Checking ~/infra"
    docker network inspect edge >/dev/null 2>&1 && docker network inspect data >/dev/null 2>&1 \
        || fail "The edge/data networks don't exist: bring ~/infra up first."
    [ "$(docker inspect -f '{{.State.Health.Status}}' infra-sqlserver 2>/dev/null)" = "healthy" ] \
        || fail "infra-sqlserver is not running/healthy."

    step "Building and starting mrkos-migrator, mrkos-api, mrkos-web"
    if ! (cd "$MRKOS_DIR" && docker compose up -d --build --remove-orphans); then
        (cd "$MRKOS_DIR" && docker compose logs --tail 40 mrkos-migrator mrkos-api) || true
        fail "Something did not start; the migrator/API logs are above."
    fi
    (cd "$MRKOS_DIR" && docker compose logs mrkos-migrator | tail -2)
    docker exec mrkos-api curl -fsS http://localhost:8080/health >/dev/null && echo "API healthy ✓"

    local domain; domain="$(env_get "$ENV_FILE" DOMAIN)"
    printf '\nDone: https://%s   (admin: https://%s/admin/login, key = ADMIN_API_KEY in %s)\n' "$domain" "$domain" "$ENV_FILE"
}

step() { printf '\n\033[1;36m==> %s\033[0m\n' "$*"; }
fail() { printf '\n\033[1;31mERROR: %s\033[0m\n' "$*" >&2; exit 1; }

# Reads one KEY=value from the .env without sourcing it, so passwords with symbols are safe.
env_get() { { grep -E "^$2=" "$1" || true; } | tail -1 | cut -d= -f2- | tr -d '\r'; }

main "$@"
