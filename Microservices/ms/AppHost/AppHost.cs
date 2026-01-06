var builder = DistributedApplication.CreateBuilder(args);

var pass = builder.AddParameter("admin", "admin");

var mongodb = builder.AddMongoDB("mongodb", password: pass)
    //.WithEndpointProxySupport(false)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("webdb");

var postgres = builder.AddPostgres("postgres", userName: pass, password: pass)
    //.WithEndpointProxySupport(false)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("webdb2");

var webapi = builder.AddProject<Projects.WebAPI>("webapi")
    .WithReference(mongodb)
    .WithReference(postgres)
    .WaitFor(mongodb)
    .WaitFor(postgres);

var webapi2 = builder.AddProject<Projects.WebAPI2>("webapi2")
    .WithReference(postgres)
    .WaitFor(postgres);

// Add Envoy proxy as API Gateway
// http://localhost:10000/api1/
// http://localhost:10000/api2/
var envoy = builder.AddContainer("envoy", "envoyproxy/envoy", "v1.36-latest")
    .WithHttpEndpoint(port: 10000, targetPort: 10000, name: "proxy")
    .WithHttpEndpoint(port: 9901, targetPort: 9901, name: "admin")
    .WithBindMount("../AppHost/envoy.yaml", "/etc/envoy/envoy.yaml")
    .WithEnvironment(context =>
    {
        // Get the allocated endpoints for webapi and webapi2
        var webapiEndpoint = webapi.GetEndpoint("http");
        var webapi2Endpoint = webapi2.GetEndpoint("http");
        
        context.EnvironmentVariables["WEBAPI_PORT"] = webapiEndpoint.Property(EndpointProperty.Port);
        context.EnvironmentVariables["WEBAPI2_PORT"] = webapi2Endpoint.Property(EndpointProperty.Port);
    })
    .WaitFor(webapi)
    .WaitFor(webapi2);

builder.Build().Run();
