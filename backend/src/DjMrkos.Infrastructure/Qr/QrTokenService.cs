using System.Security.Cryptography;
using System.Text;
using DjMrkos.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using QRCoder;

namespace DjMrkos.Infrastructure.Qr;

/// <summary>
/// Mints the opaque token embedded in an event's QR code and renders the QR image itself.
/// The token is HMAC-SHA256(eventId + a random nonce) so it cannot be guessed even by
/// someone who knows the event's id — the event repository still does the actual lookup
/// and the event's own <c>QrValidFromUtc</c>/<c>QrValidUntilUtc</c> window decide validity.
/// </summary>
public sealed class QrTokenService(IOptions<QrOptions> options) : IQrTokenService
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(options.Value.SigningKey);

    public string IssueToken(Guid eventId)
    {
        var nonce = RandomNumberGenerator.GetBytes(16);
        var payload = eventId.ToByteArray().Concat(nonce).ToArray();
        var signature = HMACSHA256.HashData(_key, payload);

        return ToBase64Url(payload.Concat(signature).ToArray());
    }

    public string GenerateQrCodeDataUrl(string token, string publicBaseUrl)
    {
        var targetUrl = $"{publicBaseUrl.TrimEnd('/')}/evento/{token}";

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(targetUrl, QRCodeGenerator.ECCLevel.Q);
        var pngBytes = new PngByteQRCode(data).GetGraphic(20);

        return $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
    }

    private static string ToBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
