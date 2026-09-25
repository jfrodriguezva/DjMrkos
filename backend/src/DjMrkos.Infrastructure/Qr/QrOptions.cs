namespace DjMrkos.Infrastructure.Qr;

public sealed class QrOptions
{
    public const string SectionName = "Qr";

    /// <summary>HMAC key used to sign tokens. Must be at least 32 random bytes in production — see docs/ARCHITECTURE.md.</summary>
    public string SigningKey { get; init; } = string.Empty;
}
