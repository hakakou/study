using Haka.Patterns.SeedWork;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Test.Domain.AggregateModel.IssueAggregate;
using Test.Domain.AggregateModel.OrderAggregate;
using Test.Domain.AggregateModel.RepoAggregate;
using Test.Domain.AggregateModel.UserAggregate;
using Test.Domain.DomainServices;
using Test.Domain.Specifications;
using Test.Infrastructure.Data;
using Test.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<TestDbContext>((sp, options) =>
{
    options.UseSqlite("Data Source=test.db");
    options.AddInterceptors(sp.GetRequiredService<EventDispatchInterceptor>());
});

// Mediator (source-generated)
builder.Services.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime.Scoped);

// Domain event dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
builder.Services.AddScoped<EventDispatchInterceptor>();

// Repositories
builder.Services.AddScoped<IRepository<Repo>, EfRepository<Repo>>();
builder.Services.AddScoped<IRepository<Issue>, EfRepository<Issue>>();
builder.Services.AddScoped<IRepository<User>, EfRepository<User>>();
builder.Services.AddScoped<IRepository<Order>, EfRepository<Order>>();

// Domain services
builder.Services.AddScoped<IssueManager>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
    db.Database.EnsureCreated();
}

// ============================================================
// Repo endpoints — Aggregate with child entities (RepoItem)
// ============================================================

var repos = app.MapGroup("/repos").WithTags("Repos");

repos.MapPost("/", async (CreateRepoRequest req, IRepository<Repo> repo) =>
{
    var newRepo = new Repo(req.Name);
    await repo.AddAsync(newRepo);
    await repo.SaveChangesAsync();
    return Results.Created($"/repos/{newRepo.Id}", new { newRepo.Id, newRepo.Name });
});

repos.MapGet("/", async (IRepository<Repo> repo) =>
{
    var all = await repo.ListAsync();
    return all.Select(r => new { r.Id, r.Name });
});

repos.MapPost("/{repoId}/items", async (Guid repoId, AddRepoItemRequest req, IRepository<Repo> repo) =>
{
    var r = await repo.GetByIdAsync(repoId);
    if (r is null) return Results.NotFound();
    r.AddRepoItem(req.Path);
    await repo.SaveChangesAsync();
    return Results.Ok(new { r.Id, Items = r.RepoItems.Select(i => new { i.Id, i.Path }) });
});

// ============================================================
// Issue endpoints — Aggregate root, value objects, domain events, domain service, specifications
// ============================================================

var issues = app.MapGroup("/issues").WithTags("Issues");

issues.MapPost("/", async (CreateIssueRequest req, IssueManager issueManager, IRepository<Issue> issueRepo) =>
{
    // Uses domain service to enforce duplicate-name rule, then raises IssueCreatedDomainEvent
    var issue = await issueManager.CreateAsync(req.RepoId, new IssueName(req.Name), DateTime.UtcNow);
    await issueRepo.AddAsync(issue);
    await issueRepo.SaveChangesAsync(); // Domain event dispatched via interceptor
    return Results.Created($"/issues/{issue.Id}", new { issue.Id, Name = issue.Name.Value, issue.RepoId });
});

issues.MapGet("/", async (IRepository<Issue> issueRepo) =>
{
    var all = await issueRepo.ListAsync();
    return all.Select(i => new { i.Id, Name = i.Name.Value, i.RepoId, i.AssignedUserId, i.Description });
});

issues.MapPost("/{issueId}/labels", async (Guid issueId, AddLabelRequest req, TestDbContext db) =>
{
    var exists = await db.Issues.AnyAsync(i => i.Id == issueId);
    if (!exists) return Results.NotFound();

    var label = new IssueLabel(Guid.NewGuid(), issueId, req.Name);
    db.Add(label);
    await db.SaveChangesAsync();

    var labels = await db.Issues
        .Where(i => i.Id == issueId)
        .SelectMany(i => i.Labels)
        .Select(l => new { l.Id, l.Name })
        .ToListAsync();

    return Results.Ok(new { Id = issueId, Labels = labels });
});

issues.MapPost("/{issueId}/assign/{userId}", async (Guid issueId, Guid userId, IRepository<Issue> issueRepo) =>
{
    var issue = await issueRepo.GetByIdAsync(issueId);
    if (issue is null) return Results.NotFound();
    // Raises IssueAssignedToUserDomainEvent
    issue.AssignToUser(userId);
    await issueRepo.SaveChangesAsync(); // Domain event dispatched via interceptor
    return Results.Ok(new { issue.Id, issue.AssignedUserId });
});

