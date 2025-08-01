using System;
using System.Text;
using DocstreamAISearch.ApiService.TextChunkers;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;

namespace DocstreamAISearch.ApiService.ContentDecoders;

public class DocxContentDecoder(IServiceProvider serviceProvider) : IContentDecoder
{
    public Task<IEnumerable<Chunk>> DecodeAsync(Stream stream, string contentType)
    {
        var textChunker = serviceProvider.GetRequiredKeyedService<ITextChunker>(contentType);

        using var document = WordprocessingDocument.Open(stream, false);

        var body = document.MainDocumentPart?.Document.Body;
        var content = new StringBuilder();

        foreach (var p in body?.Descendants<Paragraph>() ?? [])
        {
            content.AppendLine(p.InnerText);
        }

        var paragraphs = textChunker.Split(content.ToString().Trim());

        return Task.FromResult(paragraphs.Select((text, index) => new Chunk(null, index, text)).ToList().AsEnumerable());
    }
}
