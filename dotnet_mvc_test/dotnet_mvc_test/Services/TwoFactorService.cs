using System.Text;
using QRCoder;

namespace dotnet_mvc_test.Services;

public class TwoFactorService : ITwoFactorService
{
    private const string Issuer = "DotNetMVCTest";

    public string GenerateQrCodeDataUri(string email, string authenticatorKey)
    {
        var otpauthUri = $"otpauth://totp/{Issuer}:{email}?secret={authenticatorKey}&issuer={Issuer}";

        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(otpauthUri, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        string base64 = Convert.ToBase64String(qrCodeBytes);
        
        return $"data:image/png;base64,{base64}";
    }

    public string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }
}
