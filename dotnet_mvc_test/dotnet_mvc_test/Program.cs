using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using dotnet_mvc_test.Data;
using dotnet_mvc_test.Models.Entities;
using dotnet_mvc_test.Services;
using dotnet_mvc_test.Repositories;

// 以下のような順序で設定を読み込む
// 1. appsettings.json
// 2. appsettings.{Environment}.json (例: appsettings.Development.json)
// 3. ユーザーシークレット (開発環境のみ)
// 4. 環境変数
// 5. コマンドライン引数
// build時にIConfigurationとしてDIコンテナに登録される
var builder = WebApplication.CreateBuilder(args);

// サービスコンテナへの登録
// データベース接続文字列の取得
// appsettings.jsonから"DefaultConnection"を読み込み、存在しない場合はデフォルトのSQLiteファイルパスを使用
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=blog.db";

// AddDbContext: Entity Framework Coreのデータベースコンテキストをサービスコンテナに登録
// - ApplicationDbContextをDI(依存性注入)で利用可能にする
// - スコープドライフタイム: HTTPリクエストごとに新しいインスタンスが作成される
// - UseSqlite: データベースプロバイダーとしてSQLiteを使用
// - 接続文字列を使用してblog.dbファイルに接続
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// AddIdentity: ASP.NET Core Identityの認証・認可システムをセットアップ
// - ApplicationUser: カスタムユーザーエンティティ(IdentityUserを継承)
// - IdentityRole: 標準のロールエンティティ
// 主な機能:
// - ユーザー登録、ログイン、ログアウト
// - パスワードのハッシュ化と検証
// - ロールベースの認可
// - クッキーベースの認証
// - 二要素認証のサポート
// - アカウントロックアウト機能
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // サインイン設定: メール確認なしでログイン可能にする(開発環境向け)
    options.SignIn.RequireConfirmedAccount = false;
    
    // パスワードポリシー設定
    options.Password.RequireDigit = true;              // 数字を必須にする
    options.Password.RequireLowercase = true;          // 小文字を必須にする
    options.Password.RequireUppercase = true;          // 大文字を必須にする
    options.Password.RequireNonAlphanumeric = false;   // 特殊文字を不要にする
    options.Password.RequiredLength = 6;               // 最小6文字
})
.AddEntityFrameworkStores<ApplicationDbContext>()  // Entity Framework CoreでIdentityデータを保存
.AddDefaultTokenProviders();                        // パスワードリセット、メール確認などのトークン生成機能を追加

// Repository の登録
// AddScoped: HTTPリクエストごとに1つのインスタンスが作成され、リクエスト内で共有される
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();     // 記事データアクセス
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();   // カテゴリデータアクセス
builder.Services.AddScoped<ITagRepository, TagRepository>();             // タグデータアクセス

// アプリケーション固有のサービスをDIコンテナに登録
// AddScoped: HTTPリクエストごとに1つのインスタンスが作成され、リクエスト内で共有される
// インターフェースと実装クラスのペアで登録することで、疎結合な設計を実現
builder.Services.AddScoped<IArticleService, ArticleService>();           // 記事管理機能
builder.Services.AddScoped<ICategoryService, CategoryService>();         // カテゴリ管理機能
builder.Services.AddScoped<ITagService, TagService>();                   // タグ管理機能
builder.Services.AddScoped<IMarkdownService, MarkdownService>();         // Markdown処理機能
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();       // 二要素認証機能

// AddControllersWithViews: MVCフレームワークをサービスに追加
// - コントローラーとビューの両方をサポート
// - ルーティング、モデルバインディング、検証機能を含む
// - Razorビューエンジンを有効化
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTPリクエストパイプラインの設定
// ミドルウェアは登録順に実行される(重要: 順序が動作に影響する)

