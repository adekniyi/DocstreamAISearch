using System;
using Microsoft.AspNetCore.Http;

namespace DTO.DTOs;

public class UploadRequestDTO
{
    public IFormFile File { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
}
