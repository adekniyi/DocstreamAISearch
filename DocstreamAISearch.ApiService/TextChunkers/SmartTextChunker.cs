using System;
using DocstreamAISearch.ApiService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;

namespace DocstreamAISearch.ApiService.TextChunkers;

public class SmartTextChunker(IOptions<AppSettings> appSettingsOptions) : ITextChunker
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;
    
    public IList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // First, split into smaller base chunks
        var lines = TextChunker.SplitPlainTextLines(text, appSettings.MaxTokensPerLine);
        var baseParagraphs = TextChunker.SplitPlainTextParagraphs(lines, appSettings.MaxTokensPerParagraph);
        
        // Create overlapping chunks for better search coverage
        var overlappingChunks = new List<string>();
        
        for (int i = 0; i < baseParagraphs.Count; i++)
        {
            // Add the base chunk
            overlappingChunks.Add(baseParagraphs[i]);
            
            // Create overlapping chunk with next paragraph if available
            if (i < baseParagraphs.Count - 1)
            {
                var combinedChunk = $"{baseParagraphs[i]} {baseParagraphs[i + 1]}";
                
                // Only add if the combined chunk isn't too large
                if (EstimateTokenCount(combinedChunk) <= appSettings.MaxTokensPerParagraph * 1.5)
                {
                    overlappingChunks.Add(combinedChunk);
                }
            }
        }
        
        // Remove duplicates and very short chunks
        return overlappingChunks
            .Distinct()
            .Where(chunk => chunk.Trim().Length > 20) // Filter out very short chunks
            .ToList();
    }
    
    private static int EstimateTokenCount(string text)
    {
        // Rough estimate: 1 token ≈ 4 characters for English text
        return text.Length / 4;
    }
}
