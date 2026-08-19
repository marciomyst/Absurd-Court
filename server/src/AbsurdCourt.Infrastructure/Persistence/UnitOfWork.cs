using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence;

public sealed class UnitOfWork(CourtDbContext db, IPublisher publisher) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregatesWithEvents = db.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregatesWithEvents.SelectMany(a => a.DomainEvents).ToList();

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException();
        }

        foreach (var aggregate in aggregatesWithEvents)
            aggregate.ClearDomainEvents();

        // Published only after the save commits, so notified clients never hear about
        // state that didn't actually stick.
        foreach (var domainEvent in domainEvents)
        {
            var wrapperType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var wrapper = (INotification)Activator.CreateInstance(wrapperType, domainEvent)!;
            await publisher.Publish(wrapper, ct);
        }
    }
}
