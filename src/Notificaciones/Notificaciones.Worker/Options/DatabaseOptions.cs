namespace Notificaciones.Worker.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ControlDatabase { get; set; } = default!;
}
