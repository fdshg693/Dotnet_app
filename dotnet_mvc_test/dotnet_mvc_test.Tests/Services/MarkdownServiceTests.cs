using dotnet_mvc_test.Services;

namespace dotnet_mvc_test.Tests.Services;

public class MarkdownServiceTests
{
    private readonly IMarkdownService _markdownService;

    public MarkdownServiceTests()
    {
        _markdownService = new MarkdownService();
    }

    #region Basic Markdown Conversion Tests

    [Fact]
    public void ToHtml_ConvertsSimpleParagraph_ReturnsHtmlParagraph()
    {
        // Arrange
        var markdown = "This is a simple paragraph.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<p>This is a simple paragraph.</p>", result);
    }

    [Fact]
    public void ToHtml_ConvertsHeadings_ReturnsHtmlHeadings()
    {
        // Arrange
        var markdown = @"# Heading 1
## Heading 2
### Heading 3";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<h1", result);
        Assert.Contains("Heading 1", result);
        Assert.Contains("<h2", result);
        Assert.Contains("Heading 2", result);
        Assert.Contains("<h3", result);
        Assert.Contains("Heading 3", result);
    }

    [Fact]
    public void ToHtml_ConvertsBoldAndItalic_ReturnsHtmlWithStrongAndEm()
    {
        // Arrange
        var markdown = "This is **bold** and this is *italic* text.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<strong>bold</strong>", result);
        Assert.Contains("<em>italic</em>", result);
    }

    [Fact]
    public void ToHtml_ConvertsLinks_ReturnsHtmlAnchorTags()
    {
        // Arrange
        var markdown = "[OpenAI](https://openai.com)";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<a href=\"https://openai.com\">OpenAI</a>", result);
    }

    [Fact]
    public void ToHtml_ConvertsBlockquote_ReturnsHtmlBlockquote()
    {
        // Arrange
        var markdown = "> This is a quote";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<blockquote>", result);
        Assert.Contains("This is a quote", result);
        Assert.Contains("</blockquote>", result);
    }

    #endregion

    #region Advanced Extensions Tests

    [Fact]
    public void ToHtml_ConvertsUnorderedList_ReturnsHtmlUlAndLi()
    {
        // Arrange
        var markdown = @"- Item 1
- Item 2
- Item 3";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<ul>", result);
        Assert.Contains("<li>Item 1</li>", result);
        Assert.Contains("<li>Item 2</li>", result);
        Assert.Contains("<li>Item 3</li>", result);
        Assert.Contains("</ul>", result);
    }

    [Fact]
    public void ToHtml_ConvertsOrderedList_ReturnsHtmlOlAndLi()
    {
        // Arrange
        var markdown = @"1. First item
2. Second item
3. Third item";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<ol>", result);
        Assert.Contains("<li>First item</li>", result);
        Assert.Contains("<li>Second item</li>", result);
        Assert.Contains("<li>Third item</li>", result);
        Assert.Contains("</ol>", result);
    }

    [Fact]
    public void ToHtml_ConvertsCodeBlock_ReturnsHtmlPreAndCode()
    {
        // Arrange
        var markdown = @"```csharp
public void Hello()
{
    Console.WriteLine(""Hello"");
}
```";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<pre>", result);
        Assert.Contains("<code", result);
        Assert.Contains("public void Hello()", result);
        Assert.Contains("Console.WriteLine", result);
    }

    [Fact]
    public void ToHtml_ConvertsInlineCode_ReturnsHtmlCodeTag()
    {
        // Arrange
        var markdown = "Use the `var` keyword in C#.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<code>var</code>", result);
    }

    [Fact]
    public void ToHtml_ConvertsTable_ReturnsHtmlTable()
    {
        // Arrange
        var markdown = @"| Header 1 | Header 2 |
|----------|----------|
| Cell 1   | Cell 2   |
| Cell 3   | Cell 4   |";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<table>", result);
        Assert.Contains("<thead>", result);
        Assert.Contains("<th>Header 1</th>", result);
        Assert.Contains("<th>Header 2</th>", result);
        Assert.Contains("<tbody>", result);
        Assert.Contains("<td>Cell 1</td>", result);
        Assert.Contains("<td>Cell 2</td>", result);
        Assert.Contains("</table>", result);
    }

    [Fact]
    public void ToHtml_ConvertsStrikethrough_ReturnsHtmlDel()
    {
        // Arrange
        var markdown = "This is ~~deleted~~ text.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<del>deleted</del>", result);
    }

    [Fact]
    public void ToHtml_ConvertsTaskList_ReturnsHtmlCheckboxes()
    {
        // Arrange
        var markdown = @"- [x] Completed task
- [ ] Pending task";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("Completed task", result);
        Assert.Contains("Pending task", result);
        // Markdig's advanced extensions should handle task lists with class attribute
        Assert.Contains("contains-task-list", result);
    }

    [Fact]
    public void ToHtml_ConvertsAutolinks_ReturnsHtmlAnchorTags()
    {
        // Arrange
        var markdown = "Visit https://example.com for more info.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<a href=\"https://example.com\">https://example.com</a>", result);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public void ToHtml_WithNull_ReturnsEmptyString()
    {
        // Arrange
        string? markdown = null;

        // Act
        var result = _markdownService.ToHtml(markdown!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToHtml_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var markdown = string.Empty;

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToHtml_WithWhitespaceOnly_ReturnsEmptyString()
    {
        // Arrange
        var markdown = "   \n\t  ";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToHtml_WithSpecialHtmlCharacters_AllowsRawHtml()
    {
        // Arrange
        var markdown = "Text with <script>alert('xss')</script> tags.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        // MarkdigはデフォルトでHTMLタグをそのまま通す
        Assert.Contains("<script>", result);
        Assert.Contains("</script>", result);
    }

    [Fact]
    public void ToHtml_WithMultipleParagraphs_ReturnsMultipleHtmlParagraphs()
    {
        // Arrange
        var markdown = @"First paragraph.

Second paragraph.

Third paragraph.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        var paragraphCount = result.Split("<p>").Length - 1;
        Assert.Equal(3, paragraphCount);
    }

    [Fact]
    public void ToHtml_WithNestedMarkdown_ConvertsCorrectly()
    {
        // Arrange
        var markdown = "This is **bold with *italic* inside** text.";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<strong>", result);
        Assert.Contains("<em>italic</em>", result);
        Assert.Contains("</strong>", result);
    }

    [Fact]
    public void ToHtml_WithHorizontalRule_ReturnsHtmlHr()
    {
        // Arrange
        var markdown = @"Content above

---

Content below";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<hr", result);
    }

    [Fact]
    public void ToHtml_WithImageMarkdown_ReturnsHtmlImg()
    {
        // Arrange
        var markdown = "![Alt text](https://example.com/image.png)";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<img", result);
        Assert.Contains("src=\"https://example.com/image.png\"", result);
        Assert.Contains("alt=\"Alt text\"", result);
    }

    [Fact]
    public void ToHtml_WithComplexDocument_ConvertsAllElements()
    {
        // Arrange
        var markdown = @"# Article Title

This is an **introduction** with *emphasis*.

## Features

- Feature 1
- Feature 2
- Feature 3

### Code Example

```csharp
var service = new MarkdownService();
```

> Remember to test your code!

[Learn more](https://example.com)";

        // Act
        var result = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<h1", result);
        Assert.Contains("<h2", result);
        Assert.Contains("<h3", result);
        Assert.Contains("<strong>introduction</strong>", result);
        Assert.Contains("<em>emphasis</em>", result);
        Assert.Contains("<ul>", result);
        Assert.Contains("<pre>", result);
        Assert.Contains("<code", result);
        Assert.Contains("<blockquote>", result);
        Assert.Contains("<a href=\"https://example.com\">Learn more</a>", result);
    }

    #endregion
}
