using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace Test.Infrastructure.Tests.WebApi;

[Collection("WebApi")]
public class WebApiTests : IAsyncLifetime
{
    private readonly HttpClient _client;

    public WebApiTests(WebApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    ValueTask IAsyncLifetime.InitializeAsync() => ValueTask.CompletedTask;
    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // ============================================================
    // Repos — Aggregate with child entities
    // ============================================================

    [Fact]
    public async Task CreateRepo_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/repos", new { Name = "my-repo" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("my-repo");
        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetRepos_ReturnsAllRepos()
    {
        await _client.PostAsJsonAsync("/repos", new { Name = "repo-list-1" });
        await _client.PostAsJsonAsync("/repos", new { Name = "repo-list-2" });

        var response = await _client.GetAsync("/repos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        body.Should().NotBeNull();
        body!.Length.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task AddRepoItem_ReturnsItemsCollection()
    {
        var createResponse = await _client.PostAsJsonAsync("/repos", new { Name = "repo-with-items" });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repoId = created.GetProperty("id").GetString();

        var response = await _client.PostAsJsonAsync($"/repos/{repoId}/items", new { Path = "src/Program.cs" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = body.GetProperty("items").EnumerateArray().ToList();
        items.Should().ContainSingle();
        items[0].GetProperty("path").GetString().Should().Be("src/Program.cs");
    }

    [Fact]
    public async Task AddRepoItem_NonExistentRepo_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync($"/repos/{Guid.NewGuid()}/items", new { Path = "nope.cs" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ============================================================
    // Issues — Domain service, domain events, specifications
    // ============================================================

    [Fact]
    public async Task CreateIssue_ViaManager_ReturnsCreated()
    {
        var repoResponse = await _client.PostAsJsonAsync("/repos", new { Name = "issue-repo" });
        var repo = await repoResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repoId = repo.GetProperty("id").GetString();

        var response = await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Bug #1" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Bug #1");
        body.GetProperty("repoId").GetString().Should().Be(repoId);
    }

    [Fact]
    public async Task GetIssues_ReturnsAllIssues()
    {
        var repoResponse = await _client.PostAsJsonAsync("/repos", new { Name = "issue-list-repo" });
        var repo = await repoResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repoId = repo.GetProperty("id").GetString();

        await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Issue A" });
        await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Issue B" });

        var response = await _client.GetAsync("/issues");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        body!.Length.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task AddLabel_ToIssue_ReturnsLabels()
    {
        var repoResponse = await _client.PostAsJsonAsync("/repos", new { Name = "label-repo" });
        var repo = await repoResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repoId = repo.GetProperty("id").GetString();

        var issueResponse = await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Label Issue" });
        var issue = await issueResponse.Content.ReadFromJsonAsync<JsonElement>();
        var issueId = issue.GetProperty("id").GetString();

        var response = await _client.PostAsJsonAsync($"/issues/{issueId}/labels", new { Name = "critical" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var labels = body.GetProperty("labels").EnumerateArray().ToList();
        labels.Should().ContainSingle();
        labels[0].GetProperty("name").GetString().Should().Be("critical");
    }

    [Fact]
    public async Task AssignIssueToUser_RaisesDomainEvent_ReturnsAssignedUserId()
    {
        var repoResponse = await _client.PostAsJsonAsync("/repos", new { Name = "assign-repo" });
        var repo = await repoResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repoId = repo.GetProperty("id").GetString();

        var issueResponse = await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Assign Issue" });
        var issue = await issueResponse.Content.ReadFromJsonAsync<JsonElement>();
        var issueId = issue.GetProperty("id").GetString();

        var userResponse = await _client.PostAsJsonAsync("/users", new { UserName = "assignee" });
        var user = await userResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = user.GetProperty("id").GetString();

        var response = await _client.PostAsync($"/issues/{issueId}/assign/{userId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("assignedUserId").GetString().Should().Be(userId);
    }

    [Fact]
    public async Task GetInactiveIssues_ReturnsUnassignedOnly()
    {
        var repoResponse = await _client.PostAsJsonAsync("/repos", new { Name = "inactive-repo" });
        var repo = await repoResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repoId = repo.GetProperty("id").GetString();

        // Create two issues
        var issue1Resp = await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Inactive Issue" });
        var issue1 = await issue1Resp.Content.ReadFromJsonAsync<JsonElement>();

        var issue2Resp = await _client.PostAsJsonAsync("/issues", new { RepoId = repoId, Name = "Active Issue" });
        var issue2 = await issue2Resp.Content.ReadFromJsonAsync<JsonElement>();
        var issue2Id = issue2.GetProperty("id").GetString();

        // Assign one issue to a user
        var userResp = await _client.PostAsJsonAsync("/users", new { UserName = "active-user" });
        var user = await userResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = user.GetProperty("id").GetString();
        await _client.PostAsync($"/issues/{issue2Id}/assign/{userId}", null);

        // Get inactive (specification-based query)
        var response = await _client.GetAsync("/issues/inactive");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        body.Should().NotBeNull();
        body!.Should().Contain(e => e.GetProperty("name").GetString() == "Inactive Issue");
        body!.Should().NotContain(e => e.GetProperty("name").GetString() == "Active Issue");
    }

    // ============================================================
    // Users — SmartEnum, value objects
    // ============================================================

    [Fact]
    public async Task CreateUser_DefaultsToFreeType()
    {
        var response = await _client.PostAsJsonAsync("/users", new { UserName = "newuser" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("userName").GetString().Should().Be("newuser");
        body.GetProperty("userType").GetString().Should().Be("Free");
    }

    [Fact]
    public async Task SetUserAddress_ValueObject_ReturnsAddress()
    {
        var createResp = await _client.PostAsJsonAsync("/users", new { UserName = "address-user" });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = created.GetProperty("id").GetString();

        var response = await _client.PutAsJsonAsync($"/users/{userId}/address", new
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62704",
            Country = "US"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var address = body.GetProperty("address");
        address.GetProperty("street").GetString().Should().Be("123 Main St");
        address.GetProperty("city").GetString().Should().Be("Springfield");
        address.GetProperty("state").GetString().Should().Be("IL");
    }

    [Fact]
    public async Task ChangeUserType_SmartEnum_ValidTransition()
    {
        var createResp = await _client.PostAsJsonAsync("/users", new { UserName = "upgrade-user" });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = created.GetProperty("id").GetString();

        // Free -> Paid is allowed
        var response = await _client.PutAsync($"/users/{userId}/type/Paid", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("userType").GetString().Should().Be("Paid");
        body.GetProperty("allowedProjects").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task ChangeUserType_SmartEnum_InvalidTransition_ReturnsBadRequest()
    {
        var createResp = await _client.PostAsJsonAsync("/users", new { UserName = "admin-fail-user" });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = created.GetProperty("id").GetString();

        // Free -> Admin is not allowed
        var response = await _client.PutAsync($"/users/{userId}/type/Admin", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeUserType_SmartEnum_UnknownType_ReturnsBadRequest()
    {
        var createResp = await _client.PostAsJsonAsync("/users", new { UserName = "unknown-type-user" });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = created.GetProperty("id").GetString();

        var response = await _client.PutAsync($"/users/{userId}/type/SuperAdmin", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ============================================================
    // Orders — Enum status, validation
    // ============================================================

    [Fact]
    public async Task CreateOrder_ReturnsCreatedWithPendingStatus()
    {
        var response = await _client.PostAsJsonAsync("/orders", new { OrderNumber = "ORD-001", TotalAmount = 99.99m });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("orderNumber").GetString().Should().Be("ORD-001");
        body.GetProperty("status").GetString().Should().Be("Pending");
        body.GetProperty("totalAmount").GetDecimal().Should().Be(99.99m);
    }

    [Fact]
    public async Task UpdateOrderStatus_ChangesStatus()
    {
        var createResp = await _client.PostAsJsonAsync("/orders", new { OrderNumber = "ORD-002", TotalAmount = 50m });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = created.GetProperty("id").GetInt64();

        var response = await _client.PutAsync($"/orders/{orderId}/status/{(int)Test.Domain.AggregateModel.OrderAggregate.OrderStatus.Shipped}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().Should().Be("Shipped");
    }

    [Fact]
    public async Task GetOrders_ReturnsAllOrders()
    {
        await _client.PostAsJsonAsync("/orders", new { OrderNumber = "ORD-LIST-1", TotalAmount = 10m });

        var response = await _client.GetAsync("/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        body.Should().NotBeNull();
        body!.Length.Should().BeGreaterThanOrEqualTo(1);
    }
}
