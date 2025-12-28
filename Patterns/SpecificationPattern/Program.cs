using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Repositories;
using SpecificationPattern.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterEndpointsFromAssemblyContaining<Program>();

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging()
	    .UseNpgsql(connectionString, npgsqlOptions =>
	    {
		    npgsqlOptions.MigrationsHistoryTable("migrations_history", DbConsts.SchemaName);
	    })
	    .UseSnakeCaseNamingConvention();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();

// Create and seed a database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeedService.SeedAsync(dbContext);
}

await app.RunAsync();
