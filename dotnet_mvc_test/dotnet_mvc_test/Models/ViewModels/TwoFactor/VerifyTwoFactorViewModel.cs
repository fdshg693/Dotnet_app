using System.ComponentModel.DataAnnotations;

namespace dotnet_mvc_test.Models.ViewModels.TwoFactor;

public class VerifyTwoFactorViewModel
{
    /// <summary>
    /// 確認コード（6桁）
    /// </summary>
    [Required(ErrorMessage = "確認コードは必須です")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "確認コードは6桁で入力してください")]
    [Display(Name = "確認コード")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// このブラウザを記憶する
    /// </summary>
    [Display(Name = "このブラウザを記憶する")]
    public bool RememberMe { get; set; }

    /// <summary>
    /// このマシンを記憶する（2FA入力をスキップ）
    /// </summary>
    [Display(Name = "このデバイスを記憶する")]
    public bool RememberMachine { get; set; }

    /// <summary>
    /// 認証後のリダイレクト先URL
    /// </summary>
    public string? ReturnUrl { get; set; }
}
