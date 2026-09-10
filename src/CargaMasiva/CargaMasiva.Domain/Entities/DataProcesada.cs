namespace CargaMasiva.Domain.Entities;

public class DataProcesada
{
    public Guid Id { get; private set; }
    public Guid IdCarga { get; private set; }
    public string CodigoProducto { get; private set; } = default!;
    public string? NombreProducto { get; private set; }
    public decimal? Precio { get; private set; }
    public string? Periodo { get; private set; }
    public DateTime FechaRegistro { get; private set; }

    private DataProcesada() { }

    public DataProcesada(Guid idCarga, string codigoProducto, string? nombreProducto, decimal? precio, string? periodo)
    {
        Id = Guid.NewGuid();
        IdCarga = idCarga;
        CodigoProducto = codigoProducto;
        NombreProducto = nombreProducto;
        Precio = precio;
        Periodo = periodo;
        FechaRegistro = DateTime.UtcNow;
    }
}
