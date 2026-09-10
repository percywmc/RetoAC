using MassTransit;
using Notificaciones.Worker.Abstractions;
using RetoAC.IntegrationEvents;
using Polly;

namespace Notificaciones.Worker.Consumers;

public class NotificacionesConsumer : IConsumer<CargaFinalizadaEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ICargaEstadoRepository _cargaEstadoRepository;
    private readonly ILogger<NotificacionesConsumer> _logger;
    private readonly IAsyncPolicy _retryPolicy;

    public NotificacionesConsumer(
        IEmailSender emailSender,
        ICargaEstadoRepository cargaEstadoRepository,
        ILogger<NotificacionesConsumer> logger)
    {
        _emailSender = emailSender;
        _cargaEstadoRepository = cargaEstadoRepository;
        _logger = logger;

        // Polly: reintenta hasta 3 veces con backoff exponencial si el SMTP falla.
        // Si persisten los fallos, el error queda registrado y el mensaje se descarta
        // sin bloquear el consumo de otros mensajes (excepción capturada en Consume).
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    logger.LogWarning(exception,
                        "Reintento {RetryCount} de envío de correo tras esperar {Delay}s",
                        retryCount, timeSpan.TotalSeconds);
                });
    }

    public async Task Consume(ConsumeContext<CargaFinalizadaEvent> context)
    {
        var evento = context.Message;

        // ─────────────────────────────────────────────────────────────────────
        // IDEMPOTENCIA: verificar que la carga no esté ya en estado "Notificado".
        // Esto protege contra re-entregas del mismo mensaje por parte de RabbitMQ
        // (ej. si el worker cayó justo después de enviar el correo pero antes
        // de hacer el ACK), evitando correos duplicados o actualizaciones dobles.
        // ─────────────────────────────────────────────────────────────────────
        var estadoActual = await _cargaEstadoRepository.ObtenerEstadoAsync(
            evento.IdCarga, context.CancellationToken);

        if (estadoActual is "Notificado")
        {
            _logger.LogInformation(
                "Mensaje duplicado detectado para la carga {IdCarga} (estado ya es Notificado). Descartando sin efecto.",
                evento.IdCarga);
            return;
        }

        if (estadoActual is null)
        {
            _logger.LogWarning(
                "No se encontró la carga {IdCarga} en la base de datos. El mensaje no puede procesarse.",
                evento.IdCarga);
            return;
        }

        var asunto = "✅ Carga masiva finalizada — Atlantic City Casino Sports";
        var cuerpo = $"""
            Estimado usuario {evento.Usuario},

            Su proceso de carga masiva con identificador '{evento.IdCarga}' 
            finalizó exitosamente el {evento.FechaFin:yyyy-MM-dd HH:mm} UTC.

            Puede consultar el historial y el detalle de los registros procesados 
            en el portal de Atlantic City Casino Sports.

            Este es un correo automático, por favor no responder.

            © {DateTime.UtcNow.Year} Atlantic City Casino Sports
            """;

        try
        {
            // Polly aplica reintentos si el servidor SMTP no está disponible transitoriamente.
            await _retryPolicy.ExecuteAsync(
                ct => _emailSender.SendAsync(evento.Email, asunto, cuerpo, ct),
                context.CancellationToken);

            // Solo actualizar el estado si el correo se envió con éxito.
            await _cargaEstadoRepository.MarcarNotificadoAsync(
                evento.IdCarga, context.CancellationToken);

            _logger.LogInformation(
                "Correo de notificación enviado y estado actualizado a Notificado para la carga {IdCarga}",
                evento.IdCarga);
        }
        catch (Exception ex)
        {
            // Si Polly agotó los reintentos, se registra el fallo estructuradamente
            // pero NO se relanza la excepción → el mensaje se descarta (NACK implícito)
            // para no bloquear el consumo de los mensajes siguientes en la cola.
            _logger.LogError(ex,
                "No se pudo enviar el correo de notificación para la carga {IdCarga} tras los reintentos configurados. El mensaje se descarta.",
                evento.IdCarga);
        }
    }
}
