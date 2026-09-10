using CargaMasiva.Application.Dtos;
using CargaMasiva.Application.Exceptions;
using CargaMasiva.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using RetoAC.IntegrationEvents;

namespace CargaMasiva.Infrastructure.Messaging;

public class CargaMasivaConsumer : IConsumer<CargaRegistradaEvent>
{
    private readonly ProcesarCargaService _procesarCargaService;
    private readonly ILogger<CargaMasivaConsumer> _logger;

    public CargaMasivaConsumer(ProcesarCargaService procesarCargaService, ILogger<CargaMasivaConsumer> logger)
    {
        _procesarCargaService = procesarCargaService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CargaRegistradaEvent> context)
    {
        try
        {
            await _procesarCargaService.ProcesarAsync(context.Message, context.CancellationToken);
        }
        catch (CargaRechazadaException ex)
        {
            _logger.LogWarning(ex, "Carga {IdCarga} rechazada: {Mensaje}", context.Message.IdCarga, ex.Message);
        }
        catch (CargaBloqueadaException ex)
        {
            _logger.LogWarning(ex, "Carga {IdCarga} bloqueada: {Mensaje}", context.Message.IdCarga, ex.Message);
        }
    }
}
