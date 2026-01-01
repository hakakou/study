var builder = DistributedApplication.CreateBuilder(args);

var pass = builder.AddParameter("admin", "admin");

var mongodb = builder.AddMongoDB("mongodb", password: pass)
    .WithEndpointProxySupport(false)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("webdb");

var postgres = builder.AddPostgres("postgres", password: pass)
    .WithEndpointProxySupport(false)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("webdb2");

builder.AddProject<Projects.WebAPI>("webapi")
    .WithReference(mongodb)
    .WaitFor(mongodb);

builder.AddProject<Projects.WebAPI2>("webapi2")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();
