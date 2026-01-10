# Copilot Instructions for dotnet_mvc_test

## Project Overview
ASP.NET Core 9.0 MVC blog application with SQLite database, ASP.NET Identity authentication, and admin article management. Japanese-language UI with English code.

## Architecture

### Layers & Data Flow
```
Controllers → Services (interfaces) → ApplicationDbContext → SQLite (blog.db)
     ↓
ViewModels → Razor Views
```

- **Services**: Business logic via interface-based DI (e.g., `IArticleService` → `ArticleService`)
- **DbContext**: `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>` with Fluent API configurations
- **Areas**: Admin functionality isolated in `Areas/Admin/` with separate controllers and views

### Key Entities & Relationships
- `Article` → `Category` (many-to-one), `ApplicationUser` as Author, `ArticleTag` (many-to-many with `Tag`)
- Soft delete pattern: `IsDeleted` flag on `Article`
- Slug-based URLs with unique constraints

## Conventions

### Naming & Structure
- Entities in `Models/Entities/`, ViewModels in `Models/ViewModels/{Area}/`
- Service interfaces prefixed with `I` (e.g., `IArticleService.cs`, `ArticleService.cs`)
- Admin controllers use `[Area("Admin")]` and `[Route("admin/{resource}")]` attributes

### Entity Requirements
- Use `required` keyword for non-nullable properties: `public required string Title { get; set; }`
- Always include `CreatedAt`/`UpdatedAt` timestamps
- Slugs must be unique and match regex: `^[a-z0-9\-]+$`

### ViewModel Patterns
- Japanese `[ErrorMessage]` for validation: `[Required(ErrorMessage = "タイトルは必須です")]`
- Separate Create/Edit/List ViewModels per entity

### Service Implementation
- Always use `Include()` for navigation properties in queries
- Return `Task<bool>` for update/delete operations, check existence first
- Set `UpdatedAt = DateTime.UtcNow` on modifications

## Developer Commands

```powershell
# Run application
dotnet run

# Database migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Build
dotnet build
```

## Authentication & Authorization

- **Roles**: `Administrator`, `User` (seeded in `DbInitializer`)
- **Default Admin**: `admin@example.com` / `Admin123!`
- Admin controllers require `[Authorize(Roles = "Administrator")]`
- Password policy: 6+ chars, requires digit, lowercase, uppercase (no special chars required)

## Key Files

| Purpose | Location |
|---------|----------|
| DI & Middleware | `Program.cs` |
| DB Schema | `Data/ApplicationDbContext.cs` |
| Seed Data | `Data/DbInitializer.cs` |
| Admin Routes | `Controllers/Admin/*.cs` |
| Markdown Processing | `Services/MarkdownService.cs` (uses Markdig) |

## Routing

- Admin area: `/{area:exists}/{controller=Dashboard}/{action=Index}/{id?}`
- Default: `/{controller=Home}/{action=Index}/{id?}`
- Article routes: `/admin/articles`, `/admin/articles/create`, `/admin/articles/{id}`

## AI Knowledge Storage

- `.ai/knowledge/general/` - General patterns
- `.ai/knowledge/feature/{name}/` - Feature-specific implementation notes
- `.ai/memory/session_{date}/` - Temporary session logs
See `docs/for_ai/how_to_use.md` for details.
**Read**: Before starting work, check for relevant documents and leverage past insights.
**Write**: Record discovered non-obvious patterns or solutions


