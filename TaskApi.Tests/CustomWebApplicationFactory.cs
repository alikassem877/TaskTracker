
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TaskTracker.Data;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        var secrets = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var originalConnectionString =
            secrets.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is missing from user secrets.");

        var testConnectionString =
            new NpgsqlConnectionStringBuilder(originalConnectionString)
            {
                Database = "TaskTrackerTestDb"
            }.ConnectionString;

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            testConnectionString);

        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    "TaskTrackerIntegrationTestJwtKey-32Bytes!!")));

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "TaskTracker.Tests");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "TaskTracker.Tests");

        Environment.SetEnvironmentVariable(
            "DOTNET_ENVIRONMENT",
            "Testing");

        Environment.SetEnvironmentVariable(
            "Frontend__Origin",
            "http://localhost:5173");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public async Task InitializeDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Database.GetDbConnection().Database != "TaskTrackerTestDb")
        {
            throw new InvalidOperationException(
                "Integration tests must use TaskTrackerTestDb.");
        }

        await db.Database.MigrateAsync();
    }
}