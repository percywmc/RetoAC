using Control.Application.Dtos;
using MediatR;

namespace Control.Application.Features.Cargas.Queries;

public record ObtenerCargaQuery(Guid Id) : IRequest<CargaArchivoDto>;
