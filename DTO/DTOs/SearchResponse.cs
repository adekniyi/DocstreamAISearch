using System;

namespace DTO.DTOs;

public class SearchResponse
{
    public string ResponseMessage { get; set; } = string.Empty;
    public List<UploadResponseDTO> UploadedFiles { get; set; } = new();
}
