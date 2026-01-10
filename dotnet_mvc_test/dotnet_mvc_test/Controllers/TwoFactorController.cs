using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using dotnet_mvc_test.Models.Entities;
using dotnet_mvc_test.Models.ViewModels.TwoFactor;
using dotnet_mvc_test.Services;

namespace dotnet_mvc_test.Controllers;

public class TwoFactorController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ILogger<TwoFactorController> _logger;

    public TwoFactorController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITwoFactorService twoFactorService,
        ILogger<TwoFactorController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _twoFactorService = twoFactorService;
        _logger = logger;
    }

    // GET: /TwoFactor/Setup
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("ユーザーが見つかりません。");
        }

        // AuthenticatorKeyを取得（なければリセットして新規生成）
        var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(authenticatorKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        // QRコード生成
        var email = await _userManager.GetEmailAsync(user);
        var qrCodeDataUri = _twoFactorService.GenerateQrCodeDataUri(email!, authenticatorKey!);

        var viewModel = new SetupTwoFactorViewModel
        {
            SharedKey = authenticatorKey!,
            QrCodeDataUri = qrCodeDataUri
        };

        return View(viewModel);
    }

    // POST: /TwoFactor/Setup
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(SetupTwoFactorViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("ユーザーが見つかりません。");
        }

        if (!ModelState.IsValid)
        {
            // QRコードを再生成
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            model.SharedKey = authenticatorKey!;
            model.QrCodeDataUri = _twoFactorService.GenerateQrCodeDataUri(email!, authenticatorKey!);
            return View(model);
        }

        // コードを検証
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            model.Code);

        if (!isValid)
        {
            ModelState.AddModelError(nameof(model.Code), "確認コードが無効です。");
            // QRコードを再生成
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            model.SharedKey = authenticatorKey!;
            model.QrCodeDataUri = _twoFactorService.GenerateQrCodeDataUri(email!, authenticatorKey!);
            return View(model);
        }

        // 2FAを有効化
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        _logger.LogInformation("ユーザー {UserId} が2段階認証を有効にしました。", user.Id);

        // リカバリーコード生成（10個）
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        TempData["RecoveryCodes"] = recoveryCodes!.ToArray();

        return RedirectToAction(nameof(RecoveryCodes));
    }

    // GET: /TwoFactor/RecoveryCodes
    [Authorize]
    [HttpGet]
    public IActionResult RecoveryCodes()
    {
        var recoveryCodes = TempData["RecoveryCodes"] as string[];
        if (recoveryCodes == null || recoveryCodes.Length == 0)
        {
            return RedirectToAction(nameof(Setup));
        }

        var viewModel = new RecoveryCodesViewModel
        {
            RecoveryCodes = recoveryCodes
        };

        return View(viewModel);
    }

    // GET: /TwoFactor/Verify
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Verify(string? returnUrl = null)
    {
        var viewModel = new VerifyTwoFactorViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/")
        };

        return View(viewModel);
    }

    // POST: /TwoFactor/Verify
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(VerifyTwoFactorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            model.Code,
            model.RememberMe,
            model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("ユーザーが2段階認証でログインしました。");
            return LocalRedirect(model.ReturnUrl ?? Url.Content("~/"));
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("ユーザーアカウントがロックアウトされました。");
            ModelState.AddModelError(string.Empty, "アカウントがロックされています。しばらくしてから再度お試しください。");
            return View(model);
        }

        ModelState.AddModelError(nameof(model.Code), "確認コードが無効です。");
        return View(model);
    }

    // GET: /TwoFactor/VerifyRecoveryCode
    [AllowAnonymous]
    [HttpGet]
    public IActionResult VerifyRecoveryCode(string? returnUrl = null)
    {
        var viewModel = new VerifyRecoveryCodeViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/")
        };

        return View(viewModel);
    }

    // POST: /TwoFactor/VerifyRecoveryCode
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyRecoveryCode(VerifyRecoveryCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.RecoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("ユーザーがリカバリーコードでログインしました。");
            return LocalRedirect(model.ReturnUrl ?? Url.Content("~/"));
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("ユーザーアカウントがロックアウトされました。");
            ModelState.AddModelError(string.Empty, "アカウントがロックされています。しばらくしてから再度お試しください。");
            return View(model);
        }

        ModelState.AddModelError(nameof(model.RecoveryCode), "リカバリーコードが無効です。");
        return View(model);
    }
}
