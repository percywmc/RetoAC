using MediatR;

namespace Control.Application.Features.Cargas.Queries;

public record ObtenerContenidoQuery(Guid Id) : IRequest<ContenidoArchivoResult>;

public record ContenidoArchivoResult(string NombreArchivo, Stream Contenido);
