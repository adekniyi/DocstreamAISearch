using System;
using DTO.Models;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DocstreamAISearch.ApiService.Data;

public class VectorDatabase
{
    private readonly QdrantClient _qdrant;

    public VectorDatabase(QdrantClient qdrant)
    {
        _qdrant = qdrant;
    }

    public async Task InitAsync(string collectionName)
    {
        var collectionExists = await _qdrant.CollectionExistsAsync(collectionName);
        if (collectionExists)
        {
            // await DeleteCollectionAsync(collectionName);
            return;
        }

        await CreateCollectionAsync(collectionName);
    }

    public async Task UpsertVectorsAsync(string collectionName, IEnumerable<DocumentChunk> chunks)
    {
        var points = chunks.Select(chunk => 
        {
            var point = new PointStruct
            {
                Id = new PointId { Uuid = chunk.Id.ToString() },
                Vectors = chunk.Embedding
            };

            point.Payload.Add("uploadFileId", chunk.UploadFileId);
            point.Payload.Add("content", chunk.Content);
            point.Payload.Add("index", chunk.Index);
            point.Payload.Add("pageNumber", chunk.PageNumber ?? 0);
            point.Payload.Add("indexOnPage", chunk.IndexOnPage);

            return point;
        });

        await _qdrant.UpsertAsync(collectionName, points.ToList());
    }

    private async Task CreateCollectionAsync(string collectionName)
    {
        // Create collection
        await _qdrant.CreateCollectionAsync(collectionName, new VectorParams
        {
            Size = 768, // Size of the vector (nomic-embed-text produces 768-dimensional vectors)
            Distance = Distance.Cosine // Use cosine distance for similarity
        });
    }
    
    public async Task<List<int>> SearchAsync(string collectionName, float[] searchVector, int size = 10, float scoreThreshold = 0.5f)
    {
        try
        {
            // Perform vector search with score threshold
            var searchResult = await _qdrant.SearchAsync(
                collectionName, 
                searchVector, 
                limit: (ulong)size,
                scoreThreshold: scoreThreshold
            );
            
            // Extract UploadFileIds from payload and get distinct values
            // Only include results that meet the score threshold
            var uploadFileIds = searchResult
                .Where(point => point.Score >= scoreThreshold && point.Payload.ContainsKey("uploadFileId"))
                .Select(point => Convert.ToInt32(point.Payload["uploadFileId"].IntegerValue))
                .Distinct()
                .ToList();
            
            return uploadFileIds;
        }
        catch (Exception ex)
        {
            // Log the error (you might want to inject ILogger)
            throw new Exception($"Error performing vector search: {ex.Message}", ex);
        }
    }

    public async Task DeleteCollectionAsync(string collectionName)
    {
        try
        {
            var collectionExists = await _qdrant.CollectionExistsAsync(collectionName);
            if (collectionExists)
            {
                await _qdrant.DeleteCollectionAsync(collectionName);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting collection '{collectionName}': {ex.Message}", ex);
        }
    }

}
