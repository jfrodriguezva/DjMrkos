namespace DjMrkos.Application.Common.Interfaces;

/// <summary>
/// Issues the opaque, unguessable token embedded in an event's QR code and generates the
/// PNG for it. Implemented in Infrastructure (HMAC signing is an infrastructure concern,
/// not a domain one) but the Application layer is what decides when a token is minted.
/// </summary>
public interface IQrTokenService
{
    string IssueToken(Guid eventId);

    /// <summary>Renders a PNG (as a data URL) that points guests at the public song-request page for this token.</summary>
    string GenerateQrCodeDataUrl(string token, string publicBaseUrl);
}
