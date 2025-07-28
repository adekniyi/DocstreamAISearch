using System;
using Microsoft.Extensions.VectorData;

namespace DTO.Models;

public class DocumentChunk
{
    [VectorStoreKey]
    public Guid Id { get; set; }
    [VectorStoreData]
    public int UploadFileId { get; set; }
    [VectorStoreData]
    public string Content { get; set; } = string.Empty;
    [VectorStoreData]
    public int Index { get; set; }
    [VectorStoreData]
    public int? PageNumber { get; set; }
    [VectorStoreData]
    public int IndexOnPage { get; set; }
    [VectorStoreVector(384)]
    public required float[] Embedding { get; set; }
}
