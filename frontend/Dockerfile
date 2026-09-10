# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ─────────────────────────────────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci --prefer-offline

COPY . .

# Variables de entorno de build — sobreescribir al hacer docker build
ARG VITE_API_BASE_URL=http://localhost:5000
ARG VITE_POLLING_INTERVAL_MS=6000
ARG VITE_MAX_FILE_SIZE_MB=10

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_POLLING_INTERVAL_MS=$VITE_POLLING_INTERVAL_MS
ENV VITE_MAX_FILE_SIZE_MB=$VITE_MAX_FILE_SIZE_MB

RUN npm run build

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 – Serve con Nginx
# ─────────────────────────────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS runtime

# Configuración de Nginx para SPA (React Router — todas las rutas -> index.html)
RUN printf 'server {\n\
    listen 80;\n\
    root /usr/share/nginx/html;\n\
    index index.html;\n\
    location / {\n\
        try_files $uri $uri/ /index.html;\n\
    }\n\
    gzip on;\n\
    gzip_types text/plain text/css application/json application/javascript;\n\
}\n' > /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
