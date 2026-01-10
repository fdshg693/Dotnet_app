using System.ComponentModel.DataAnnotations;

namespace dotnet_mvc_test.Models.ViewModels.TwoFactor;

public class SetupTwoFactorViewModel
{
    /// <summary>
    /// QRコード画像（Base64 Data URI形式）
    /// </summary>
    public string QrCodeDataUri { get; set; } = string.Empty;

    /// <summary>
    /// 手動入力用の共有キー
    /// </summary>
    public string SharedKey { get; set; } = string.Empty;

    /// <summary>
    /// 確認コード入力用（6桁）
    /// </summary>
    [Required(ErrorMessage = "確認コードは必須です")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "確認コードは6桁で入力してください")]
    [Display(Name = "確認コード")]
    public string Code { get; set; } = string.Empty;
}
