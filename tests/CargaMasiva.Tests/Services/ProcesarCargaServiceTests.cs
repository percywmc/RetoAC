using CargaMasiva.Application.Abstractions;
using CargaMasiva.Application.Dtos;
using CargaMasiva.Application.Exceptions;
using CargaMasiva.Application.Services;
using CargaMasiva.Domain.Entities;
using CargaMasiva.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RetoAC.IntegrationEvents;

namespace CargaMasiva.Tests.Services;

public class ProcesarCargaServiceTests
{
    private readonly Mock<IFileDownloader> _fileDownloaderMock = new();
    private readonly Mock<IExcelReader> _excelReaderMock = new();
    private readonly Mock<IDataProcesadaRepository> _dataProcesadaRepositoryMock = new();
    private readonly Mock<ICargaFallidaRepository> _cargaFallidaRepositoryMock = new();
    private readonly Mock<IRegistroFallidoRepository> _registroFallidoRepositoryMock = new();
    private readonly Mock<ICargaEstadoService> _cargaEstadoServiceMock = new();
    private readonly Mock<IIntegrationEventPublisher> _integrationEventPublisherMock = new();
    private readonly ProcesarCargaService _service;

    public ProcesarCargaServiceTests()
    {
        _fileDownloaderMock
            .Setup(d => d.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Stream.Null);

        _cargaEstadoServiceMock
            .Setup(s => s.ObtenerUltimaCargaPorPeriodoAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<Guid, EstadoCarga>?)null);

        _dataProcesadaRepositoryMock
            .Setup(r => r.GetCodigosExistentesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        _service = new ProcesarCargaService(
            _fileDownloaderMock.Object,
            _excelReaderMock.Object,
            _dataProcesadaRepositoryMock.Object,
            _cargaFallidaRepositoryMock.Object,
            _registroFallidoRepositoryMock.Object,
            _cargaEstadoServiceMock.Object,
            _integrationEventPublisherMock.Object,
            Mock.Of<ILogger<ProcesarCargaService>>());
    }

    private CargaRegistradaEvent CrearEvento() => new(Guid.NewGuid(), "seaweed://archivo.xlsx", "usuario@example.com");

