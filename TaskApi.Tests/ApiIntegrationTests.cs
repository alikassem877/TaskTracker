using System.Net.Http.Json;
using System.Text.Json;
using System.Net;

public class ApiIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
    }

[Fact]
public async Task GetTasks_ReturnsCorrectPaginationMetadata()
{
    // Arrange
    var email = $"pagination-{Guid.NewGuid()}@example.com";
    const string password = "StrongPassword123!";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    var loginData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
    var token = loginData.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await _client.GetAsync("/api/tasks?page=1&pageSize=5");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(1, result.GetProperty("page").GetInt32());
    Assert.Equal(5, result.GetProperty("pageSize").GetInt32());
    Assert.Equal(0, result.GetProperty("totalCount").GetInt32());
    Assert.Empty(result.GetProperty("items").EnumerateArray());
}


[Fact]
public async Task GetTasks_ReturnsCorrectPagesWithDistinctItems()
{
    // Arrange
    var email = $"pagination-{Guid.NewGuid()}@example.com";
    const string password = "StrongPassword123!";

    var registerResponse = await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    var loginData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
    var token = loginData.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Create 3 tasks
    for (var i = 1; i <= 3; i++)
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/tasks",
            new { Title = $"Pagination Task {i}", Priority = 1 });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    }

    // Act
    var page1Response = await _client.GetAsync(
        "/api/tasks?page=1&pageSize=2");

    var page2Response = await _client.GetAsync(
        "/api/tasks?page=2&pageSize=2");

    // Assert
    Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
    Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

    var page1 = await page1Response.Content.ReadFromJsonAsync<JsonElement>();
    var page2 = await page2Response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(1, page1.GetProperty("page").GetInt32());
    Assert.Equal(2, page2.GetProperty("page").GetInt32());

    Assert.Equal(3, page1.GetProperty("totalCount").GetInt32());
    Assert.Equal(3, page2.GetProperty("totalCount").GetInt32());

    var page1Items = page1.GetProperty("items").EnumerateArray().ToList();
    var page2Items = page2.GetProperty("items").EnumerateArray().ToList();

    Assert.Equal(2, page1Items.Count);
    Assert.Single(page2Items);

    var page1Ids = page1Items
        .Select(item => item.GetProperty("id").GetInt32())
        .ToHashSet();

    var page2Ids = page2Items
        .Select(item => item.GetProperty("id").GetInt32())
        .ToHashSet();

    Assert.Empty(page1Ids.Intersect(page2Ids));
}


[Fact]
public async Task GetTasks_Search_ReturnsOnlyMatchingTasks()
{
    var email = $"search-{Guid.NewGuid()}@example.com";
    const string password = "StrongPassword123!";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    var loginData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
    var token = loginData.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    await _client.PostAsJsonAsync("/api/tasks",
        new { Title = "Team Meeting", Priority = 1 });

    await _client.PostAsJsonAsync("/api/tasks",
        new { Title = "Buy groceries", Priority = 1 });

    await _client.PostAsJsonAsync("/api/tasks",
        new { Title = "100% Complete", Priority = 1 });

    await _client.PostAsJsonAsync("/api/tasks",
        new { Title = "Task_One", Priority = 1 });

    // Case-insensitive search
    var response = await _client.GetAsync("/api/tasks?search=mEeTiNg");
    var result = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(1, result.GetProperty("totalCount").GetInt32());
    Assert.Equal("Team Meeting",
        result.GetProperty("items")[0].GetProperty("title").GetString());

    // Percent must be treated literally
    response = await _client.GetAsync("/api/tasks?search=%25");
    result = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(1, result.GetProperty("totalCount").GetInt32());
    Assert.Equal("100% Complete",
        result.GetProperty("items")[0].GetProperty("title").GetString());

    // Underscore must be treated literally
    response = await _client.GetAsync("/api/tasks?search=%5F");
    result = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(1, result.GetProperty("totalCount").GetInt32());
    Assert.Equal("Task_One",
        result.GetProperty("items")[0].GetProperty("title").GetString());
}


