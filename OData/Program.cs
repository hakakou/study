using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using OData.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHybridCache();

// Build the EDM model using conventions
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Event>("Events");

// Define the bound function to collection
var function = modelBuilder.EntityType<Event>()
    .Collection
    .Function("GetEventsInDateRange")
    .ReturnsFromEntitySet<Event>("Events");
function.Parameter<DateOnly>("startDate");
function.Parameter<DateOnly>("endDate");
function.Parameter<TimeOnly?>("preferredTime");

var edmModel = modelBuilder.GetEdmModel();

// Add OData services with query features
builder.Services.AddControllers()
    .AddOData(options => options
        .EnableQueryFeatures(maxTopValue: 20)
        .AddRouteComponents("odata", edmModel));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
