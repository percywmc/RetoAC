using CargaMasiva.Application.Abstractions;
using CargaMasiva.Application.Dtos;
using ClosedXML.Excel;

namespace CargaMasiva.Infrastructure.Excel;

public class ClosedXmlExcelReader : IExcelReader
{
    public IReadOnlyCollection<FilaExcelDto> LeerFilas(Stream contenido)
    {
        using var workbook = new XLWorkbook(contenido);
        var worksheet = workbook.Worksheets.First();
        var filas = new List<FilaExcelDto>();

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow is null)
        {
            return filas;
        }

        var columnas = headerRow.Cells()
            .ToDictionary(c => c.GetString().Trim().ToUpperInvariant(), c => c.Address.ColumnNumber);

        var colCodigo = columnas.GetValueOrDefault("CODIGOPRODUCTO");
        var colDescripcion = columnas.GetValueOrDefault("NOMBREPRODUCTO");
        var colCantidad = columnas.GetValueOrDefault("PRECIO");
        var colPeriodo = columnas.GetValueOrDefault("PERIODO");

        if (colCodigo == 0 || colDescripcion == 0 || colCantidad == 0 || colPeriodo == 0)
        {
            throw new FormatException("El archivo no tiene el formato correcto. Las columnas obligatorias son: Periodo, CodigoProducto, NombreProducto, Precio.");
        }

        var filasUsadas = worksheet.RowsUsed().Skip(1);

        foreach (var fila in filasUsadas)
        {
            var codigoProducto = colCodigo > 0 ? fila.Cell(colCodigo).GetString().Trim() : string.Empty;
            var descripcion = colDescripcion > 0 ? fila.Cell(colDescripcion).GetString().Trim() : null;
            var periodo = colPeriodo > 0 ? fila.Cell(colPeriodo).GetString().Trim() : null;

            decimal? cantidad = null;
            if (colCantidad > 0)
            {
                var celdaCantidad = fila.Cell(colCantidad);
                if (celdaCantidad.TryGetValue(out decimal valorCantidad))
                {
                    cantidad = valorCantidad;
                }
            }

            filas.Add(new FilaExcelDto(fila.RowNumber(), codigoProducto, descripcion, cantidad, periodo));
        }

        return filas;
    }
}
