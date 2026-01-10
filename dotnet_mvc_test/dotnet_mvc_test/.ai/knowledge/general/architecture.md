# アーキテクチャドキュメント

## 概要

このアプリケーションは、ASP.NET Core 9.0 MVCをベースにしたブログ管理システムで、**レイヤードアーキテクチャ**と**インターフェースベース設計**を採用しています。管理者向けの機能は**Area分離パターン**により隔離され、セキュリティと保守性を高めています。

## アーキテクチャ全体像

```
┌─────────────────────────────────────────────────────────────┐
│                    HTTP Request Pipeline                     │
├─────────────────────────────────────────────────────────────┤
│  HTTPS Redirection → Routing → Authentication → Authorization│
└──────────────────────────┬──────────────────────────────────┘
                           │
        ┌──────────────────┴──────────────────┐
        │                                      │
┌───────▼────────┐                   ┌────────▼────────┐
│  Public Area   │                   │   Admin Area    │
│  (Home, etc.)  │                   │  (Dashboard,    │
│                │                   │   Articles)     │
└───────┬────────┘                   └────────┬────────┘
        │                                     │
        └──────────────┬──────────────────────┘
                       │
        ┌──────────────▼──────────────┐
        │     Controller Layer        │
        │   (MVC Controllers)         │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │     Service Layer           │
        │  (Business Logic)           │
        │  • IArticleService          │
        │  • ICategoryService         │
        │  • ITagService              │
        │  • IMarkdownService         │
        │  • ITwoFactorService        │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │   Data Access Layer         │
        │  (ApplicationDbContext)     │
        │  + Entity Framework Core    │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │      SQLite Database        │
        │        (blog.db)            │
        └─────────────────────────────┘
```

## レイヤー構成と責務

### 1. プレゼンテーション層 (Presentation Layer)
**責務:** HTTPリクエスト処理、ビジネスロジック層への委譲、ViewModel/エンティティマッピング、入力検証
**構成要素:** Controllers、Razor Views、ViewModels (DTO)
**設計パターン:** MVC、ViewModel Pattern、Action Filter Pattern (RequireAdminTwoFactorAttribute)

### 2. ビジネスロジック層 (Business Logic Layer)
**責務:** ドメインロジック、データ検証、ビジネスルール適用、トランザクション管理
**構成要素:** Service Interfaces (IArticleService等)、Service Implementations
**設計パターン:** Repository-like Pattern、Dependency Injection、Eager Loading (Include/ThenInclude)、Async/Await

### 3. データアクセス層 (Data Access Layer)
**責務:** DBスキーマ定義、エンティティ永続化、クエリ実行、制約管理
**構成要素:** ApplicationDbContext (IdentityDbContext継承)、Domain Entities、Fluent API Configurations
**設計パターン:** Unit of Work (暗黙的)、Identity Pattern、Soft Delete Pattern (準備済み)、Slug-Based URL

## エンティティ関係図

```
┌─────────────────┐
│ ApplicationUser │ (ASP.NET Identity)
└────────┬────────┘
         │ 1
         │
         │ many
         ▼
    ┌────────┐     many      ┌───────────┐    many     ┌──────┐
    │Article │◄──────────────┤ArticleTag ├────────────►│ Tag  │
    └───┬────┘               └───────────┘             └──────┘
        │
        │ many
        │
        ▼ 1
   ┌──────────┐
   │ Category │
   └──────────┘
        ▲
        │ 1
        │
        │ many
   ┌─────────┐
   │ Comment │
   └─────────┘
```

**主要リレーションシップ:**

| From Entity    | To Entity      | Relationship | Delete Behavior |
|----------------|----------------|--------------|-----------------|
| Article        | Category       | Many-to-One  | Restrict        |
| Article        | ApplicationUser| Many-to-One  | Restrict        |
| Article        | Tag            | Many-to-Many | Cascade (via join) |
| Article        | Comment        | One-to-Many  | Cascade         |

