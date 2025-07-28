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
    }
}
