using System;

namespace DocstreamAISearch.ApiService.TextChunkers;

public interface ITextChunker
{
    IList<string> Split(string text);
}
