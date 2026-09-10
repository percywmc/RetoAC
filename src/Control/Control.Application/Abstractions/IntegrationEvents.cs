namespace RetoAC.IntegrationEvents;

public record CargaRegistradaEvent(Guid IdCarga, string RutaArchivo, string Usuario, string Email);
