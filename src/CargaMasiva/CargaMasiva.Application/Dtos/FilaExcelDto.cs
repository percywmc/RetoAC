namespace CargaMasiva.Application.Dtos;

public record FilaExcelDto(int NumeroFila, string CodigoProducto, string? NombreProducto, decimal? Precio, string? Periodo);
