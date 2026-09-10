namespace Notificaciones.Worker.Options;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string User { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromAddress { get; set; } = default!;
    public string FromName { get; set; } = "Atlantic City Casino Sports";
    public bool UseSsl { get; set; } = true;
}
