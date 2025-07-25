using System;

namespace DTO.Models;

public class UploadFile
{
    public int Id { get; set; }
    public string Name { get; set; }
    public long Size { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadDate { get; set; }
}
