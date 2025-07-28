using System;

namespace DTO.DTOs;

public class PagingHeader
{
    public PagingHeader()
    {
    }

    public PagingHeader(
        int totalItems, int pageNumber, int pageSize, int totalPages, int nextPage, int previousPage)
    {
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        NextPage = nextPage;
        PreviousPage = previousPage;
    }

    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int NextPage { get; set; }
    public int PreviousPage { get; set; }
    public List<UploadResponseDTO> Result { get; set; }
}
