# Two-Factor Authentication (2FA) Documentation

## 1. 概要

### 1.1 実装目的
管理者アカウントに対してTOTP（Time-based One-Time Password）ベースの2段階認証を強制し、管理画面へのアクセスセキュリティを強化する。

### 1.2 対象ユーザー
- **Administrator ロール保持者:** 2段階認証が必須
- **User ロール:** 2段階認証は任意（現状未実装）

### 1.3 現在の実装状態
基本的な2段階認証機能は実装済み。管理者向け強制設定、QRコード生成、認証コード検証、リカバリーコード発行の各フローが動作する。一部、ユーザー体験の最適化や管理機能（リカバリーコード再生成など）が未実装。

---

## 2. アーキテクチャ

### 2.1 技術スタック

| 項目 | 選択技術 | 理由 |
|------|----------|------|
| 認証方式 | TOTP (RFC 6238) | Google Authenticator、Microsoft Authenticator等で広くサポート |
| QRコード生成 | QRCoder v1.6.0 | .NET対応、軽量、MITライセンス |
| 秘密鍵管理 | ASP.NET Identity標準機能 | UserManager.GetAuthenticatorKeyAsync()を使用、既存のIdentityインフラを活用 |
| データ保存 | AspNetUsers, AspNetUserTokens | マイグレーション不要、既存のIdentityスキーマを利用 |

### 2.2 コンポーネント構成

実装されたファイル構造は以下の通り：

**コントローラー層**
- Controllers/TwoFactorController.cs（219行）- 2段階認証の設定・検証を担当
- Controllers/AccountController.cs - ログインフロー内で2段階認証を統合（RequiresTwoFactorハンドリング）

**サービス層**
- Services/ITwoFactorService.cs - QRコード生成とキーフォーマットのインターフェース
- Services/TwoFactorService.cs - QRCoderを使用したQRコード生成とotpauth URI構築の実装

**フィルター層**
- Filters/RequireAdminTwoFactorAttribute.cs - 管理者ロールに対して2段階認証未設定時に設定ページへリダイレクトするActionFilter

**ビューモデル層**
- Models/ViewModels/TwoFactor/SetupTwoFactorViewModel.cs - 2段階認証設定画面用（QRコード表示と確認コード入力）
- Models/ViewModels/TwoFactor/VerifyTwoFactorViewModel.cs - ログイン時の確認コード入力用
- Models/ViewModels/TwoFactor/RecoveryCodesViewModel.cs - リカバリーコード表示用
- Models/ViewModels/TwoFactor/VerifyRecoveryCodeViewModel.cs - リカバリーコードでのログイン用

**ビュー層**
- Views/TwoFactor/Setup.cshtml - QRコード表示と初期設定
- Views/TwoFactor/Verify.cshtml - ログイン時のコード入力
- Views/TwoFactor/RecoveryCodes.cshtml - リカバリーコード表示
- Views/TwoFactor/VerifyRecoveryCode.cshtml - リカバリーコードでのログイン

**依存性注入設定**
- Program.cs: ITwoFactorServiceの登録（Scoped）

**適用対象**
- Controllers/Admin/DashboardController.cs - RequireAdminTwoFactor属性適用
- Controllers/Admin/ArticleController.cs - RequireAdminTwoFactor属性適用

### 2.3 データフロー

**2段階認証設定フロー**
```
[管理者ログイン] 
    ↓
[RequireAdminTwoFactorフィルター実行]
    ↓
[2FA未設定？] → Yes → [/TwoFactor/Setup へリダイレクト]
    ↓ No                       ↓
[管理画面表示]          [UserManager.GetAuthenticatorKeyAsync()] - 秘密鍵取得または生成
                               ↓
                        [TwoFactorService.GenerateQrCodeDataUri()] - otpauth URI生成 + QRコード画像化
                               ↓
                        [Setup.cshtmlでQRコード表示]
                               ↓
                        [ユーザーが認証アプリでスキャン]
                               ↓
                        [6桁コード入力 → POST /TwoFactor/Setup]
                               ↓
                        [UserManager.VerifyTwoFactorTokenAsync()] - コード検証
                               ↓ 成功
                        [UserManager.SetTwoFactorEnabledAsync(true)] - 2FA有効化
                               ↓
                        [UserManager.GenerateNewTwoFactorRecoveryCodesAsync(10)] - リカバリーコード生成
                               ↓
                        [RecoveryCodes.cshtml表示] - ユーザーに保存を促す
```