if (!app.Environment.IsDevelopment())
{
    // 本番環境: 例外が発生した場合、エラーページにリダイレクト
    app.UseExceptionHandler("/Home/Error");
    
    // UseHsts: HTTP Strict Transport Security (HSTS)ヘッダーを追加
    // ブラウザに対してHTTPSのみでアクセスするよう指示(セキュリティ強化)
    // デフォルト30日間、本番環境では長期間(例: 365日)への変更を検討
    app.UseHsts();
}

// UseHttpsRedirection: HTTPリクエストをHTTPSにリダイレクト
// セキュアな通信を強制し、中間者攻撃を防ぐ
app.UseHttpsRedirection();

// UseStaticFiles: 静的ファイル(CSS, JS, 画像など)の配信を有効化
// wwwrootフォルダ内のファイルを直接HTTPリクエストで取得可能にする
// MapStaticAssetsと併用することで、より広範な静的ファイルサポートを実現
app.UseStaticFiles();

// UseRouting: リクエストURLを解析し、適切なエンドポイントにルーティング
// この後のミドルウェアでルート情報が利用可能になる
app.UseRouting();

// UseAuthentication: 認証ミドルウェアを有効化
// - クッキーからユーザー情報を読み取り、HttpContext.Userを設定
// - ログイン状態を判定
// - UseAuthorizationの前に配置する必要がある
app.UseAuthentication();

// UseAuthorization: 認可ミドルウェアを有効化
// - [Authorize]属性やロール要件をチェック
// - 認証済みユーザーがリソースへのアクセス権限を持つか判定
// - 権限がない場合、403 Forbiddenまたはログインページへリダイレクト
app.UseAuthorization();

// MapStaticAssets: 静的ファイル(CSS, JS, 画像など)の配信を有効化
// wwwrootフォルダ内のファイルにアクセス可能にする
app.MapStaticAssets();

// ルートマッピング: URLパターンとコントローラー/アクションの対応関係を定義
// 注意: より具体的なルート（パラメータが少ない）を先に定義する

// 記事一覧用ルート
// パターン: /articles
// 例: /articles → ArticlesController.Index()
app.MapControllerRoute(
    name: "articles",
    pattern: "articles",
    defaults: new { controller = "Articles", action = "Index" });

// 記事詳細用ルート (スラッグベース)
// パターン: /articles/{slug}
// 例: /articles/my-first-post → ArticlesController.Details(slug: "my-first-post")
app.MapControllerRoute(
    name: "article_details",
    pattern: "articles/{slug}",
    defaults: new { controller = "Articles", action = "Details" });

// Admin Area route: 管理画面用のルート
// パターン: /admin/{controller}/{action}/{id?}
// 例: /admin/article/edit/5 → Areas/Admin/Controllers/ArticleController.Edit(5)
// {area:exists}: areaルート値が存在する場合のみマッチ
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// デフォルトルート: 一般ユーザー向けのルート
// パターン: /{controller}/{action}/{id?}
// 例: /home/privacy → HomeController.Privacy()
// デフォルト: / → HomeController.Index()
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// データベース初期化処理
// アプリケーション起動時に一度だけ実行される
// CreateScope: DIコンテナから新しいスコープを作成
// - スコープ内でサービスを解決し、処理完了後に自動的に破棄される
// - using文により、リソースの適切な解放が保証される
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 必要なサービスをDIコンテナから取得
        var context = services.GetRequiredService<ApplicationDbContext>();        // データベースコンテキスト
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();  // ユーザー管理
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();     // ロール管理
        
        // DbInitializer.InitializeAsync: データベースの初期設定を実行
        // - マイグレーションの適用(テーブル作成)
        // - 初期ロール(Administrator, User)の作成
        // - デフォルト管理者アカウント(admin@example.com)の作成
        // - サンプルデータの投入
        await DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        // 初期化エラーをログに記録
        // アプリケーションはクラッシュせず、エラー状態で起動する
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "データベース初期化中にエラーが発生しました。");
    }
}

app.Run();
