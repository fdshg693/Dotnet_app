using dotnet_mvc_test.Services;

namespace dotnet_mvc_test.Tests.Services;

public class TwoFactorServiceTests
{
    private readonly ITwoFactorService _twoFactorService;

    public TwoFactorServiceTests()
    {
        _twoFactorService = new TwoFactorService();
    }

    #region FormatKey Tests

    [Fact]
    public void FormatKey_WithExactlyDivisibleByFour_ReturnsSpaceSeparatedLowercaseKey()
    {
        // Arrange
        var unformattedKey = "ABCD1234EFGH5678";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("abcd 1234 efgh 5678", result);
    }

    [Fact]
    public void FormatKey_WithNotDivisibleByFour_ReturnsSpaceSeparatedWithRemainder()
    {
        // Arrange
        var unformattedKey = "ABCD1234EF";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("abcd 1234 ef", result);
    }

    [Fact]
    public void FormatKey_WithLessThanFourCharacters_ReturnsLowercaseWithoutSpaces()
    {
        // Arrange
        var unformattedKey = "ABC";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("abc", result);
    }

    [Fact]
    public void FormatKey_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var unformattedKey = "";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void FormatKey_WithMixedCase_ReturnsAllLowercase()
    {
        // Arrange
        var unformattedKey = "AbCd1234EfGh";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("abcd 1234 efgh", result);
    }

    [Fact]
    public void FormatKey_WithExactlyFourCharacters_ReturnsLowercaseWithoutTrailingSpace()
    {
        // Arrange
        var unformattedKey = "ABCD";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("abcd", result);
    }

    [Fact]
    public void FormatKey_WithFiveCharacters_ReturnsFourCharsSpaceOneChar()
    {
        // Arrange
        var unformattedKey = "ABCDE";

        // Act
        var result = _twoFactorService.FormatKey(unformattedKey);

        // Assert
        Assert.Equal("abcd e", result);
    }

    #endregion

    #region GenerateQrCodeDataUri Tests

    [Fact]
    public void GenerateQrCodeDataUri_WithValidEmailAndKey_ReturnsDataUri()
    {
        // Arrange
        var email = "test@example.com";
        var authenticatorKey = "ABCD1234EFGH5678";

        // Act
        var result = _twoFactorService.GenerateQrCodeDataUri(email, authenticatorKey);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.StartsWith("data:image/png;base64,", result);
    }

    [Fact]
    public void GenerateQrCodeDataUri_ContainsBase64EncodedData_ReturnsValidBase64String()
    {
        // Arrange
        var email = "user@example.com";
        var authenticatorKey = "TEST1234KEY56789";

        // Act
        var result = _twoFactorService.GenerateQrCodeDataUri(email, authenticatorKey);

        // Assert
        var base64Part = result.Replace("data:image/png;base64,", "");
        Assert.True(base64Part.Length > 0);
        
        // Base64文字列として有効か検証
        var isValidBase64 = IsValidBase64String(base64Part);
        Assert.True(isValidBase64, "Generated data should be valid base64 string");
    }

    [Fact]
    public void GenerateQrCodeDataUri_WithDifferentEmails_GeneratesDifferentQrCodes()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var authenticatorKey = "SAMEKEY123456789";

        // Act
        var result1 = _twoFactorService.GenerateQrCodeDataUri(email1, authenticatorKey);
        var result2 = _twoFactorService.GenerateQrCodeDataUri(email2, authenticatorKey);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void GenerateQrCodeDataUri_WithDifferentKeys_GeneratesDifferentQrCodes()
    {
        // Arrange
        var email = "test@example.com";
        var key1 = "KEY1234567890ABC";
        var key2 = "DIFFERENTKEY1234";

        // Act
        var result1 = _twoFactorService.GenerateQrCodeDataUri(email, key1);
        var result2 = _twoFactorService.GenerateQrCodeDataUri(email, key2);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    #endregion

    #region Helper Methods

    private static bool IsValidBase64String(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
