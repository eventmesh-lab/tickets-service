using System;

namespace tickets_service.Domain.Abstractions;

/// <summary>
/// Representa un evento de dominio emitido por un agregado.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Fecha en que ocurrió el evento en UTC.</summary>
    DateTime OccurredOn { get; }
}