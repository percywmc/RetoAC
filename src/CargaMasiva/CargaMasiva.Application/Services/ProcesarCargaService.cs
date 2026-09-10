using CargaMasiva.Application.Abstractions;
using CargaMasiva.Application.Dtos;
using CargaMasiva.Application.Exceptions;
using CargaMasiva.Domain.Entities;
using CargaMasiva.Domain.Enums;
using Microsoft.Extensions.Logging;
using RetoAC.IntegrationEvents;

namespace CargaMasiva.Application.Services;

public class ProcesarCargaService
{
    private readonly IFileDownloader _fileDownloader;
    private readonly IExcelReader _excelReader;
    private readonly IDataProcesadaRepository _dataProcesadaRepository;
    private readonly ICargaFallidaRepository _cargaFallidaRepository;
    private readonly IRegistroFallidoRepository _registroFallidoRepository;
    private readonly ICargaEstadoService _cargaEstadoService;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<ProcesarCargaService> _logger;

    private static readonly EstadoCarga[] EstadosRechazoDefinitivo =
    [
        EstadoCarga.Cargado,
        EstadoCarga.Finalizado,
        EstadoCarga.Notificado
    ];

    private static readonly EstadoCarga[] EstadosBloqueo =
    [
        EstadoCarga.Pendiente,
        EstadoCarga.EnProceso
    ];

    public ProcesarCargaService(
        IFileDownloader fileDownloader,
        IExcelReader excelReader,
        IDataProcesadaRepository dataProcesadaRepository,
        ICargaFallidaRepository cargaFallidaRepository,
        IRegistroFallidoRepository registroFallidoRepository,
        ICargaEstadoService cargaEstadoService,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<ProcesarCargaService> logger)
    {
        _fileDownloader = fileDownloader;
        _excelReader = excelReader;
        _dataProcesadaRepository = dataProcesadaRepository;
        _cargaFallidaRepository = cargaFallidaRepository;
        _registroFallidoRepository = registroFallidoRepository;
        _cargaEstadoService = cargaEstadoService;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task ProcesarAsync(CargaRegistradaEvent evento, CancellationToken cancellationToken)
    {
        await _cargaEstadoService.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.EnProceso, null, cancellationToken);

        await using var contenido = await _fileDownloader.DownloadAsync(evento.RutaArchivo, cancellationToken);
        IReadOnlyCollection<FilaExcelDto> filas;
        try
        {
            filas = _excelReader.LeerFilas(contenido);
        }
        catch (FormatException ex)
        {
            await _cargaEstadoService.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.Rechazado, DateTime.UtcNow, cancellationToken);
            await _cargaFallidaRepository.AddRangeAsync([new CargaFallida(evento.IdCarga, MotivoFallo.FormatoInvalido, ex.Message)], cancellationToken);
            throw new CargaRechazadaException(ex.Message);
        }

        var filasNoVacias = filas
            .Where(f => !EsFilaVacia(f))
            .ToList();

        var registrosFallidos = new List<RegistroFallido>();
        var cargasFallidas = new List<CargaFallida>();
        var registrosValidos = new List<DataProcesada>();

        var periodos = filasNoVacias
            .Select(f => f.Periodo)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        foreach (var p in periodos)
        {
            await ValidarPeriodoAsync(evento.IdCarga, p!, cargasFallidas, cancellationToken);
        }

        var codigos = filasNoVacias.Select(f => f.CodigoProducto).Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
        var codigosExistentes = await _dataProcesadaRepository.GetCodigosExistentesAsync(codigos, cancellationToken);

        foreach (var fila in filasNoVacias)
        {
            var datosJson = $"{{ CodigoProducto: '{fila.CodigoProducto}', NombreProducto: '{fila.NombreProducto}', Precio: {fila.Precio}, Periodo: '{fila.Periodo}' }}";

            if (string.IsNullOrWhiteSpace(fila.CodigoProducto) || string.IsNullOrWhiteSpace(fila.Periodo))
            {
                registrosFallidos.Add(new RegistroFallido(evento.IdCarga, fila.NumeroFila, MotivoFallo.DatosFaltantes,
                    "El Periodo y el CodigoProducto son obligatorios en cada registro.", datosJson));
                continue;
            }

            var codigoProducto = fila.CodigoProducto;

            if (codigosExistentes.Contains(codigoProducto))
            {
                registrosFallidos.Add(new RegistroFallido(evento.IdCarga, fila.NumeroFila, MotivoFallo.CodigoExistente,
                    $"El código de producto '{codigoProducto}' ya existe en el sistema.", datosJson));
                continue;
            }

            var nombreProducto = string.IsNullOrWhiteSpace(fila.NombreProducto) ? "Sin descripción" : fila.NombreProducto;
            var precio = fila.Precio ?? 0;

            registrosValidos.Add(new DataProcesada(evento.IdCarga, codigoProducto, nombreProducto, precio, fila.Periodo));
            codigosExistentes.Add(codigoProducto);
        }

        if (cargasFallidas.Count > 0)
        {
            await _cargaFallidaRepository.AddRangeAsync(cargasFallidas, cancellationToken);
        }

        if (registrosFallidos.Count > 0)
        {
            await _registroFallidoRepository.BulkInsertAsync(registrosFallidos, cancellationToken);
        }

        if (registrosValidos.Count > 0)
        {
            await _dataProcesadaRepository.BulkInsertAsync(registrosValidos, cancellationToken);
        }

        await _cargaEstadoService.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.Cargado, null, cancellationToken);

        var fechaFin = DateTime.UtcNow;
        

        await _integrationEventPublisher.PublishCargaFinalizadaAsync(
            new CargaFinalizadaEvent(evento.IdCarga, evento.Usuario, evento.Email, fechaFin),
            cancellationToken);
        await _cargaEstadoService.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.Finalizado, fechaFin, cancellationToken);
    }

    private async Task ValidarPeriodoAsync(
        Guid idCarga, string periodo, List<CargaFallida> registrosFallidos, CancellationToken cancellationToken)
    {
        var cargaPrevia = await _cargaEstadoService.ObtenerUltimaCargaPorPeriodoAsync(periodo, idCarga, cancellationToken);
        if (cargaPrevia is null)
        {
            return;
        }

        var (_, estado) = cargaPrevia.Value;

        if (EstadosRechazoDefinitivo.Contains(estado))
        {
            var mensaje = $"Ya existe una carga {estado} para el periodo '{periodo}'. La carga es rechazada.";
            registrosFallidos.Add(new CargaFallida(idCarga, MotivoFallo.PeriodoRechazado, mensaje));
            await _cargaEstadoService.ActualizarEstadoAsync(idCarga, EstadoCarga.Rechazado, DateTime.UtcNow, cancellationToken);
            throw new CargaRechazadaException(mensaje);
        }

        if (EstadosBloqueo.Contains(estado))
        {
            var mensaje = $"Existe una carga en estado {estado} para el periodo '{periodo}'. La carga queda bloqueada.";
            registrosFallidos.Add(new CargaFallida(idCarga, MotivoFallo.PeriodoBloqueado, mensaje));
            await _cargaFallidaRepository.AddRangeAsync(registrosFallidos, cancellationToken);
            throw new CargaBloqueadaException(mensaje);
        }
    }

    private static bool EsFilaVacia(FilaExcelDto fila)
    {
        return string.IsNullOrWhiteSpace(fila.CodigoProducto)
            && string.IsNullOrWhiteSpace(fila.NombreProducto)
            && fila.Precio is null
            && string.IsNullOrWhiteSpace(fila.Periodo);
    }
}
