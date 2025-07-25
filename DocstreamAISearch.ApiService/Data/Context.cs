using System;
using DTO.Models;
using Microsoft.EntityFrameworkCore;

namespace DocstreamAISearch.ApiService.Data;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<UploadFile> UploadFiles { get; set; }
}
