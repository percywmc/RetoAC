using Control.Application.Abstractions;
using Control.Domain.Repositories;
using Control.Infrastructure.ExternalServices;
using Control.Infrastructure.Messaging;
using Control.Infrastructure.Options;
using Control.Infrastructure.Persistence;
using Control.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Control.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ControlDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ControlDatabase")));

        services.Configure<SeaweedFsOptions>(configuration.GetSection(SeaweedFsOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();

        services.AddHttpClient<ISeaweedFsClient, SeaweedFsClient>((serviceProvider, client) =>
            {
                var seaweedFsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SeaweedFsOptions>>().Value;
                client.BaseAddress = new Uri(seaweedFsOptions.FilerUrl);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();

        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("La configuración 'RabbitMq' es obligatoria.");

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, "/", h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
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
