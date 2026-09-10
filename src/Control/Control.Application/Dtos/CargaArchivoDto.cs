using Control.Domain.Enums;

namespace Control.Application.Dtos;

public record CargaArchivoDto(
    Guid Id,
    string NombreArchivo,
    string Usuario,
    EstadoCarga Estado,
    DateTime FechaRegistro,
    DateTime? FechaFin);
