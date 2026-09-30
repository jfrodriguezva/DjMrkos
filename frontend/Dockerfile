FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Caddy both serves the built SPA and reverse-proxies /api and /hubs to the API container —
# one edge container, automatic HTTPS via Let's Encrypt from just the DOMAIN env var, no
# separate nginx + certbot setup to maintain on the VPS.
FROM caddy:2-alpine
COPY --from=build /app/dist /usr/share/caddy
COPY Caddyfile /etc/caddy/Caddyfile