[Fact]
public async Task GetTasks_PageSizeAboveMaximum_IsCappedAt50()
{
    var email = $"pagesize-{Guid.NewGuid()}@example.com";
    const string password = "StrongPassword123!";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    var loginData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
    var token = loginData.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await _client.GetAsync("/api/tasks?pageSize=100");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(50, result.GetProperty("pageSize").GetInt32());
}


[Fact]
public async Task TaskOwner_CanCreateCompleteAndDeleteTask()
{
    var email = $"crud-{Guid.NewGuid()}@example.com";
    const string password = "StrongPassword123!";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    var loginData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
    var token = loginData.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Create
    var createResponse = await _client.PostAsJsonAsync(
        "/api/tasks",
        new { Title = "CRUD integration test", Priority = 1 });

    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

    var createdTask = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
    var taskId = createdTask.GetProperty("id").GetInt32();

    // Complete
    var completeResponse = await _client.PutAsync(
    $"/api/tasks/{taskId}/complete", null);

    Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

    // Delete
    var deleteResponse = await _client.DeleteAsync($"/api/tasks/{taskId}");

    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    // Verify deletion
    var getResponse = await _client.GetAsync($"/api/tasks/{taskId}");

    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
}


[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    var email = $"login-{Guid.NewGuid()}@example.com";
    var password = "StrongPassword123!";

    var registerResponse = await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

    var result = await loginResponse.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    Assert.Equal(email, result.GetProperty("email").GetString());

    var token = result.GetProperty("token").GetString();

    Assert.False(string.IsNullOrWhiteSpace(token));
}


[Fact]
public async Task GetTasks_WithoutToken_ReturnsUnauthorized()
{
    var response = await _client.GetAsync("/api/tasks");

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task Login_ExceedingRateLimit_ReturnsTooManyRequests()
{
    // Arrange
    var email = $"ratelimit-{Guid.NewGuid()}@example.com";
    var password = "StrongPassword123!";

    var registerResponse = await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password }
    );

    Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

    // Act & Assert: First 5 login attempts should succeed
    for (var i = 0; i < 5; i++)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { Email = email, Password = password }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Sixth attempt should be rate limited
    var sixthResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password }
    );

    Assert.Equal(
        HttpStatusCode.TooManyRequests,
        sixthResponse.StatusCode
    );
}

[Fact]
public async Task GetTasks_WithValidToken_ReturnsOk()
{
    var email = $"protected-{Guid.NewGuid()}@example.com";
    var password = "StrongPassword123!";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = email, Password = password });

    var loginResponse = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = email, Password = password });

    var loginResult = await loginResponse.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var token = loginResult.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            token);

    var response = await _client.GetAsync("/api/tasks");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}