    [Fact]
    public async Task Debe_procesar_filas_validas_e_insertarlas_y_finalizar_la_carga()
    {
        var evento = CrearEvento();
        _excelReaderMock.Setup(r => r.LeerFilas(It.IsAny<Stream>())).Returns(new List<FilaExcelDto>
        {
            new(1, "COD-001", "Producto 1", 10, "2026-01"),
            new(2, "COD-002", "Producto 2", 20, "2026-01")
        });

        await _service.ProcesarAsync(evento, CancellationToken.None);

        _dataProcesadaRepositoryMock.Verify(
            r => r.BulkInsertAsync(It.Is<IReadOnlyCollection<DataProcesada>>(l => l.Count == 2), It.IsAny<CancellationToken>()),
            Times.Once);

        _cargaEstadoServiceMock.Verify(
            s => s.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.EnProceso, null, It.IsAny<CancellationToken>()), Times.Once);
        _cargaEstadoServiceMock.Verify(
            s => s.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.Cargado, null, It.IsAny<CancellationToken>()), Times.Once);
        _cargaEstadoServiceMock.Verify(
            s => s.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.Finalizado, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);

        _integrationEventPublisherMock.Verify(
            p => p.PublishCargaFinalizadaAsync(It.IsAny<CargaFinalizadaEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Debe_ignorar_filas_completamente_vacias()
    {
        var evento = CrearEvento();
        _excelReaderMock.Setup(r => r.LeerFilas(It.IsAny<Stream>())).Returns(new List<FilaExcelDto>
        {
            new(1, "COD-001", "Producto 1", 10, "2026-01"),
            new(2, string.Empty, null, null, null)
        });

        await _service.ProcesarAsync(evento, CancellationToken.None);

        _dataProcesadaRepositoryMock.Verify(
            r => r.BulkInsertAsync(It.Is<IReadOnlyCollection<DataProcesada>>(l => l.Count == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Debe_asignar_valores_por_defecto_cuando_campos_estan_vacios()
    {
        var evento = CrearEvento();
        _excelReaderMock.Setup(r => r.LeerFilas(It.IsAny<Stream>())).Returns(new List<FilaExcelDto>
        {
            new(1, "COD-001", null, null, "2026-01")
        });

        IReadOnlyCollection<DataProcesada>? capturado = null;
        _dataProcesadaRepositoryMock
            .Setup(r => r.BulkInsertAsync(It.IsAny<IReadOnlyCollection<DataProcesada>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<DataProcesada>, CancellationToken>((registros, _) => capturado = registros)
            .Returns(Task.CompletedTask);

        await _service.ProcesarAsync(evento, CancellationToken.None);

        capturado.Should().NotBeNull();
        capturado!.First().NombreProducto.Should().Be("Sin descripción");
        capturado.First().Precio.Should().Be(0);
    }

    [Fact]
    public async Task Debe_reportar_codigo_existente_y_no_insertarlo()
    {
        var evento = CrearEvento();
        _excelReaderMock.Setup(r => r.LeerFilas(It.IsAny<Stream>())).Returns(new List<FilaExcelDto>
        {
            new(1, "COD-001", "Producto 1", 10, "2026-01")
        });

        _dataProcesadaRepositoryMock
            .Setup(r => r.GetCodigosExistentesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "COD-001" });

        await _service.ProcesarAsync(evento, CancellationToken.None);

        _registroFallidoRepositoryMock.Verify(
            r => r.BulkInsertAsync(
                It.Is<IReadOnlyCollection<RegistroFallido>>(l => l.Any(f => f.Motivo == MotivoFallo.CodigoExistente)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _dataProcesadaRepositoryMock.Verify(
            r => r.BulkInsertAsync(It.Is<IReadOnlyCollection<DataProcesada>>(l => l.Count == 0), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EstadoCarga.Cargado)]
    [InlineData(EstadoCarga.Finalizado)]
    [InlineData(EstadoCarga.Notificado)]
    public async Task Debe_rechazar_la_carga_cuando_el_periodo_ya_esta_cargado_finalizado_o_notificado(EstadoCarga estadoPrevio)
    {
        var evento = CrearEvento();
        _excelReaderMock.Setup(r => r.LeerFilas(It.IsAny<Stream>())).Returns(new List<FilaExcelDto>
        {
            new(1, "COD-001", "Producto 1", 10, "2026-01")
        });

        _cargaEstadoServiceMock
            .Setup(s => s.ObtenerUltimaCargaPorPeriodoAsync("2026-01", evento.IdCarga, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid.NewGuid(), estadoPrevio));

        var accion = () => _service.ProcesarAsync(evento, CancellationToken.None);

        await accion.Should().ThrowAsync<CargaRechazadaException>();

        _cargaEstadoServiceMock.Verify(
            s => s.ActualizarEstadoAsync(evento.IdCarga, EstadoCarga.Rechazado, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _dataProcesadaRepositoryMock.Verify(
            r => r.BulkInsertAsync(It.IsAny<IReadOnlyCollection<DataProcesada>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(EstadoCarga.Pendiente)]
    [InlineData(EstadoCarga.EnProceso)]
    public async Task Debe_bloquear_la_carga_cuando_el_periodo_tiene_una_carga_concurrente(EstadoCarga estadoPrevio)
    {
        var evento = CrearEvento();
        _excelReaderMock.Setup(r => r.LeerFilas(It.IsAny<Stream>())).Returns(new List<FilaExcelDto>
        {
            new(1, "COD-001", "Producto 1", 10, "2026-01")
        });

        _cargaEstadoServiceMock
            .Setup(s => s.ObtenerUltimaCargaPorPeriodoAsync("2026-01", evento.IdCarga, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid.NewGuid(), estadoPrevio));

        var accion = () => _service.ProcesarAsync(evento, CancellationToken.None);

        await accion.Should().ThrowAsync<CargaBloqueadaException>();

        _dataProcesadaRepositoryMock.Verify(
            r => r.BulkInsertAsync(It.IsAny<IReadOnlyCollection<DataProcesada>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