**設計理由:** Restrict (Category/Author) = 整合性保証、Cascade (ArticleTag/Comment) = 孤児レコード防止

## セキュリティアーキテクチャ

### 認証・認可の多層防御

```
HTTP Request
    │
    ▼
┌───────────────────────────────┐
│ UseAuthentication Middleware  │  → HttpContext.User を設定
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│ UseAuthorization Middleware   │  → ポリシー/ロールチェック
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│ [Authorize(Roles="Admin")]    │  → ロールベース認可
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│ [RequireAdminTwoFactor]       │  → カスタム2FA強制
└───────────────┬───────────────┘
                │
                ▼
        Controller Action
```

**ASP.NET Core Identity統合:** ApplicationUser (extends IdentityUser)、パスワードポリシー (6文字以上/大小文字数字必須)、ロール (Administrator/User)

### カスタム認可フィルター
**RequireAdminTwoFactorAttribute:** ActionFilterAttribute継承、管理者2FA強制、未設定時/TwoFactor/Setupへリダイレクト

**多層防御の利点:** 単一障害点排除、Identity活用、拡張可能性、2FA管理者保護

## Area分離パターン

### 物理的構造

```
Controllers/
├── HomeController.cs          (Public)
├── AccountController.cs       (Public)
└── Admin/
    ├── ArticleController.cs   (Admin Only)
    └── DashboardController.cs (Admin Only)

Areas/
└── Admin/
    └── Views/
        ├── Article/
        │   ├── Index.cshtml
        │   ├── Create.cshtml
        │   └── Edit.cshtml
        └── Dashboard/
            └── Index.cshtml

Models/ViewModels/
├── Account/
│   └── LoginViewModel.cs
└── Admin/
    ├── ArticleCreateViewModel.cs
    └── ArticleEditViewModel.cs
```

### ルーティング戦略
```
Priority 1: {area:exists}/{controller=Dashboard}/{action=Index}/{id?}
Priority 2: {controller=Home}/{action=Index}/{id?}
```

**Areaパターンの利点:** 責任分離、独立認可、ビュー階層分離、将来拡張容易 (API/UserDashboard等)

## 採用された設計パターン

### 1. Dependency Injection (DI)
**実装:** ASP.NET Core組み込みDIコンテナ
**ライフタイム:** Scoped (サービス層/DbContext)、Transient (未使用)、Singleton (IConfiguration)
**利点:** テスト容易性、疎結合、自動ライフタイム管理

### 2. Repository-like Pattern
**実装:** サービス層がRepository役割
**特徴:** DbContext直接使用回避、CRUD/ビジネスロジックをサービス層にカプセル化
**差異:** 汎用`IRepository<T>`未実装、エンティティ特化型インターフェース

### 3. Unit of Work (暗黙的)
**実装:** DbContextがUnit of Work機能
**動作:** Change Tracking、SaveChangesAsync()一括コミット、暗黙的トランザクション
**トレードオフ:** シンプルだが複数DbContext間の調整困難

### 4. ViewModel (DTO) Pattern
**命名:** `{Entity}{Create|Edit|List|Detail}ViewModel`
**利点:** 必要データのみ露出、検証分離、エンティティ変更の影響遮断

### 5. Eager Loading Pattern
**実装:** Include/ThenInclude一貫使用
```
Articles.Include(a => a.Author).Include(a => a.Category)
  .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
```
**理由:** N+1回避、パフォーマンス予測可能、Lazy Loading不使用

### 6. Action Filter Pattern
**実装:** RequireAdminTwoFactorAttribute (ActionFilterAttribute継承)
**適用:** クラスレベル/メソッドレベル
**拡張性:** ロギング、キャッシング、レート制限等に応用可能

### 7. Slug-Based URL Pattern
**実装:** Slugプロパティ (`^[a-z0-9\-]+$`)、Uniqueインデックス、重複チェック
**利点:** SEO対応、可読性、ID非露出

### 8. Soft Delete Pattern (準備済み)
**現状:** IsDeletedフラグ定義済みだがハードデリート実装
**技術的負債:** サービス層での論理削除実装が必要

