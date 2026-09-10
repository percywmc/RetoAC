# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj principal y sus dependencias de proyecto
COPY ["src/Control/Control.Api/Control.Api.csproj", "src/Control/Control.Api/"]
COPY ["src/Control/Control.Application/Control.Application.csproj", "src/Control/Control.Application/"]
COPY ["src/Control/Control.Domain/Control.Domain.csproj", "src/Control/Control.Domain/"]
COPY ["src/Control/Control.Infrastructure/Control.Infrastructure.csproj", "src/Control/Control.Infrastructure/"]

RUN dotnet restore "src/Control/Control.Api/Control.Api.csproj"

COPY . .
WORKDIR "/src/src/Control/Control.Api"
RUN dotnet publish "Control.Api.csproj" -c Release -o /app/publish --no-restore

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 – Runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --no-create-home appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Control.Api.dll"]
