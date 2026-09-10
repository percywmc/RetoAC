using CargaMasiva.Application.Abstractions;
using CargaMasiva.Infrastructure.Excel;
using CargaMasiva.Infrastructure.ExternalServices;
using CargaMasiva.Infrastructure.Messaging;
using CargaMasiva.Infrastructure.Options;
using CargaMasiva.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace CargaMasiva.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeaweedFsOptions>(configuration.GetSection(SeaweedFsOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<DatabaseOptions>(options =>
        {
            options.ControlDatabase = configuration.GetConnectionString("ControlDatabase")
                ?? throw new InvalidOperationException("La cadena de conexión 'ControlDatabase' es obligatoria.");
        });

        services.AddScoped<IExcelReader, ClosedXmlExcelReader>();
        services.AddScoped<IDataProcesadaRepository, DapperDataProcesadaRepository>();
        services.AddScoped<ICargaFallidaRepository, DapperCargaFallidaRepository>();
        services.AddScoped<IRegistroFallidoRepository, DapperRegistroFallidoRepository>();
        services.AddScoped<ICargaEstadoService, DapperCargaEstadoService>();
        services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();

        services.AddHttpClient<IFileDownloader, SeaweedFsDownloader>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("La configuración 'RabbitMq' es obligatoria.");

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<CargaMasivaConsumer>();

            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, "/", h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.ReceiveEndpoint(rabbitMqOptions.CargaMasivaQueue, e =>
                {
                    e.ConfigureConsumer<CargaMasivaConsumer>(context);
                });
            });
        });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
