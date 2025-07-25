using System;

namespace DocstreamAISearch.ApiService.Repositories;

public class PagedList<T> where T : class
{
    public PagedList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        TotalItems = source.Count();
        PageNumber = pageNumber;
        PageSize = pageSize;
        List = source
                        .Skip(pageSize * (pageNumber - 1))
                        .Take(pageSize)
                        .ToList();
    }

    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public List<T> List { get; }
    public int TotalPages =>
            (int)Math.Ceiling(this.TotalItems / (double)this.PageSize);
    public bool HasPreviousPage => this.PageNumber > 1;
    public bool HasNextPage => this.PageNumber < this.TotalPages;
    public int NextPageNumber =>
            this.HasNextPage ? this.PageNumber + 1 : this.TotalPages;
    public int PreviousPageNumber =>
            this.HasPreviousPage ? this.PageNumber - 1 : 1;

    public PagingHeader GetHeader()
    {
        return new PagingHeader(
                TotalItems, PageNumber,
                PageSize, TotalPages, NextPageNumber, PreviousPageNumber, List);
    }
     
}


public class PagingHeader
{
    public PagingHeader(
        int totalItems, int pageNumber, int pageSize, int totalPages, int nextPage, int previousPage, dynamic result)
    {
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        NextPage = nextPage;
        PreviousPage = previousPage;
        Result = result;
    }

    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int NextPage { get; }
    public int PreviousPage { get; }
    public dynamic Result { get; set; }
}