**2段階認証ログインフロー**
```
[メール/パスワード入力 → POST /Account/Login]
    ↓
[SignInManager.PasswordSignInAsync()]
    ↓
[result.RequiresTwoFactor = true?] → No → [通常ログイン完了]
    ↓ Yes
[Redirect to /TwoFactor/Verify]
    ↓
[6桁コード入力 → POST /TwoFactor/Verify]
    ↓
[SignInManager.TwoFactorAuthenticatorSignInAsync()] - 認証コード検証
    ↓ 成功
[ログイン完了 → returnUrlへリダイレクト]

※リカバリーコード使用時
[Verify画面でリカバリーコードリンククリック]
    ↓
[/TwoFactor/VerifyRecoveryCode]
    ↓
[POST] → [SignInManager.TwoFactorRecoveryCodeSignInAsync()] - コード検証（使用後無効化）
    ↓ 成功
[ログイン完了]
```

---

## 3. 実装済み機能

### 3.1 QRコード生成（ITwoFactorService / TwoFactorService）

**実装内容**
- otpauth URI形式の文字列を生成（Issuer: "DotNetMVCTest"）
- QRCoderライブラリでPNG画像化し、Base64 Data URIとして返却
- 手動入力用に秘密鍵を4文字ごとにスペース区切りでフォーマット

**実装方法の特徴**
- サーバーサイドでQRコード生成（クライアントサイドJavaScriptは不使用）
- Data URIとして直接imgタグに埋め込み、外部ファイル保存不要
- セッションやキャッシュに依存せず、リクエストごとに生成

### 3.2 2段階認証設定（TwoFactorController.Setup）

**実装内容**
- GET /TwoFactor/Setup: QRコード表示、手動入力用キー表示
- POST /TwoFactor/Setup: 入力された6桁コードを検証
  - UserManager.VerifyTwoFactorTokenAsync()でTOTPトークンの正当性を確認
  - 成功時、UserManager.SetTwoFactorEnabledAsync(true)で2段階認証を有効化
  - 10個のリカバリーコードを生成し、TempDataに保存してRecoveryCodesへリダイレクト

**検証ロジック**
- ASP.NET Identityのデフォルトトークンプロバイダーを使用
- TOTP標準（30秒間隔、6桁）に準拠

### 3.3 リカバリーコード表示（TwoFactorController.RecoveryCodes）

**実装内容**
- TempDataからリカバリーコード配列を取得し表示
- ユーザーに安全な場所への保存を促すUI

**制約**
- TempData経由のため、ページリロードで消失
- リカバリーコード再表示機能は未実装（後述の課題参照）

### 3.4 ログイン統合（AccountController.Login）

**実装内容**
- SignInManager.PasswordSignInAsync()の結果をチェック
- result.RequiresTwoFactorがtrueの場合、/TwoFactor/Verifyへリダイレクト
- 通常のメール/パスワード認証に加えて2段階認証が必要なフローを実現

### 3.5 認証コード検証（TwoFactorController.Verify）

**実装内容**
- GET /TwoFactor/Verify: 6桁コード入力フォーム表示
- POST /TwoFactor/Verify: SignInManager.TwoFactorAuthenticatorSignInAsync()で検証
  - RememberMe（セッション維持）とRememberMachine（デバイス記憶）オプション対応
  - 成功時、returnUrlへリダイレクト
  - 失敗時、エラーメッセージ表示（ModelStateにエラー追加）

**セキュリティ機能**
- 連続失敗時のロックアウト（ASP.NET Identityのデフォルト設定に依存）

### 3.6 リカバリーコードでのログイン（TwoFactorController.VerifyRecoveryCode）

**実装内容**
- GET /TwoFactor/VerifyRecoveryCode: リカバリーコード入力フォーム
- POST: SignInManager.TwoFactorRecoveryCodeSignInAsync()で検証
  - 成功時、使用済みリカバリーコードは自動的に無効化される（Identity標準動作）

### 3.7 管理者2段階認証強制（RequireAdminTwoFactorAttribute）

**実装内容**
- ActionFilterAttributeを継承したカスタムフィルター
- OnActionExecutionAsync()で以下をチェック：
  1. 現在のユーザーがログイン済みか
  2. Administratorロールを持つか
  3. TwoFactorEnabledがfalseか
- 上記すべてに該当する場合、/TwoFactor/Setupへリダイレクト（returnUrlに現在のパスを設定）

**適用箇所**
- DashboardController（クラスレベル）
- ArticleController（クラスレベル）

**設計判断**
- ミドルウェアではなくActionFilterを採用した理由：
  - コントローラー単位での細かい制御が可能
  - 属性により強制適用が明示的でコードの可読性が高い
  - 将来的に特定アクションのみ除外する柔軟性を確保