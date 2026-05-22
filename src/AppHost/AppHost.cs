var builder = DistributedApplication.CreateBuilder(args);

var sqlserver = builder.AddSqlServer("sqlserver")
                       .WithImage("mcr.microsoft.com/mssql/server")
                       .WithImageTag("2022-latest")
                       .WithEndpoint(port: 1433, targetPort: 1433, name: "tcp");

var backend = builder.AddProject<Projects.BasErpFramework_Services_WebApi>("backend")
                     .WithReference(sqlserver);

var frontend = builder.AddNpmApp("frontend", "../frontend")
                      .WithReference(backend)
                      .WithEnvironment("BROWSER", "none") // Prevent opening multiple browser tabs
                      .WithHttpEndpoint(env: "PORT")
                      .WithExternalHttpEndpoints()
                      .PublishAsDockerFile();

builder.Build().Run();
