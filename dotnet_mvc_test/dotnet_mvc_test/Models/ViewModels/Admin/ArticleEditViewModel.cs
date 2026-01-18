using System.ComponentModel.DataAnnotations;

namespace dotnet_mvc_test.Models.ViewModels.Admin
{
    public class ArticleEditViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? Excerpt { get; set; }

        [Display(Name = "カテゴリ")]
        public int? CategoryId { get; set; }

        [Display(Name = "アイキャッチ画像URL")]
        public string? FeaturedImageUrl { get; set; }

        [Display(Name = "公開する")]
        public bool IsPublished { get; set; }

        [Display(Name = "タグ")]
        public List<int> SelectedTagIds { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
