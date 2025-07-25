using System;
using DocstreamAISearch.ApiService.TextChunkers;

namespace DocstreamAISearch.ApiService.ContentDecoders;

public class TextContentDecoder(IServiceProvider serviceProvider) : IContentDecoder
{
    public async Task<IEnumerable<Chunk>> DecodeAsync(Stream stream, string contentType)
    {
        var textChunker = serviceProvider.GetRequiredKeyedService<ITextChunker>(contentType);

        using var readStream = new StreamReader(stream);
        var content = await readStream.ReadToEndAsync();

        var paragraphs = textChunker.Split(content.Trim());
        return paragraphs.Select((text, index) => new Chunk(null, index, text)).ToList();
    }
}
