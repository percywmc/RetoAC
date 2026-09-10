using Control.Application.Abstractions;
using Control.Application.Exceptions;
using Control.Application.Features.Cargas.Commands;
using Control.Domain.Entities;
using Control.Domain.Enums;
using Control.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Control.Tests.Handlers;

public class RegistrarCargaCommandHandlerTests
{
    private readonly Mock<ICargaArchivoRepository> _repositoryMock = new();
    private readonly Mock<ISeaweedFsClient> _seaweedFsClientMock = new();
    private readonly Mock<IIntegrationEventPublisher> _eventPublisherMock = new();
    private readonly RegistrarCargaCommandHandler _handler;

    public RegistrarCargaCommandHandlerTests()
    {
        _handler = new RegistrarCargaCommandHandler(
            _repositoryMock.Object,
            _seaweedFsClientMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Debe_registrar_la_carga_subir_el_archivo_y_publicar_el_evento()
    {
        var comando = new RegistrarCargaCommand("archivo.xlsx", 1024, Stream.Null, "usuario@example.com");

        _seaweedFsClientMock
            .Setup(s => s.UploadAsync(comando.NombreArchivo, comando.Contenido, It.IsAny<CancellationToken>()))
            .ReturnsAsync("seaweed://archivo.xlsx");

        var resultado = await _handler.Handle(comando, CancellationToken.None);

        resultado.NombreArchivo.Should().Be(comando.NombreArchivo);
        resultado.Estado.Should().Be(EstadoCarga.Pendiente);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<CargaArchivo>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishCargaRegistradaAsync(It.IsAny<CargaRegistradaEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CargaArchivo CrearCargaConEstado(EstadoCarga estado)
    {
        var carga = new CargaArchivo(Guid.NewGuid(), "anterior.xlsx", "usuario@example.com", "seaweed://anterior.xlsx");

        switch (estado)
        {
            case EstadoCarga.EnProceso:
                carga.MarcarEnProceso();
                break;
            case EstadoCarga.Cargado:
                carga.MarcarCargado();
                break;
            case EstadoCarga.Finalizado:
                carga.MarcarFinalizado();
                break;
            case EstadoCarga.Notificado:
                carga.MarcarNotificado();
                break;
            case EstadoCarga.Rechazado:
                carga.Rechazar("motivo de prueba");
                break;
        }

        return carga;
    }
}
