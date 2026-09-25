namespace DjMrkos.Api.Security;

public sealed class ApiKeyOptions
{
    public const string SectionName = "Admin";

    /// <summary>Shared secret the admin SPA sends as `X-Api-Key`. Swap for JWT/OAuth once there's more than one admin — see docs/ARCHITECTURE.md.</summary>
    public string ApiKey { get; init; } = string.Empty;
}
