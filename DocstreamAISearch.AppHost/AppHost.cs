var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

// Add MSSQL 2025 Preview database
// var sql2025db = builder.AddSqlServer("sql2025")
// .WithImageTag("2025-latest")
//  .WithEnvironment("MSSQL_PLATFORM", "linux/amd64") // Force x86_64
//     // .WithImage("mcr.microsoft.com/mssql/server", "2025-preview")
//     .WithDataVolume("sql2025-data")
//     .AddDatabase("sql2025db");

var qdrant = builder.AddQdrant("qdrant")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var apiService = builder.AddProject<Projects.DocstreamAISearch_ApiService>("apiservice")
    .WithReference(sqldb)
    .WaitFor(sqldb)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.DocstreamAISearch_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
