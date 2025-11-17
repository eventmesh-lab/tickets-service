using System.Collections.Generic;

namespace tickets_service.Domain.Abstractions;

/// <summary>
/// Clase base que centraliza el registro de eventos de dominio.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Eventos registrados por el agregado.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Registra un evento para ser publicado posteriormente.</summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
        {
            return;
        }

        _domainEvents.Add(domainEvent);
    }

    /// <summary>Limpia los eventos registrados (usado por la capa de infraestructura).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}