[Fact]
public async Task GetTask_BelongingToAnotherUser_ReturnsNotFound()
{
    const string password = "StrongPassword123!";

    // Register User A
    var emailA = $"owner-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailA, Password = password });

    // Login User A
    var loginA = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailA, Password = password });

    var resultA = await loginA.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenA = resultA.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenA);

    // User A creates a task
    var createResponse = await _client.PostAsJsonAsync(
        "/api/tasks",
        new { Title = "User A private task", Priority = 1 });

    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

    var createdTask = await createResponse.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var taskId = createdTask.GetProperty("id").GetInt32();

    // Register and login User B
    var emailB = $"other-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailB, Password = password });

    var loginB = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailB, Password = password });

    var resultB = await loginB.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenB = resultB.GetProperty("token").GetString();

    // Switch to User B's token
    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenB);

    // User B attempts to access User A's task
    var response = await _client.GetAsync($"/api/tasks/{taskId}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}


[Fact]
public async Task DeleteTask_BelongingToAnotherUser_ReturnsNotFound()
{
    const string password = "StrongPassword123!";

    // Register and login User A
    var emailA = $"delete-owner-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailA, Password = password });

    var loginA = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailA, Password = password });

    var resultA = await loginA.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenA = resultA.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenA);

    // User A creates a task
    var createResponse = await _client.PostAsJsonAsync(
        "/api/tasks",
        new { Title = "Private task", Priority = 1 });

    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

    var createdTask = await createResponse.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var taskId = createdTask.GetProperty("id").GetInt32();

    // Register and login User B
    var emailB = $"delete-other-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailB, Password = password });

    var loginB = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailB, Password = password });

    var resultB = await loginB.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenB = resultB.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenB);

    // User B attempts to delete User A's task
    var response = await _client.DeleteAsync(
        $"/api/tasks/{taskId}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}


[Fact]
public async Task CompleteTask_BelongingToAnotherUser_ReturnsNotFound()
{
    const string password = "StrongPassword123!";

    var emailA = $"complete-owner-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailA, Password = password });

    var loginA = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailA, Password = password });

    var resultA = await loginA.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenA = resultA.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenA);

    var createResponse = await _client.PostAsJsonAsync(
        "/api/tasks",
        new { Title = "Private task", Priority = 1 });

    var createdTask = await createResponse.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var taskId = createdTask.GetProperty("id").GetInt32();

    var emailB = $"complete-other-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailB, Password = password });

    var loginB = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailB, Password = password });

    var resultB = await loginB.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenB = resultB.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenB);

    var response = await _client.PutAsync(
        $"/api/tasks/{taskId}/complete",
        null);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}


[Fact]
public async Task GetTasks_ReturnsOnlyCurrentUsersTasks()
{
    const string password = "StrongPassword123!";

    // User A
    var emailA = $"list-owner-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailA, Password = password });

    var loginA = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailA, Password = password });

    var resultA = await loginA.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenA = resultA.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenA);

    await _client.PostAsJsonAsync(
        "/api/tasks",
        new { Title = "User A task", Priority = 1 });

    // User B
    var emailB = $"list-other-{Guid.NewGuid()}@example.com";

    await _client.PostAsJsonAsync(
        "/api/auth/register",
        new { Email = emailB, Password = password });

    var loginB = await _client.PostAsJsonAsync(
        "/api/auth/login",
        new { Email = emailB, Password = password });

    var resultB = await loginB.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var tokenB = resultB.GetProperty("token").GetString();

    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenB);

    await _client.PostAsJsonAsync(
        "/api/tasks",
        new { Title = "User B task", Priority = 1 });

    // Verify User B only sees their own task
    var response = await _client.GetAsync("/api/tasks");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    var items = result.GetProperty("items");

    Assert.Equal(1, items.GetArrayLength());
    Assert.Equal(
        "User B task",
        items[0].GetProperty("title").GetString());
}


[Fact]
public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
{
    var request = new
    {
        Email = $"unknown-{Guid.NewGuid()}@example.com",
        Password = "WrongPassword123!"
    };

    var response = await _client.PostAsJsonAsync(
        "/api/auth/login",
        request);

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}


[Fact]
public async Task Register_WithDuplicateEmail_ReturnsConflict()
{
    var request = new
    {
        Email = $"duplicate-{Guid.NewGuid()}@example.com",
        Password = "StrongPassword123!"
    };

    var firstResponse = await _client.PostAsJsonAsync(
        "/api/auth/register",
        request);

    var secondResponse = await _client.PostAsJsonAsync(
        "/api/auth/register",
        request);

    Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
    Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
}

public async Task DisposeAsync()
{
    _client.Dispose();
    await _factory.DisposeAsync();
}

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


[Fact]
public async Task Register_WithValidData_ReturnsCreated()
{
    var request = new
    {
        Email = $"test-{Guid.NewGuid()}@example.com",
        Password = "StrongPassword123!"
    };

    var response = await _client.PostAsJsonAsync(
        "/api/auth/register",
        request);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<
        System.Text.Json.JsonElement>();

    Assert.True(result.GetProperty("id").GetInt32() > 0);

    Assert.Equal(
        request.Email,
        result.GetProperty("email").GetString());
}

}
