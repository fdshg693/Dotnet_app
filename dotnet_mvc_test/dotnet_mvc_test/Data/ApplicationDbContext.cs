using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Article configuration
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.HasIndex(e => e.Slug)
                .IsUnique();
            
            entity.Property(e => e.Content)
                .IsRequired();
            
            entity.Property(e => e.Summary)
                .HasMaxLength(500);
            
            entity.Property(e => e.Excerpt)
                .HasMaxLength(500);
            
            entity.Property(e => e.FeaturedImageUrl)
                .HasMaxLength(500);
            
            entity.Property(e => e.IsPublished)
                .HasDefaultValue(false);
            
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("datetime('now')");

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Articles)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.PublishedAt);
            entity.HasIndex(e => new { e.IsPublished, e.IsDeleted });
        });

        // ArticleTag configuration (Many-to-Many)
        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.HasKey(at => new { at.ArticleId, at.TagId });

            entity.HasOne(at => at.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(at => at.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(at => at.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(at => at.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Slug)
                .IsUnique();
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.HasIndex(e => e.Slug)
                .IsUnique();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AuthorName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.AuthorEmail)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");

            // Relationship
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index
            entity.HasIndex(e => e.ArticleId);
        });

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
        });
    }
}
