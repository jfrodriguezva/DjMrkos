using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DjMrkos.Api.Security;

/// <summary>
/// Deliberately simple: the DJ is the only admin in v1, so one shared secret in the
/// `X-Api-Key` header is enough. It's a real gate (constant-time compare, no bypass), just
/// not the one you'd want with a second admin — see docs/ARCHITECTURE.md for the upgrade path.
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<ApiKeyOptions> apiKeyOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    private const string HeaderName = "X-Api-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var provided) || provided.Count == 0)
            return Task.FromResult(AuthenticateResult.Fail($"Falta el encabezado {HeaderName}."));

        var expected = apiKeyOptions.Value.ApiKey;
        var isValid = !string.IsNullOrEmpty(expected) &&
            System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(provided[0] ?? string.Empty),
                System.Text.Encoding.UTF8.GetBytes(expected));

        if (!isValid)
            return Task.FromResult(AuthenticateResult.Fail("API key inválida."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "dj-admin")], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
