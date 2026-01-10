using System.ComponentModel.DataAnnotations;

namespace dotnet_mvc_test.Models.ViewModels.Admin
{
    public class ArticleEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "タイトルは必須です")]
        [StringLength(200, ErrorMessage = "タイトルは200文字以内で入力してください")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "スラッグは必須です")]
        [StringLength(200, ErrorMessage = "スラッグは200文字以内で入力してください")]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "スラッグは小文字英数字とハイフンのみ使用できます")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "本文は必須です")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "抜粋は500文字以内で入力してください")]
        public string? Excerpt { get; set; }

        [Display(Name = "カテゴリ")]
        public int? CategoryId { get; set; }

        [Display(Name = "アイキャッチ画像URL")]
        [StringLength(500)]
        [Url(ErrorMessage = "有効なURLを入力してください")]
        public string? FeaturedImageUrl { get; set; }

        [Display(Name = "公開する")]
        public bool IsPublished { get; set; }

        [Display(Name = "タグ")]
        public List<int> SelectedTagIds { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
