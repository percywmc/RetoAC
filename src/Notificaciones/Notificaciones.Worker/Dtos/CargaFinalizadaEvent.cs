namespace RetoAC.IntegrationEvents;

public record CargaFinalizadaEvent(Guid IdCarga, string Usuario, string Email, DateTime FechaFin);
