using System;
using DocstreamAISearch.ApiService.Data;
using DocstreamAISearch.ApiService.Interfaces;
using DTO.DTOs;
using DTO.Models;
using Microsoft.EntityFrameworkCore;

namespace DocstreamAISearch.ApiService.Repositories;

public class FileManager : IFileManager
{
    Context _context;
    VectorManager vectorManager;
    public FileManager(Context context, VectorManager vectorManager)
    {
        _context = context;
        this.vectorManager = vectorManager;
    }
    public Task DeleteFileAsync(int fileId)
    {
        throw new NotImplementedException();
    }

    public async Task<UploadResponseDTO?> GetFileDetailsAsync(int fileId)
    {
        var file = await _context.UploadFiles.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file == null)
        {
            return null;
        }

        return new UploadResponseDTO
        {
            Id = file.Id,
            FileName = file.Name,
            FileSize = file.Size,
            FileRelativePath = file.FilePath,
            UploadDate = file.UploadDate
        };
    }

    public async Task<PagingHeader?> ListUploadedFilesAsync(PagingParams pagingParams)
    {
        IQueryable<UploadFile> files;
        
        if (pagingParams.SearchByIA)
        {
            if (string.IsNullOrEmpty(pagingParams.SearchParams))
            {
                return null; // No search params provided
            }
            var fileIds = await vectorManager.SearchAsync(pagingParams.SearchParams, pagingParams.PageSize);
            files = GetUploadFiles(fileIds);
        }
        else
        {
           files = _context.UploadFiles.Where(f => string.IsNullOrEmpty(pagingParams.SearchParams) || EF.Functions.Like(f.Name, $"%{pagingParams.SearchParams}%"));
        }

        var result = new PagedList<UploadFile>(files.AsQueryable(), pagingParams.PageNumber, pagingParams.PageSize);

        var response = result.GetHeader();
        response.Result = result.List.Select(f => new UploadResponseDTO
        {
            Id = f.Id,
            FileName = f.Name,
            FileSize = f.Size,
            FileRelativePath = f.FilePath,
            UploadDate = f.UploadDate
        }).ToList();

        return response;
    }

    public async Task<bool> UploadFileAsync(UploadRequestDTO request)
    {
        var (filePath, stream) = SaveFileToRoot(request.File);
        
        try
        {
            var file = new UploadFile
            {
                Name = request.FileName,
                Size = request.FileSize,
                FilePath = filePath,
                UploadDate = DateTime.UtcNow
            };

            _context.UploadFiles.Add(file);
            await _context.SaveChangesAsync();

            await vectorManager.ImportAsync(stream, request.FileName, request.File.ContentType, file.Id);
            return true;
        }
        finally
        {
            // Ensure the stream is disposed
            stream?.Dispose();
        }
    }


    private (string, Stream) SaveFileToRoot(IFormFile file)
    {
        var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsDirectory))
        {
            Directory.CreateDirectory(uploadsDirectory);
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);
        
        // Log the file path for debugging
        Console.WriteLine($"Saving file to: {filePath}");
        
        // Save the file first
        using (var writeStream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(writeStream);
        }
        
        // Then create a new stream for reading
        var readStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        return ($"/uploads/{file.FileName}", readStream);
    }
    
    private IQueryable<UploadFile> GetUploadFiles(List<int> fileIds)
    {
        return  _context.UploadFiles
            .Where(f => fileIds.Contains(f.Id));
    }
}
