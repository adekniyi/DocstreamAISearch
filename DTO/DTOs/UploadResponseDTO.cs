using System;

namespace DTO.DTOs;

public class UploadResponseDTO
{
    public string FileRelativePath { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public int Id { get; set; }
}
