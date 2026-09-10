# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj principal y sus dependencias de proyecto
COPY ["src/CargaMasiva/CargaMasiva.Worker/CargaMasiva.Worker.csproj", "src/CargaMasiva/CargaMasiva.Worker/"]
COPY ["src/CargaMasiva/CargaMasiva.Application/CargaMasiva.Application.csproj", "src/CargaMasiva/CargaMasiva.Application/"]
COPY ["src/CargaMasiva/CargaMasiva.Domain/CargaMasiva.Domain.csproj", "src/CargaMasiva/CargaMasiva.Domain/"]
COPY ["src/CargaMasiva/CargaMasiva.Infrastructure/CargaMasiva.Infrastructure.csproj", "src/CargaMasiva/CargaMasiva.Infrastructure/"]

RUN dotnet restore "src/CargaMasiva/CargaMasiva.Worker/CargaMasiva.Worker.csproj"

COPY . .
WORKDIR "/src/src/CargaMasiva/CargaMasiva.Worker"
RUN dotnet publish "CargaMasiva.Worker.csproj" -c Release -o /app/publish --no-restore

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 – Runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --no-create-home appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CargaMasiva.Worker.dll"]
