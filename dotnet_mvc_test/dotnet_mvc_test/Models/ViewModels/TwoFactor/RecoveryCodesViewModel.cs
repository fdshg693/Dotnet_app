namespace dotnet_mvc_test.Models.ViewModels.TwoFactor;

public class RecoveryCodesViewModel
{
    /// <summary>
    /// リカバリーコードのリスト
    /// </summary>
    public IEnumerable<string> RecoveryCodes { get; set; } = Enumerable.Empty<string>();
}
