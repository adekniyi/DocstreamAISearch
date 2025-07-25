using System;

namespace DTO.DTOs;

public class PagingParams
{
    public string SearchParams { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool SearchByIA { get; set; }
}
