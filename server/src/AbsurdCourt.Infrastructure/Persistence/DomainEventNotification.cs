using AbsurdCourt.Domain.Common;
using MediatR;

namespace AbsurdCourt.Infrastructure.Persistence;

/// <summary>
/// Bridges a pure Domain event (which only knows about IDomainEvent, not MediatR) into
/// MediatR's INotification so UnitOfWork can publish it after a successful save. Domain
/// stays free of any MediatR/Infrastructure reference this way.
/// </summary>
public sealed class DomainEventNotification<TEvent>(TEvent domainEvent) : INotification
    where TEvent : IDomainEvent
{
    public TEvent DomainEvent { get; } = domainEvent;
}
