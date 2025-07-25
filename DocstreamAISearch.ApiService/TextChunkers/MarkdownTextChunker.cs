using System;
using DocstreamAISearch.ApiService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;

namespace DocstreamAISearch.ApiService.TextChunkers;

public class MarkdownTextChunker(IOptions<AppSettings> appSettingsOptions) : ITextChunker
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;
    public IList<string> Split(string text)
    {
        var lines = TextChunker.SplitMarkDownLines(text, appSettings.MaxTokensPerLine);
        var paragraphs = TextChunker.SplitMarkdownParagraphs(lines, appSettings.MaxTokensPerParagraph);

        return paragraphs;
    }
}
