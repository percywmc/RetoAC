using Control.Application.Dtos;
using MediatR;

namespace Control.Application.Features.Cargas.Queries;

public record ListarCargasQuery(string? Usuario, int PageNumber, int PageSize) : IRequest<PagedResultDto<CargaArchivoDto>>;
