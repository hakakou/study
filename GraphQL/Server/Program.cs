using Bogus;
using Microsoft.Extensions.DependencyInjection;
using NewsWeb;
using Server;
using Server.Types;

Randomizer.Seed = new Random(42);

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["NewsApiKey"] ?? throw new InvalidOperationException("NewsApiKey not found in configuration");

builder.Services.AddHostedService<TimedHostedService>();

builder.Services.AddHttpClient<NewsService>()
    .AddHttpMessageHandler(() => new ApiKeyHandler(apiKey));

builder.Services
   .AddGraphQLServer()

       //.AddDocumentFromFile("Types/Schema.graphql")

   .AddMutationType<Mutation>()
   .AddQueryType<Query>()
   .AddDefaultTransactionScopeHandler()
   .AddMutationConventions()
   .AddErrorInterfaceType<IUserError>()
   .AddSubscriptionType<Subscription>().AddInMemorySubscriptions();


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

// Subscriptions
// app.UseRouting();
app.UseWebSockets();
//

app.Run();
