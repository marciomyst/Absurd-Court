namespace AbsurdCourt.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; }

    protected Entity() { }

    protected Entity(Guid id) => Id = id;

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
