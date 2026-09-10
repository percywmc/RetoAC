# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar sólo el .csproj primero para aprovechar la caché de capas de Docker
COPY ["src/Notificaciones/Notificaciones.Worker/Notificaciones.Worker.csproj", "src/Notificaciones/Notificaciones.Worker/"]

RUN dotnet restore "src/Notificaciones/Notificaciones.Worker/Notificaciones.Worker.csproj"

# Copiar el resto del código fuente
COPY . .

WORKDIR "/src/src/Notificaciones/Notificaciones.Worker"
RUN dotnet publish "Notificaciones.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 – Runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Usuario sin privilegios de root (buenas prácticas de seguridad)
RUN adduser --disabled-password --no-create-home appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Notificaciones.Worker.dll"]
