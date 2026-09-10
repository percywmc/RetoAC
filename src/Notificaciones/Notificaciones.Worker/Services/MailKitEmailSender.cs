using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Notificaciones.Worker.Abstractions;
using Notificaciones.Worker.Options;

namespace Notificaciones.Worker.Services;

public class MailKitEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public MailKitEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string destinatario, string asunto, string cuerpo, CancellationToken cancellationToken)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("plain") { Text = cuerpo };

        using var smtpClient = new SmtpClient();
        var secureSocketOptions = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

        await smtpClient.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken);
        await smtpClient.AuthenticateAsync(_options.User, _options.Password, cancellationToken);
        await smtpClient.SendAsync(mensaje, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
