using Markdig;

namespace dotnet_mvc_test.Services
{
    public interface IMarkdownService
    {
        string ToHtml(string markdown);
    }

    public class MarkdownService : IMarkdownService
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownService()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        public string ToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            return Markdown.ToHtml(markdown, _pipeline);
        }
    }
}
