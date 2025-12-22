using Bogus;
using NewsWeb;
using Server.Types;
using System.Web;

Randomizer.Seed = new Random(42);

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["NewsApiKey"] ?? throw new InvalidOperationException("NewsApiKey not found in configuration");

builder.Services.AddHttpClient<NewsService>()
    .AddHttpMessageHandler(() => new ApiKeyHandler(apiKey));

builder.Services
   .AddGraphQLServer()
   //.AddDocumentFromFile("Types/Schema.graphql")
   //.BindRuntimeType<Query>();
   .AddMutationType<Mutation>()
   .AddQueryType<Query>();

builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder
                .WithOrigins("https://studio.apollographql.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

var app = builder.Build();
app.MapGraphQL();
app.UseCors();
app.Run();

public class ApiKeyHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(request.RequestUri!);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["api-key"] = apiKey;

        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return base.SendAsync(request, cancellationToken);
    }
}