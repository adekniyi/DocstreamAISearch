using System;

namespace DocstreamAISearch.ApiService.ContentDecoders;

public interface IContentDecoder
{
    Task<IEnumerable<Chunk>> DecodeAsync(Stream stream, string contentType);
}

public record class Chunk(int? PageNumber, int IndexOnPage, string Content);