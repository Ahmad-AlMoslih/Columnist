using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Columnist.Auth.Security.Jwt;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Columnist.IntegrationTests.Api;

// Read-only: only GET endpoints and read procedures. Nothing is written to the database.
public class TasksReadTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AdminEmail = "admin@example.com";   // change to your admin's email

    private readonly WebApplicationFactory<Program> _factory;

    public TasksReadTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_all_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_all_with_invalid_token_returns_401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_all_as_admin_returns_200_and_success()
    {
        var client = await ClientForAsync(AdminEmail);

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Get_by_id_of_existing_task_returns_200()
    {
        var taskId = await FirstTaskIdAsync();
        var client = await ClientForAsync(AdminEmail);

        var response = await client.GetAsync($"/api/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_by_id_of_missing_task_returns_404()
    {
        var client = await ClientForAsync(AdminEmail);

        var response = await client.GetAsync($"/api/tasks/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Builds a JWT for a real user without logging in (login would write a refresh token).
    private async Task<HttpClient> ClientForAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbExecutor>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);
        var user = await db.QuerySingleOrDefaultAsync<UserCredentialsDto>("dbo.GetUserByEmail", parameters);
        Assert.NotNull(user);

        var token = jwt.CreateToken(user.Id, user.Username, user.FullName, user.SessionVersion);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // First task on a board the admin can open (the procedure returns nothing without @UserId or @BoardId).
    private async Task<int> FirstTaskIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbExecutor>();

        var userParameters = new DynamicParameters();
        userParameters.Add("@Email", AdminEmail);
        var admin = await db.QuerySingleOrDefaultAsync<UserCredentialsDto>("dbo.GetUserByEmail", userParameters);
        Assert.NotNull(admin);

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", admin.Id);
        var tasks = await db.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", parameters);
        Assert.NotEmpty(tasks);
        return tasks[0].Id;
    }
}