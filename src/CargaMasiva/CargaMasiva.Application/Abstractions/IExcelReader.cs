using CargaMasiva.Application.Dtos;

namespace CargaMasiva.Application.Abstractions;

public interface IExcelReader
{
    IReadOnlyCollection<FilaExcelDto> LeerFilas(Stream contenido);
}
