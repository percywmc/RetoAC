using Auth.Application.Abstractions;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public static class AuthDbSeeder
{
    public static async Task SeedAsync(AuthDbContext dbContext, IPasswordHasher passwordHasher, string adminEmail, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var adminExists = await dbContext.Users.AnyAsync(u => u.Username == "admin", cancellationToken);
        if (adminExists)
        {
            return;
        }

        var admin = new User(
            Guid.NewGuid(),
            username: "admin",
            email: adminEmail,
            passwordHash: passwordHasher.Hash("Admin#2026"),
            role: "CargaMasiva.Ejecutor");

        await dbContext.Users.AddAsync(admin, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
