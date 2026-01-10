# 2段階認証（TOTP）実装ドキュメント

**作成日:** 2025-12-30  
**ステータス:** 実装完了（基本機能）  
**対象:** ASP.NET Core 9.0 MVC ブログアプリケーション

---

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

---

## 4. 未実装機能・課題

### 4.1 リカバリーコード再生成機能

**現状**
- 初回設定時のみリカバリーコードを表示
- ユーザーがリカバリーコードを紛失した場合、再生成する手段がない

**必要な実装**
- GET/POST /TwoFactor/RegenerateRecoveryCodes アクション
- UserManager.GenerateNewTwoFactorRecoveryCodesAsync()を呼び出し
- セキュリティ上、パスワード再入力またはメール確認が望ましい

### 4.2 2段階認証の無効化機能

**現状**
- 管理者は自身で2段階認証を無効化できない（設計通り）
- 一般ユーザーの無効化機能も未実装

**必要な実装（一般ユーザー向け）**
- GET/POST /TwoFactor/Disable アクション
- UserManager.SetTwoFactorEnabledAsync(false)
- UserManager.ResetAuthenticatorKeyAsync()で秘密鍵をリセット

**管理者に対する制約**
- 計画書では管理者は無効化不可とされているが、実際には制約コードが未実装
- RequireAdminTwoFactorフィルターのみで強制しているため、SetTwoFactorEnabledAsync(false)を直接呼び出せば無効化可能
- 必要であれば、UserManagerのカスタムバリデーターで管理者の無効化を禁止する実装を追加

### 4.3 UI/UX改善項目

**未実装の使い勝手向上機能**
- Verify画面でリカバリーコード入力への切り替えリンク（現在はVerifyRecoveryCodeへの直接リンクが必要）
- 2段階認証設定状態のユーザーダッシュボード表示（有効/無効の確認画面）
- RememberMachineによるデバイス記憶の動作確認（ブラウザCookie管理）

**表示上の課題**
- エラーメッセージが日本語化されているが、一部の汎用メッセージは英語のまま（Identity標準エラー）
- QRコードスキャン手順の説明が簡素（より詳細なステップバイステップガイドが望ましい）

### 4.4 セキュリティ強化の余地

**レート制限**
- 現状はASP.NET Identityのデフォルトロックアウト設定に依存
- 計画書では「連続5回失敗でロックアウト」とあるが、実際の設定は未確認（Program.csでlockoutOnFailure: falseとなっているため、現状ロックアウトは無効）

**推奨実装**
- AccountController.LoginおよびTwoFactorController.VerifyでlockoutOnFailure: trueに変更
- Program.csのIdentity設定でLockout.DefaultLockoutTimeSpanを設定（例: 5分間）

**その他の検討事項**
- QRコード生成時の秘密鍵の長さ・エントロピーの検証（Identity標準に依存）
- リカバリーコードのハッシュ化（Identity標準でハッシュ化済みのため対応不要）

### 4.5 テストカバレッジ

**現状**
- 手動テストのみ実施（計画書のテスト計画参照）
- 自動テスト（単体テスト、統合テスト）は未実装

**推奨テスト項目**
- TwoFactorServiceのQRコード生成ロジックの単体テスト
- RequireAdminTwoFactorAttributeの動作確認テスト（モックユーザーでの検証）
- 統合テスト：2段階認証設定から管理画面アクセスまでのE2Eフロー

---

## 5. 設計判断と代替案

### 5.1 QRコード生成方法

**採用した実装: サーバーサイドでQRCoder使用**

**メリット**
- セキュリティ: 秘密鍵がクライアントサイドJavaScriptに露出しない
- 実装が単純: .NETライブラリで完結、追加のフロントエンド依存不要
- パフォーマンス: 画像生成がサーバー側のため、ブラウザ性能に依存しない

**デメリット**
- サーバー負荷: リクエストごとにQRコード画像生成（ただし軽量処理）
- Base64埋め込み: HTMLサイズが若干増加（数KB程度）

**代替案A: クライアントサイドQRコード生成（qrcode.jsなど）**
- メリット: サーバー負荷軽減、リアルタイム生成
- デメリット: 秘密鍵がJavaScriptに露出（セキュリティリスク）、追加のフロントエンドライブラリ管理

