using System;
using DTO.Models;

namespace DocstreamAISearch.ApiService.Data;

public static class DbInitializer
{
    public static void Initialize(Context context)
    {
        if (context.UploadFiles.Any())
            return;
        
        var files = new List<UploadFile>
        {
            new UploadFile { Name = "Sample1.txt", Size = 1024, FilePath = "/uploads/sample1.txt", UploadDate = DateTime.Now, },
            new UploadFile { Name = "Sample2.txt", Size = 2048, FilePath = "/uploads/sample2.txt", UploadDate = DateTime.Now }
        };
        context.UploadFiles.AddRange(files);
        context.SaveChanges();
    }
}
