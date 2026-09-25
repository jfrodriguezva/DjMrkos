namespace DjMrkos.Application.Common.Exceptions;

/// <summary>Too many song requests from the same fingerprint in a short window. Maps to HTTP 429.</summary>
public sealed class RateLimitExceededException(string reason) : Exception(reason);
