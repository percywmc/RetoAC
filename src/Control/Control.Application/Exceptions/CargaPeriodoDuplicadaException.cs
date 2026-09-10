namespace Control.Application.Exceptions;

public class CargaPeriodoDuplicadaException : Exception
{
    public CargaPeriodoDuplicadaException(string periodo)
        : base($"Ya existe una carga activa o finalizada para el periodo '{periodo}'.")
    {
    }
}
