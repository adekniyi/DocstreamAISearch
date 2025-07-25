using System;
using DocstreamAISearch.ApiService.Repositories;
using DTO.DTOs;

namespace DocstreamAISearch.ApiService.Interfaces;

public interface IFileManager
{
    Task<bool> UploadFileAsync(UploadRequestDTO request);
    Task DeleteFileAsync(int fileId);
    Task<UploadResponseDTO?> GetFileDetailsAsync(int fileId);
    Task<PagingHeader?> ListUploadedFilesAsync(PagingParams pagingParams);
}
