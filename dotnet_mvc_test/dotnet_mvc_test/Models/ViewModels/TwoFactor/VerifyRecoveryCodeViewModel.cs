using System.ComponentModel.DataAnnotations;

namespace dotnet_mvc_test.Models.ViewModels.TwoFactor;

public class VerifyRecoveryCodeViewModel
{
    [Required(ErrorMessage = "リカバリーコードは必須です")]
    [Display(Name = "リカバリーコード")]
    public string RecoveryCode { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
