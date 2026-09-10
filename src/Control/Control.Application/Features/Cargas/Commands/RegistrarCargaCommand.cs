using Control.Application.Dtos;
using MediatR;

namespace Control.Application.Features.Cargas.Commands;

public record RegistrarCargaCommand(
    string NombreArchivo,
    long TamanoBytes,
    Stream Contenido,
    string Usuario,
    string Email) : IRequest<CargaArchivoDto>;
