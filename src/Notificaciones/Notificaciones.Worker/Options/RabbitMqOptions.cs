namespace Notificaciones.Worker.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string NotificacionesQueue { get; set; } = "notificaciones";
}
