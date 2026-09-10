namespace CargaMasiva.Infrastructure.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string CargaMasivaQueue { get; set; } = "carga_masiva";
    public string NotificacionesQueue { get; set; } = "notificaciones";
}