## データフロー

### 典型的なリクエストフロー（記事作成）
```
HTTP POST → Routing → Authentication → Authorization → 
[Authorize] → [RequireAdminTwoFactor] → Model Binding/Validation →
Controller.Create → Service.CreateArticleAsync → DbContext.Add →
SaveChangesAsync → TempData設定 → RedirectToAction
```

### クエリ実行フロー（タグ付き記事取得）
```
Controller → Service.GetByIdAsync(id) → DbContext.Articles
  .Include(Author/Category/ArticleTags.Tag).FirstOrDefaultAsync
→ EF Core (SQL生成/JOIN) → SQLite (実行) → 
Entity Materialization → Service (返却) → 
Controller (ViewModelマッピング) → View (HTML生成)
```

## 拡張性の考慮点

### 現在の設計の強み
1. **インターフェースベース設計:** モック化容易、実装差し替え可能、依存関係明示
2. **Areaパターン:** Area追加容易 (Api/UserDashboard等)、機能独立性、並行作業性
3. **サービス層抽象化:** データアクセス戦略変更の影響遮断、ビジネスロジック集中、トランザクション境界明確
4. **Fluent API設定:** スキーマ一元管理、マイグレーション容易、エンティティ純粋性維持
5. **ViewModelパターン:** プレゼンテーション/ドメイン分離、Over-posting防止、柔軟なバージョニング

### 将来的な改善案

#### 1. 明示的なRepositoryパターン
**理由:** CRUD重複削減、標準化、テスト容易性
**実装:** `IRepository<T>` (GetByIdAsync/GetAllAsync/AddAsync/UpdateAsync/DeleteAsync) + エンティティ特化型

#### 2. CQRS (Command Query Responsibility Segregation)
**理由:** 読み書き最適化分離、パフォーマンス向上、イベントソーシング発展可能性
**実装:** Queries (Dapper等でDTO直接取得) / Commands (EF Core変更追跡)

#### 3. Specification Pattern
**理由:** クエリロジック再利用、ビジネスルールカプセル化
**実装:** PublishedArticles/NotDeleted/ArticlesByCategory Specification (組み合わせ可能)

#### 4. キャッシング戦略
**対象:** カテゴリ/タグ一覧 (変更頻度低)、記事一覧 (時間ベース無効化)
**手段:** IMemoryCache (単一サーバー) / IDistributedCache / Redis (分散)

#### 5. イベント駆動アーキテクチャ
**ユースケース:** 記事公開/コメント投稿通知、監査ログ
**実装:** Domain Events (ArticlePublished/CommentCreated等) + Event Handlers

#### 6. API Layer追加
**実装:** Areas/Api新設、RESTful設計、JWT認証、Swagger統合、既存サービス層再利用

#### 7. ソフトデリート実装
**変更:** DeleteAsync()をIsDeleted=true設定に変更、Global Query Filter追加

### 現在の技術的負債
1. **DashboardControllerの直接DbContext使用** → IDashboardServiceの導入
2. **ハードデリート実装** → ソフトデリートへ移行
3. **明示的トランザクション管理なし** → IDbContextTransaction活用
4. **TempDataによるメッセージ伝達** → FlashMessageサービス
5. **例外ハンドリングの分散** → Global Exception Handling Middleware

## まとめ

本アーキテクチャは**レイヤードアーキテクチャ**をベースに、**MVC**、**DI**、**Repository-like**パターンを組み合わせた実用的設計。

**主な設計思想:** 関心の分離、疎結合、高凝集、テスタビリティ、拡張性

**適用シナリオ:** 中規模CMS、権限分離が必要なアプリ、EF Core活用RDBMSシステム

**非推奨シナリオ:** 大規模分散システム (マイクロサービス)、超高スループット要求 (キャッシング必須)、リアルタイム重視 (イベント駆動適切)

将来拡張 (CQRS/イベント駆動/API化) への道筋が明確で、段階的成長が可能な設計。
