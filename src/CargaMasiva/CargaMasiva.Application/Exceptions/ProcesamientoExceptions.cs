namespace CargaMasiva.Application.Exceptions;

public class CargaRechazadaException : Exception
{
    public CargaRechazadaException(string mensaje) : base(mensaje)
    {
    }
}

public class CargaBloqueadaException : Exception
{
    public CargaBloqueadaException(string mensaje) : base(mensaje)
    {
    }
}
