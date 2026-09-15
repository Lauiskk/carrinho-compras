# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# build: instala dependências exatamente como no package-lock e gera os estáticos.
# ---------------------------------------------------------------------------
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npm run build

# ---------------------------------------------------------------------------
# web: nginx sem root servindo a SPA e repassando /api para o container da API.
# ---------------------------------------------------------------------------
FROM nginxinc/nginx-unprivileged:1.30-alpine AS web
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
