using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Notificaciones.Worker.Abstractions;
using Notificaciones.Worker.Consumers;
using Notificaciones.Worker.Options;
using Notificaciones.Worker.Persistence;
using Notificaciones.Worker.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// ---------------------------------------------------------------------------
// Serilog: logging estructurado
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Notificaciones")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddSerilog(Log.Logger);

// ---------------------------------------------------------------------------
// Options Pattern: todas las configuraciones se inyectan vía IOptions<T>
// ---------------------------------------------------------------------------
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection(DatabaseOptions.SectionName));

// ---------------------------------------------------------------------------
// Inyección de dependencias de la capa de infraestructura
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<ICargaEstadoRepository, DapperCargaEstadoRepository>();

// ---------------------------------------------------------------------------
// MassTransit + RabbitMQ: consumidor de la cola "notificaciones"
// ---------------------------------------------------------------------------
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificacionesConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitOptions = builder.Configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>()!;

        cfg.Host(rabbitOptions.Host, h =>
        {
            h.Username(rabbitOptions.Username);
            h.Password(rabbitOptions.Password);
        });

        cfg.ReceiveEndpoint(rabbitOptions.NotificacionesQueue, e =>
        {
            // Idempotencia: si el mensaje ya fue procesado (estado = Notificado),
            // el consumidor no vuelve a enviar el correo ni actualiza el estado.
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)));

            e.ConfigureConsumer<NotificacionesConsumer>(ctx);
        });
    });
});

// ---------------------------------------------------------------------------
// Health Checks: /health (SQL Server + RabbitMQ)
// ---------------------------------------------------------------------------
var dbConnectionString = builder.Configuration[$"{DatabaseOptions.SectionName}:ControlDatabase"]
                         ?? string.Empty;
var rabbitHost = builder.Configuration[$"{RabbitMqOptions.SectionName}:Host"]
                 ?? "localhost";
var rabbitUser = builder.Configuration[$"{RabbitMqOptions.SectionName}:Username"]
                 ?? string.Empty;
var rabbitPass = builder.Configuration[$"{RabbitMqOptions.SectionName}:Password"]
                 ?? string.Empty;

builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: dbConnectionString,
        name: "sqlserver",
        failureStatus: HealthStatus.Degraded,
        tags: ["db", "sql"])
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{Uri.EscapeDataString(rabbitUser)}:{Uri.EscapeDataString(rabbitPass)}@{rabbitHost}",
        name: "rabbitmq",
        failureStatus: HealthStatus.Degraded,
        tags: ["messaging", "rabbitmq"]);

var host = builder.Build();

try
{
    Log.Information("Iniciando Notificaciones.Worker...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El Notificaciones.Worker terminó de forma inesperada.");
}
finally
{
    await Log.CloseAndFlushAsync();
}
