using System;
using DTO.DTOs;

namespace DocstreamAISearch.Web.Services;

public class UploadService : IUploadService
{
    HttpClient _httpClient;
    ILogger<UploadService> _logger;
    public UploadService(HttpClient httpClient, ILogger<UploadService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<bool> UploadFileAsync(UploadRequestDTO request)
    {
        try
        {
            if (request?.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("Upload request is null or file is empty");
                return false;
            }

            using var content = new MultipartFormDataContent();
            
            // Add the file content
            using var fileStream = request.File.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.File.ContentType ?? "application/octet-stream");
            content.Add(fileContent, "file", request.File.FileName);
            
            // Add additional form data if needed
            content.Add(new StringContent(request.FileName ?? request.File.FileName), "fileName");
            content.Add(new StringContent(request.FileSize.ToString()), "fileSize");

            var response = await _httpClient.PostAsync("/api/file/upload", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("File uploaded successfully: {FileName}", request.FileName ?? request.File.FileName);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("File upload failed. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while uploading file: {FileName}", request?.FileName ?? "Unknown");
            return false;
        }
    }

    public Task DeleteFileAsync(int fileId)
    {
        // Implementation for deleting a file by ID
        throw new NotImplementedException();
    }

    public async Task<UploadResponseDTO?> GetFileDetailsAsync(int fileId)
    {
        try
        {
            _logger.LogInformation("Getting file details for file ID: {FileId}", fileId);

            var response = await _httpClient.GetAsync($"/api/file/file/{fileId}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var fileDetails = System.Text.Json.JsonSerializer.Deserialize<UploadResponseDTO>(jsonContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("File details retrieved successfully for file ID: {FileId}", fileId);
                return fileDetails;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File not found for ID: {FileId}", fileId);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get file details. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting file details for ID: {FileId}", fileId);
            return null;
        }
    }

    public async Task<SearchResponse?> ListUploadedFilesAsync()
    {
        try
        {
            _logger.LogInformation("Getting list of uploaded files");

            var response = await _httpClient.GetAsync("/api/file/list");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var filesList = System.Text.Json.JsonSerializer.Deserialize<SearchResponse>(jsonContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Files list retrieved successfully. Count: {FileCount}", filesList?.UploadedFiles?.Count ?? 0);
                return filesList;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get files list. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting files list");
            return null;
        }
    }
}

