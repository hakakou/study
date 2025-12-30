using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var mongoConnectionString = builder.Configuration.GetConnectionString("webdb")!;
builder.Services.AddDbContext<PlanetDbContext>(options =>
{
    options.UseMongoDB(mongoConnectionString, "webdb");
});

builder.Services.Configure<MyOptions>(builder.Configuration.GetSection("MyOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // https://localhost:7128/openapi/v1.json
    app.MapOpenApi().CacheOutput();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// https://github.com/Redocly/redoc
// Swashbuckle.AspNetCore.ReDoc
app.UseReDoc(options =>
{
    options.DocumentTitle = "WebAPI API Docs";
    options.RoutePrefix = "docs";
    options.SpecUrl("/openapi/v1.json");
});
//app.UseSwaggerUi(); // UseSwaggerUI Protected by if (env.IsDevelopment())

var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<PlanetDbContext>();
dbContext.Database.EnsureCreated();
app.Run();

public class MyOptions
{
    public string Option1 { get; set; }
}
