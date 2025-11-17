namespace tickets_service.Domain.Tickets.ValueObjects;

/// <summary>Estados posibles del ciclo de vida del ticket.</summary>
public enum EstadoTicket
{
    Pendiente = 0,
    Confirmado = 1,
    Cancelado = 2,
    Usado = 3
}

