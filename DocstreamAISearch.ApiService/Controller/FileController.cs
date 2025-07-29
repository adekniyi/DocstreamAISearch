using DocstreamAISearch.ApiService.Interfaces;
using DTO.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocstreamAISearch.ApiService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileManager _fileManager;

        public FileController(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadRequestDTO request)
        {
            var result = await _fileManager.UploadFileAsync(request);
            return result ? Ok() : BadRequest("File upload failed.");
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            await _fileManager.DeleteFileAsync(fileId);
            return NoContent();
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFileDetails(int fileId)
        {
            var fileDetails = await _fileManager.GetFileDetailsAsync(fileId);
            return fileDetails != null ? Ok(fileDetails) : NotFound();
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListUploadedFiles([FromQuery]PagingParams pagingParams)
        {
            var files = await _fileManager.ListUploadedFilesAsync(pagingParams);
            return Ok(files);
        }

        [HttpGet("view/{fileId}")]
        public async Task<IActionResult> ViewFile(int fileId)
        {
            try
            {
                var fileDetails = await _fileManager.GetFileDetailsAsync(fileId);
                if (fileDetails == null)
                {
                    return NotFound("File not found.");
                }

                // Construct the full file path
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileDetails.FileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Physical file not found.");
                }

                // Get the file extension to determine content type
                var extension = Path.GetExtension(fileDetails.FileName).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".txt" => "text/plain; charset=utf-8",
                    ".md" or ".markdown" => "text/markdown; charset=utf-8",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".svg" => "image/svg+xml",
                    _ => "application/octet-stream"
                };

                // Read the file and return it for inline viewing
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                
                // Return file with inline content disposition to prevent download
                var result = File(fileBytes, contentType);
                
                // Set headers for inline viewing
                Response.Headers.Add("Content-Disposition", "inline");
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
                
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error serving file: {ex.Message}");
            }
        }
    }
}
