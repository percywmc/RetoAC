using CargaMasiva.Application;
using CargaMasiva.Infrastructure;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "CargaMasiva")
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddSerilog(Log.Logger);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("ControlDatabase") 
    ?? throw new InvalidOperationException("Falta ControlDatabase");
var logger = host.Services.GetRequiredService<ILogger<Program>>();
await CargaMasiva.Infrastructure.Persistence.CargaMasivaDbInitializer.InitializeAsync(connectionString, logger);

host.Run();
