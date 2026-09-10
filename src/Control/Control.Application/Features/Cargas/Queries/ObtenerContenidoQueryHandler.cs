using Control.Application.Abstractions;
using Control.Application.Exceptions;
using Control.Domain.Repositories;
using MediatR;

namespace Control.Application.Features.Cargas.Queries;

public class ObtenerContenidoQueryHandler : IRequestHandler<ObtenerContenidoQuery, ContenidoArchivoResult>
{
    private readonly ICargaArchivoRepository _cargaArchivoRepository;
    private readonly ISeaweedFsClient _seaweedFsClient;

    public ObtenerContenidoQueryHandler(ICargaArchivoRepository cargaArchivoRepository, ISeaweedFsClient seaweedFsClient)
    {
        _cargaArchivoRepository = cargaArchivoRepository;
        _seaweedFsClient = seaweedFsClient;
    }

    public async Task<ContenidoArchivoResult> Handle(ObtenerContenidoQuery request, CancellationToken cancellationToken)
    {
        var carga = await _cargaArchivoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new CargaNoEncontradaException(request.Id);

        var contenido = await _seaweedFsClient.DownloadAsync(carga.RutaArchivo, cancellationToken);

        return new ContenidoArchivoResult(carga.NombreArchivo, contenido);
    }
}
