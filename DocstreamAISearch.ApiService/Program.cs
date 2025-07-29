using System.Net.Mime;
using DocstreamAISearch.ApiService.ContentDecoders;
using DocstreamAISearch.ApiService.Data;
using DocstreamAISearch.ApiService.Interfaces;
using DocstreamAISearch.ApiService.Repositories;
using DocstreamAISearch.ApiService.Settings;
using DocstreamAISearch.ApiService.TextChunkers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add CORS for the Web app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:7443", "http://localhost:5000", "https+http://webfrontend")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.AddSqlServerDbContext<Context>("sqldb");
// Add services to the container.

builder.AddQdrantClient("qdrant");

builder.AddOllamaSharpChatClient("chat-ollama");
builder.AddOllamaSharpEmbeddingGenerator("embed-ollama");


// builder.Services.AddSingleton(sp =>
// {
//     var logger = sp.GetService<ILogger<Program>>();
//     logger.LogInformation("Creating memory context");
//     return new MemoryContext(logger, sp.GetService<IChatClient>(), sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
// });


var appSettingsSection = builder.Configuration.GetSection(nameof(AppSettings));
builder.Services.Configure<AppSettings>(appSettingsSection);

builder.Services.AddScoped<IFileManager, FileManager>();


builder.Services.AddKeyedSingleton<IContentDecoder, PdfContentDecoder>(MediaTypeNames.Application.Pdf);
builder.Services.AddKeyedSingleton<IContentDecoder, DocxContentDecoder>("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
builder.Services.AddKeyedSingleton<IContentDecoder, TextContentDecoder>(MediaTypeNames.Text.Plain);
builder.Services.AddKeyedSingleton<IContentDecoder, TextContentDecoder>(MediaTypeNames.Text.Markdown);

builder.Services.AddKeyedSingleton<ITextChunker, DefaultTextChunker>(KeyedService.AnyKey);
builder.Services.AddKeyedSingleton<ITextChunker, MarkdownTextChunker>(MediaTypeNames.Text.Markdown);
builder.Services.AddScoped<VectorManager>();
builder.Services.AddSingleton<VectorDatabase>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Enable CORS
app.UseCors("AllowWebApp");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    try
    {
        app.Logger.LogInformation("Ensure database created");
        context.Database.EnsureCreated();
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error creating database");
    }
    DbInitializer.Initialize(context);

    // app.Logger.LogInformation("Start fill products in vector db");
    // var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    // await memoryContext.InitMemoryContextAsync(context);
    // app.Logger.LogInformation("Done fill products in vector db");
}
app.Run();