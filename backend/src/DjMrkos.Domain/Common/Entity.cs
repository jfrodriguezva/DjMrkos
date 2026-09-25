namespace DjMrkos.Domain.Common;

/// <summary>Base type for every entity that is identified and persisted by its <see cref="Id"/>.</summary>
public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public override bool Equals(object? obj) => obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
