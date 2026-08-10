using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UserManagementApi.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databasePath;

    private readonly Dictionary<string, string?>
        _previousEnvironmentVariables = new();


    public CustomWebApplicationFactory()
    {
        _databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"usermanagement-tests-{Guid.NewGuid()}.db");


        byte[] keyBytes =
            Enumerable
                .Range(1, 64)
                .Select(x => (byte)x)
                .ToArray();


        // Environment variables are available
        // BEFORE Program.cs starts reading configuration.

        SetEnvironmentVariable(
            "Jwt__Key",
            Convert.ToBase64String(keyBytes));

        SetEnvironmentVariable(
            "Jwt__Issuer",
            "UserManagementApi");

        SetEnvironmentVariable(
            "Jwt__Audience",
            "UserManagementApiClient");

        SetEnvironmentVariable(
            "Jwt__AccessTokenMinutes",
            "30");


        SetEnvironmentVariable(
            "SeedAdmin__Email",
            "admin@test.com");

        SetEnvironmentVariable(
            "SeedAdmin__Password",
            "Admin1234!");


        SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            $"Data Source={_databasePath}");
    }


    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }


    private void SetEnvironmentVariable(
        string name,
        string value)
    {
        _previousEnvironmentVariables[name] =
            Environment.GetEnvironmentVariable(name);

        Environment.SetEnvironmentVariable(
            name,
            value);
    }


    protected override void Dispose(
        bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }


        foreach (var variable
                 in _previousEnvironmentVariables)
        {
            Environment.SetEnvironmentVariable(
                variable.Key,
                variable.Value);
        }


        if (File.Exists(_databasePath))
        {
            try
            {
                File.Delete(_databasePath);
            }
            catch
            {
                // Ignore cleanup errors in test database.
            }
        }
    }
}
