namespace dotnet_mvc_test.Services;

public interface ITwoFactorService
{
    // QRコードのData URIを生成（Base64 PNG）
    string GenerateQrCodeDataUri(string email, string authenticatorKey);
    
    // 認証キーをフォーマット（4文字ごとにスペース区切り）
    string FormatKey(string unformattedKey);
}
