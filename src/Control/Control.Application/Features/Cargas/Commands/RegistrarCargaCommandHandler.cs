using Control.Application.Abstractions;
using Control.Application.Dtos;
using Control.Application.Exceptions;
using Control.Domain.Entities;
using Control.Domain.Enums;
using Control.Domain.Repositories;
using MediatR;
using RetoAC.IntegrationEvents;

namespace Control.Application.Features.Cargas.Commands;

public class RegistrarCargaCommandHandler : IRequestHandler<RegistrarCargaCommand, CargaArchivoDto>
{
    private readonly ICargaArchivoRepository _cargaArchivoRepository;
    private readonly ISeaweedFsClient _seaweedFsClient;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public RegistrarCargaCommandHandler(
        ICargaArchivoRepository cargaArchivoRepository,
        ISeaweedFsClient seaweedFsClient,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _cargaArchivoRepository = cargaArchivoRepository;
        _seaweedFsClient = seaweedFsClient;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task<CargaArchivoDto> Handle(RegistrarCargaCommand request, CancellationToken cancellationToken)
    {
        var rutaArchivo = await _seaweedFsClient.UploadAsync(request.NombreArchivo, request.Contenido, cancellationToken);

        var cargaArchivo = new CargaArchivo(Guid.NewGuid(), request.NombreArchivo, request.Usuario, rutaArchivo);
        await _cargaArchivoRepository.AddAsync(cargaArchivo, cancellationToken);
        await _cargaArchivoRepository.SaveChangesAsync(cancellationToken);

        await _integrationEventPublisher.PublishCargaRegistradaAsync(
            new CargaRegistradaEvent(cargaArchivo.Id, cargaArchivo.RutaArchivo, request.Usuario, request.Email),
            cancellationToken);

        return new CargaArchivoDto(
            cargaArchivo.Id,
            cargaArchivo.NombreArchivo,
            cargaArchivo.Usuario,
            cargaArchivo.Estado,
            cargaArchivo.FechaRegistro,
            cargaArchivo.FechaFin);
    }
}
