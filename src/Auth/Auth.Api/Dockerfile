# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj principal y sus dependencias de proyecto
COPY ["src/Auth/Auth.Api/Auth.Api.csproj", "src/Auth/Auth.Api/"]
COPY ["src/Auth/Auth.Application/Auth.Application.csproj", "src/Auth/Auth.Application/"]
COPY ["src/Auth/Auth.Domain/Auth.Domain.csproj", "src/Auth/Auth.Domain/"]
COPY ["src/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj", "src/Auth/Auth.Infrastructure/"]

RUN dotnet restore "src/Auth/Auth.Api/Auth.Api.csproj"

COPY . .
WORKDIR "/src/src/Auth/Auth.Api"
RUN dotnet publish "Auth.Api.csproj" -c Release -o /app/publish --no-restore

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 – Runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --no-create-home appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Auth.Api.dll"]
