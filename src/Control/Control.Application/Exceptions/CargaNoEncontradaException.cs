namespace Control.Application.Exceptions;

public class CargaNoEncontradaException : Exception
{
    public CargaNoEncontradaException(Guid id) : base($"No se encontró la carga con Id '{id}'.")
    {
    }
}