issues.MapGet("/inactive", async (IRepository<Issue> issueRepo) =>
{
    // Uses the InactiveIssueSpecification (issues with no assigned user)
    var spec = new InactiveIssueSpecification();
    var inactive = await issueRepo.ListAsync(spec);
    return inactive.Select(i => new { i.Id, Name = i.Name.Value, i.RepoId });
});

// ============================================================
// User endpoints — SmartEnum (UserType), value object (Address)
// ============================================================

var users = app.MapGroup("/users").WithTags("Users");

users.MapPost("/", async (CreateUserRequest req, IRepository<User> userRepo) =>
{
    var user = new User(Guid.NewGuid(), req.UserName);
    await userRepo.AddAsync(user);
    await userRepo.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", new { user.Id, user.UserName, UserType = user.UserType.Name });
});

users.MapGet("/", async (IRepository<User> userRepo) =>
{
    var all = await userRepo.ListAsync();
    return all.Select(u => new
    {
        u.Id,
        u.UserName,
        UserType = u.UserType.Name,
        AllowedProjects = u.UserType.AllowedProjects,
        u.Address
    });
});

users.MapPut("/{userId}/address", async (Guid userId, SetAddressRequest req, IRepository<User> userRepo) =>
{
    var user = await userRepo.GetByIdAsync(userId);
    if (user is null) return Results.NotFound();
    // Value object — immutable, validated via guard clauses
    user.SetAddress(new Address(req.Street, req.City, req.State, req.PostalCode, req.Country));
    await userRepo.SaveChangesAsync();
    return Results.Ok(new { user.Id, user.Address });
});

users.MapPut("/{userId}/type/{typeName}", async (Guid userId, string typeName, IRepository<User> userRepo) =>
{
    var user = await userRepo.GetByIdAsync(userId);
    if (user is null) return Results.NotFound();

    // SmartEnum — parse by name, enforce transition rules
    if (!UserType.TryFromName(typeName, ignoreCase: true, out var newType))
        return Results.BadRequest($"Unknown user type: {typeName}");

    if (!user.UserType.CanTransitionTo(newType))
        return Results.BadRequest($"Cannot transition from {user.UserType.Name} to {newType.Name}");

    user.SetUserType(newType);
    await userRepo.SaveChangesAsync();
    return Results.Ok(new { user.Id, UserType = user.UserType.Name, user.UserType.AllowedProjects });
});

// ============================================================
// Order endpoints — Simple aggregate with enum status and validation
// ============================================================

var orders = app.MapGroup("/orders").WithTags("Orders");

orders.MapPost("/", async (CreateOrderRequest req, IRepository<Order> orderRepo) =>
{
    var order = new Order(0, req.OrderNumber, DateTime.UtcNow);
    order.SetTotalAmount(req.TotalAmount);
    await orderRepo.AddAsync(order);
    await orderRepo.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", new { order.Id, order.OrderNumber, order.TotalAmount, Status = order.Status.ToString() });
});

orders.MapGet("/", async (IRepository<Order> orderRepo) =>
{
    var all = await orderRepo.ListAsync();
    return all.Select(o => new { o.Id, o.OrderNumber, o.TotalAmount, Status = o.Status.ToString(), o.OrderDate });
});

orders.MapPut("/{orderId}/status/{status}", async (long orderId, OrderStatus status, IRepository<Order> orderRepo) =>
{
    var order = await orderRepo.GetByIdAsync(orderId);
    if (order is null) return Results.NotFound();
    order.SetStatus(status);
    await orderRepo.SaveChangesAsync();
    return Results.Ok(new { order.Id, Status = order.Status.ToString() });
});

app.Run();

// ============================================================
// Request DTOs
// ============================================================

record CreateRepoRequest(string Name);
record AddRepoItemRequest(string Path);
record CreateIssueRequest(Guid RepoId, string Name);
record AddLabelRequest(string Name);
record CreateUserRequest(string UserName);
record SetAddressRequest(string Street, string City, string State, string PostalCode, string Country);
record CreateOrderRequest(string OrderNumber, decimal TotalAmount);

// Make the implicit Program class accessible for WebApplicationFactory<Program> in tests
public partial class Program;
