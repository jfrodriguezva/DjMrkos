namespace DjMrkos.Domain.Common;

/// <summary>Thrown when an operation would violate an invariant owned by the domain model itself.</summary>
public sealed class DomainException(string message) : Exception(message);
