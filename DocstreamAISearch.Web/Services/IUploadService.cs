using System;
using DTO.DTOs;

namespace DocstreamAISearch.Web.Services;

public interface IUploadService
{
    Task<bool> UploadFileAsync(UploadRequestDTO request);
    Task DeleteFileAsync(int fileId);
    Task<UploadResponseDTO?> GetFileDetailsAsync(int fileId);
    Task<PagingHeader?> ListUploadedFilesAsync(PagingParams? pagingParams = null);
    string GetFileViewUrl(int fileId);
}