**代替案B: QRコード画像をファイルシステムに保存**
- メリット: 再利用可能（キャッシュ）
- デメリット: ファイル管理が必要、セッションとの紐付けロジックが複雑化、セキュリティリスク（画像URLの漏洩）

**結論**
サーバーサイド生成が最もバランスが良い。秘密鍵の機密性とシンプルな実装を優先。

### 5.2 管理者2段階認証強制の実装方法

**採用した実装: ActionFilterAttribute（RequireAdminTwoFactorAttribute）**

**メリット**
- 粒度の細かい制御: コントローラー単位、アクション単位で適用可能
- 明示性: 属性によりどのコントローラーが2段階認証を要求するか一目瞭然
- 柔軟性: 特定アクションを除外する場合、Attributeを削除するだけ
- テスト容易性: フィルターの動作を単独でテスト可能

**デメリット**
- 適用漏れのリスク: 新しい管理コントローラーに属性を付け忘れる可能性
- 冗長性: 全ての管理コントローラーに属性を記述する必要

**代替案A: カスタムミドルウェア**
- メリット: /admin/*への全リクエストを一括処理、適用漏れなし
- デメリット: 粒度が粗い、特定アクションの除外が困難、ルーティング前後の配置に注意が必要

**代替案B: Authorize属性のカスタムPolicy**
- メリット: ASP.NET Core標準の認可機構に統合、宣言的
- デメリット: 2段階認証「未設定」へのリダイレクトという動作がAuthorize属性の標準動作と合わない（通常はAccessDeniedへリダイレクト）

**代替案C: AdminArea用のBaseController + OnActionExecuting**
- メリット: 全管理コントローラーがBaseControllerを継承すれば自動適用
- デメリット: 継承の強制、アーキテクチャ変更が大きい

**結論**
ActionFilterが最も適切。適用漏れリスクはコードレビューや命名規則（Admin配下のコントローラーは必須適用）でカバー可能。将来的にArea全体にグローバルフィルターとして登録する選択肢もある。

### 5.3 リカバリーコードの一時保存方法

**採用した実装: TempData**

**メリット**
- ASP.NET Core標準機能、追加の依存なし
- リダイレクト間でデータ引き継ぎ可能
- 一度読み取ると自動削除（セキュリティ上有利）

**デメリット**
- ページリロードで消失（ユーザーがブラウザバックや再読み込みすると再表示不可）
- セッション依存（サーバー再起動で消失）

**代替案A: ViewBagまたはViewData**
- メリット: 同一リクエスト内で使用可能
- デメリット: リダイレクトを跨げない（Setupから別アクションへのリダイレクトに対応不可）

**代替案B: データベースに一時保存**
- メリット: 永続化、再表示可能
- デメリット: オーバーエンジニアリング、セキュリティリスク（暗号化が必要）

**代替案C: SetupアクションでそのままRecoveryCodesビューを返す**
- メリット: TempData不要
- デメリット: PRG（Post-Redirect-Get）パターンに反する（ブラウザリロードでPOST再送信）

**結論**
TempDataが最適。リカバリーコードは一度だけ表示すべき機密情報であり、TempDataの「一度読み取ると削除」特性がセキュリティ要件に合致。ユーザーが保存し忘れた場合の再生成機能を別途提供することで解決。

### 5.4 認証コード検証の実装

**採用した実装: ASP.NET Identity標準のSignInManager**

**メリット**
- 標準機能: TOTP検証ロジックが実装済み、バグのリスクが低い
- セキュリティ: タイムウィンドウ、リプレイ攻撃対策が組み込み済み
- 保守性: Identityのアップデートで自動的に改善

**デメリット**
- カスタマイズ制約: 標準動作から外れる要件（例: 10桁コード）は実装困難
- ブラックボックス: 内部ロジックが隠蔽されている

**代替案: 独自のTOTPライブラリ（OtpNetなど）を使用**
- メリット: 完全なカスタマイズ可能、コード長やタイムウィンドウを自由に設定
- デメリット: セキュリティリスク（実装ミスの可能性）、保守負担

**結論**
標準のSignInManagerを使用することで、セキュリティと保守性を優先。TOTPの仕様はRFC 6238に準拠しており、特殊な要件がない限りカスタマイズ不要。

---

## 6. 今後の拡張性

### 6.1 一般ユーザーへの2段階認証任意設定

**現状**
- 管理者のみ強制、一般ユーザーは未対応

**拡張方針**
- ユーザー設定画面に「2段階認証を有効にする」トグルを追加
- RequireAdminTwoFactorAttributeの適用対象を変更せず、任意設定用の別UIを提供
- 既存のTwoFactorControllerを再利用可能（Setup/Verify/RecoveryCodesは共通）

### 6.2 代替認証手段の追加

**SMS/メール認証**
- 計画書でスコープ外とされているが、技術的にはASP.NET IdentityのIUserTwoFactorTokenProviderで実装可能
- 外部SMSサービス（Twilio、AWS SNSなど）との連携が必要

**ハードウェアキー（FIDO2/WebAuthn）**
- より高度なセキュリティ
- ブラウザAPIとの連携、追加のNuGetパッケージ（Fido2NetLibなど）が必要
- モダンブラウザのみ対応

### 6.3 管理者によるユーザー2段階認証管理

**想定機能**
- 管理画面で全ユーザーの2段階認証有効/無効状態を一覧表示
- 特定ユーザーの2段階認証を強制リセット（秘密鍵リセット + リカバリーコード無効化）
- 監査ログ（2段階認証の設定変更履歴）

**実装要件**
- Admin/UserManagementController の追加
- UserManagerを使用したバルク操作
- セキュリティ配慮（管理者のみアクセス可能、操作ログの記録）

### 6.4 信頼済みデバイス管理

**現状**
- RememberMachine機能は実装済みだが、ユーザーが信頼済みデバイスを確認・削除する手段がない

**拡張方針**
- ユーザー設定画面に「信頼済みデバイス一覧」を表示
- AspNetUserTokensテーブルから該当トークンを取得
- 削除機能の実装（UserManager.RemoveAuthenticationTokenAsync）

### 6.5 多言語対応

**現状**
- UIは日本語のみ、エラーメッセージも日本語（一部Identity標準メッセージは英語）

**拡張方針**
- リソースファイル（.resx）を使用した多言語化
- Identity UIのローカライゼーションオプション設定
- Accept-Languageヘッダーまたはユーザー設定で言語切り替え

---

## 7. 実装参考情報

### 7.1 otpauth URI形式
TwoFactorService.GenerateQrCodeDataUriで生成されるURI形式：
```
otpauth://totp/{Issuer}:{Email}?secret={Key}&issuer={Issuer}
```
- Issuer: アプリケーション名（"DotNetMVCTest"）
- Email: ユーザーのメールアドレス
- Key: Base32エンコードされた秘密鍵（Identity内部で生成）

この形式はGoogle Authenticator Keyの標準フォーマットに準拠。

### 7.2 ASP.NET Identityの関連テーブル

**AspNetUsers**
- TwoFactorEnabled: 2段階認証の有効/無効フラグ

**AspNetUserTokens**
- LoginProvider: "AuthenticatorKey"
- Name: TokenName
- Value: 秘密鍵（暗号化保存）

リカバリーコードもAspNetUserTokensに保存されるが、ハッシュ化されている。

### 7.3 主要なIdentity APIメソッド

**秘密鍵管理**
- GetAuthenticatorKeyAsync(user): 秘密鍵取得（なければnull）
- ResetAuthenticatorKeyAsync(user): 秘密鍵リセット・新規生成

**2段階認証制御**
- SetTwoFactorEnabledAsync(user, enabled): 2段階認証の有効/無効切り替え
- VerifyTwoFactorTokenAsync(user, provider, token): TOTPトークン検証

**リカバリーコード**
- GenerateNewTwoFactorRecoveryCodesAsync(user, count): 新規リカバリーコード生成
- RedeemTwoFactorRecoveryCodeAsync(user, code): リカバリーコード使用（自動無効化）
- CountRecoveryCodesAsync(user): 残りのリカバリーコード数取得

**サインイン**
- PasswordSignInAsync(...): メール/パスワード認証（result.RequiresTwoFactorでチェック）
- TwoFactorAuthenticatorSignInAsync(code, rememberMe, rememberMachine): TOTP検証ログイン
- TwoFactorRecoveryCodeSignInAsync(code): リカバリーコードでログイン

---

## 8. トラブルシューティング

### 8.1 QRコードがスキャンできない

**原因**
- QRCoderのバージョン不一致
- 秘密鍵が正しくBase32エンコードされていない
- otpauth URIの形式エラー

**確認方法**
- Setup.cshtmlで手動入力用キーが正しく表示されているか確認
- ブラウザ開発者ツールでimgタグのsrc属性を確認（data:image/png;base64,で始まるか）

### 8.2 確認コードが常に無効と表示される

**原因**
- サーバーの時刻がずれている（TOTPは時刻ベース）
- 秘密鍵が正しく保存されていない

**確認方法**
- サーバーのシステム時刻を確認（NTPで同期推奨）
- AspNetUserTokensテーブルでAuthenticatorKeyが保存されているか確認

### 8.3 リカバリーコードが表示されない

**原因**
- TempDataが正しく渡されていない
- セッション設定の問題

**確認方法**
- Setup.cshtmlからRecoveryCodes.cshtmlへのリダイレクトが正しく行われているか
- Program.csでセッション設定が有効か確認（デフォルトで有効）

### 8.4 管理画面にアクセスしてもSetupへリダイレクトされない

**原因**
- RequireAdminTwoFactor属性が適用されていない
- ユーザーがAdministratorロールでない

**確認方法**
- DashboardController/ArticleControllerに属性が記載されているか
- AspNetUserRolesテーブルで該当ユーザーのロールIDを確認

---

## 9. セキュリティ考慮事項

### 9.1 実装済みセキュリティ対策

**秘密鍵の保護**
- ASP.NET Identityのトークンストレージ機能で暗号化保存
- QRコード生成時もサーバーサイドで処理、クライアント露出なし

**リカバリーコードの保護**
- AspNetUserTokensにハッシュ化して保存
- 一度使用したコードは自動的に無効化

**TOTP標準準拠**
- RFC 6238に準拠した実装（Identity標準機能）
- タイムウィンドウによる時刻誤差吸収
- リプレイ攻撃対策

### 9.2 推奨されるさらなる対策

**レート制限の有効化**
- 現状、AccountController.LoginでlockoutOnFailure: falseとなっている
- lockoutOnFailure: trueに変更し、Lockout設定をProgram.csで調整（DefaultLockoutTimeSpan、MaxFailedAccessAttemptsなど）

**監査ログの記録**
- 2段階認証の設定変更、無効化、リカバリーコード使用などのイベントをログに記録
- セキュリティインシデント調査に活用

**HTTPSの強制**
- 既にProgram.csでUseHttpsRedirection()が有効
- 本番環境ではHSTSヘッダーも有効（UseHsts）

**秘密鍵のローテーション**
- ユーザーが定期的に秘密鍵をリセットできる機能の提供
- 管理者による強制リセット機能

---

## 10. まとめ

### 10.1 実装の完成度

**完成している部分**
- 基本的な2段階認証フロー（設定 → ログイン → 検証）
- QRコード生成とTOTP検証
- リカバリーコード発行
- 管理者への強制適用

**不完全な部分**
- リカバリーコードの再生成機能
- 2段階認証の無効化機能（一般ユーザー向け）
- レート制限の本格的な適用
- ユーザー向け管理UI（設定状態確認、デバイス管理）

### 10.2 推奨される次のステップ

**優先度: 高**
1. レート制限の有効化（lockoutOnFailure: true + Lockout設定）
2. リカバリーコード再生成機能の実装
3. エラーハンドリングと日本語化の完全対応

**優先度: 中**
4. 一般ユーザー向け2段階認証任意設定UI
5. 自動テストの追加（単体テスト、統合テスト）
6. 管理者による2段階認証管理機能

**優先度: 低**
7. 代替認証手段（SMS、FIDO2）の検討
8. 多言語対応
9. 監査ログの充実

### 10.3 技術的評価

**良い設計判断**
- ASP.NET Identity標準機能を最大限活用（車輪の再発明を避けた）
- ActionFilterによる柔軟で明示的な強制適用
- サーバーサイドQRコード生成によるセキュリティ確保

**改善の余地**
- レート制限が無効なのはセキュリティ上の課題（早期対応推奨）
- TempDataへの依存がユーザー体験を制限（リカバリーコード再表示機能で解決）
- テストカバレッジの不足

**総合評価**
基本機能は堅実に実装されており、TOTP標準に準拠したセキュアな2段階認証が動作する。管理者強制機能により、プロジェクトの主目的は達成されている。今後は運用面での利便性向上（リカバリーコード管理、デバイス管理）とセキュリティ強化（レート制限、監査ログ）に注力すべき。
