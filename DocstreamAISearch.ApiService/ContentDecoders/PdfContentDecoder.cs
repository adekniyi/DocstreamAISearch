using System;
using DocstreamAISearch.ApiService.TextChunkers;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace DocstreamAISearch.ApiService.ContentDecoders;

public class PdfContentDecoder(IServiceProvider serviceProvider) : IContentDecoder
{
    public Task<IEnumerable<Chunk>> DecodeAsync(Stream stream, string contentType)
    {
       var textChunker = serviceProvider.GetRequiredKeyedService<ITextChunker>(contentType);

        // Read the content of the PDF document.
        using var pdfDocument = PdfDocument.Open(stream);
        var paragraphs = pdfDocument.GetPages().SelectMany(page => GetPageParagraphs(page, textChunker)).ToList();

        return Task.FromResult(paragraphs.AsEnumerable());
    }

    private static IEnumerable<Chunk> GetPageParagraphs(Page pdfPage, ITextChunker textChunker)
    {
        var letters = pdfPage.Letters;
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
        var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        var pageText = string.Join($"{Environment.NewLine}{Environment.NewLine}", textBlocks.Select(t => t.Text.ReplaceLineEndings(" ")));

        var paragraphs = textChunker.Split(pageText.Trim());

        return paragraphs.Where(p => !string.IsNullOrWhiteSpace(p)).Select((text, index) => new Chunk(pdfPage.Number, index, text));
    }
}
