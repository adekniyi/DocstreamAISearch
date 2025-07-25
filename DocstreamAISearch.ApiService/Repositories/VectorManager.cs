using System;
using DocstreamAISearch.ApiService.ContentDecoders;
using DocstreamAISearch.ApiService.Data;
using DocstreamAISearch.ApiService.Settings;
using DTO.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace DocstreamAISearch.ApiService.Repositories;

public class VectorManager(IServiceProvider serviceProvider, Context dbContext
, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IChatClient chatService
, IOptions<AppSettings> appSettingsOptions, ILogger<VectorManager> logger, VectorDatabase vectorDatabase)
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;
    private readonly string collectionName = "document";

    public async Task<bool> ImportAsync(Stream stream, string name, string contentType, int uploadFileId)
    {
        try
        {
            await vectorDatabase.InitAsync(collectionName);

            var decoder = serviceProvider.GetKeyedService<IContentDecoder>(contentType) ?? throw new NotSupportedException($"Content type '{contentType}' is not supported.");
            var chunks = await decoder.DecodeAsync(stream, contentType);
            var chunkContents = chunks.Select(p => p.Content).ToList();

            var embeddings = new List<Embedding<float>>();
            List<DocumentChunk> documentChunks = new();

            foreach (var batch in chunkContents.Chunk(appSettings.EmbeddingBatchSize))
            {
                logger.LogDebug("Processing batch of {Count} chunks for embedding generation...", batch.Length);

                // Generate embeddings for this batch.
                var batchEmbeddings = await embeddingGenerator.GenerateAsync(batch, cancellationToken: default);
                embeddings.AddRange(batchEmbeddings);
            }

            foreach (var (index, embedding) in embeddings.Index())
            {
                var chunk = chunks.ElementAt(index);

                var documentChunk = new DocumentChunk
                {
                    UploadFileId = uploadFileId,
                    Index = index,
                    PageNumber = chunk.PageNumber,
                    IndexOnPage = chunk.IndexOnPage,
                    Content = chunk.Content,
                    Embedding = embedding.Vector.ToArray()
                };

                documentChunks.Add(documentChunk);
            }
            await vectorDatabase.UpsertVectorsAsync(collectionName, documentChunks);

            return true;
        }
        catch (System.Exception ex) when (ex is NotSupportedException || ex is InvalidOperationException)
        {
            logger.LogError(ex, "An error occurred while importing the document: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while importing the document: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<int>> SearchAsync(string searchQuery, int size = 10)
    {
        try
        {
            logger.LogInformation("Performing vector search for query: {SearchQuery}", searchQuery);

            await vectorDatabase.InitAsync(collectionName);

            var searchEmbedding = await embeddingGenerator.GenerateAsync(searchQuery);
            var searchVector = searchEmbedding.Vector.ToArray();

            var uploadFileIds = await vectorDatabase.SearchAsync(collectionName, searchVector, size);

            logger.LogInformation("Vector search completed. Found {Count} distinct files for query: {SearchQuery}", 
                uploadFileIds.Count, searchQuery);

            return uploadFileIds;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while performing vector search for query: {SearchQuery}", searchQuery);
            throw;
        }
    }

}
