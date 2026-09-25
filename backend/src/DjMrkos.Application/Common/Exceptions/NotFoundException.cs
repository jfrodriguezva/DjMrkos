namespace DjMrkos.Application.Common.Exceptions;

/// <summary>Maps to HTTP 404 at the API boundary.</summary>
public sealed class NotFoundException(string entity, object key)
    : Exception($"No se encontró '{entity}' con clave '{key}'.");
