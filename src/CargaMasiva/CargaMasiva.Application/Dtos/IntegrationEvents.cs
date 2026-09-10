namespace RetoAC.IntegrationEvents;

public record CargaRegistradaEvent(Guid IdCarga, string RutaArchivo, string Usuario, string Email);

public record CargaFinalizadaEvent(Guid IdCarga, string Usuario, string Email, DateTime FechaFin);
