using FluentValidation;
using dotnet_mvc_test.Models.ViewModels.Admin;

namespace dotnet_mvc_test.Models.Validators.Admin
{
    public class ArticleEditViewModelValidator : AbstractValidator<ArticleEditViewModel>
    {
        public ArticleEditViewModelValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("タイトルは必須です")
                .MaximumLength(200).WithMessage("タイトルは200文字以内で入力してください");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("スラッグは必須です")
                .MaximumLength(200).WithMessage("スラッグは200文字以内で入力してください")
                .Matches(@"^[a-z0-9\-]+$").WithMessage("スラッグは小文字英数字とハイフンのみ使用できます");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("本文は必須です");

            RuleFor(x => x.Excerpt)
                .MaximumLength(500).WithMessage("抜粋は500文字以内で入力してください");

            RuleFor(x => x.FeaturedImageUrl)
                .MaximumLength(500)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("有効なURLを入力してください");
        }
    }
}
