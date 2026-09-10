using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Infrastructure.Persistence;

/// <summary>
/// Fábrica usada únicamente por las herramientas de diseño de EF Core (dotnet ef migrations)
/// para poder generar migraciones sin necesitar levantar la API completa.
/// La cadena de conexión real en runtime siempre proviene de configuración (appsettings/env vars).
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=AuthDb;User Id=sa;Password=CambiarEstaClave#2026;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}
