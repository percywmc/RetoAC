using Control.Application.Dtos;
using Control.Application.Exceptions;
using Control.Domain.Repositories;
using MediatR;

namespace Control.Application.Features.Cargas.Queries;

public class ObtenerCargaQueryHandler : IRequestHandler<ObtenerCargaQuery, CargaArchivoDto>
{
    private readonly ICargaArchivoRepository _cargaArchivoRepository;

    public ObtenerCargaQueryHandler(ICargaArchivoRepository cargaArchivoRepository)
    {
        _cargaArchivoRepository = cargaArchivoRepository;
    }

    public async Task<CargaArchivoDto> Handle(ObtenerCargaQuery request, CancellationToken cancellationToken)
    {
        var carga = await _cargaArchivoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new CargaNoEncontradaException(request.Id);

        return new CargaArchivoDto(
            carga.Id, carga.NombreArchivo, carga.Usuario, carga.Estado, carga.FechaRegistro, carga.FechaFin);
    }
}
