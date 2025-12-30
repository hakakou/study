var builder = DistributedApplication.CreateBuilder(args);

var pass = builder.AddParameter("admin", "admin");
var mongodb = builder.AddMongoDB("mongodb", password: pass)
    .WithEndpointProxySupport(false)
    .AddDatabase("webdb");

builder.AddProject<Projects.WebAPI>("webapi")
    .WithReference(mongodb)
    .WaitFor(mongodb);

builder.Build().Run();
