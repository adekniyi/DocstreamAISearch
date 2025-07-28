using System;
using DocstreamAISearch.ApiService.Data;
using DocstreamAISearch.ApiService.Interfaces;
using DTO.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DocstreamAISearch.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly VectorDatabase _vectorDatabase;
    private readonly ILogger<DebugController> _logger;

    public DebugController(VectorDatabase vectorDatabase, ILogger<DebugController> logger)
    {
        _vectorDatabase = vectorDatabase;
        _logger = logger;
    }

    [HttpGet("search-debug/{query}")]
    public async Task<IActionResult> SearchDebug(string query, [FromQuery] float threshold = 0.1f, [FromQuery] int limit = 20)
    {
        try
        {
            // This is a debug endpoint to help you see what's in your vector database
            _logger.LogInformation("Debug search for query: {Query} with threshold: {Threshold}", query, threshold);
            
            // You'll need to implement a debug version of SearchAsync that returns more details
            // For now, let's just return a message
            return Ok(new { 
                Message = "Debug endpoint created. You can enhance this to show chunk details.",
                Query = query,
                Threshold = threshold,
                Suggestion = "Try re-uploading your documents with the new smaller chunk settings"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug search");
            return BadRequest(ex.Message);
        }
    }
}
