using Control.Application.Dtos;
using Control.Domain.Repositories;
using MediatR;

namespace Control.Application.Features.Cargas.Queries;

public class ListarCargasQueryHandler : IRequestHandler<ListarCargasQuery, PagedResultDto<CargaArchivoDto>>
{
    private readonly ICargaArchivoRepository _cargaArchivoRepository;

    public ListarCargasQueryHandler(ICargaArchivoRepository cargaArchivoRepository)
    {
        _cargaArchivoRepository = cargaArchivoRepository;
    }

    public async Task<PagedResultDto<CargaArchivoDto>> Handle(ListarCargasQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _cargaArchivoRepository.GetPagedAsync(
            request.Usuario, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = items.Select(c => new CargaArchivoDto(
            c.Id, c.NombreArchivo, c.Usuario, c.Estado, c.FechaRegistro, c.FechaFin)).ToList();

        return new PagedResultDto<CargaArchivoDto>(dtos, request.PageNumber, request.PageSize, totalCount);
    }
}
