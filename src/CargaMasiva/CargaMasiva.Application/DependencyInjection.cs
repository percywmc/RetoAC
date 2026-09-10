using CargaMasiva.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CargaMasiva.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProcesarCargaService>();

        return services;
    }
}
