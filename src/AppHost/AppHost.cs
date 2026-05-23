var builder = DistributedApplication.CreateBuilder(args);

var sqlserver = builder.AddSqlServer("sqlserver")
                       .WithEndpoint(port: 14330, targetPort: 1433, name: "tcp");

var backend = builder.AddProject<Projects.BasErpFramework_Services_WebApi>("backend")
                     .WithReference(sqlserver);

var frontend = builder.AddNpmApp("frontend", "../frontend")
                      .WithReference(backend)
                      .WithEnvironment("BROWSER", "none") // Prevent opening multiple browser tabs
                      .WithHttpEndpoint(targetPort: 4200)
                      .WithExternalHttpEndpoints()
                      .PublishAsDockerFile();

builder.Build().Run();
