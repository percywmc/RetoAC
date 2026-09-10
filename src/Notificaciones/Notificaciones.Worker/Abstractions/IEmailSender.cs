namespace Notificaciones.Worker.Abstractions;

public interface IEmailSender
{
    Task SendAsync(string destinatario, string asunto, string cuerpo, CancellationToken cancellationToken);
}
