using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UserManagementApi.Models;

namespace UserManagementApi.Tests;

public class SecurityTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SecurityTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response =
            await _client.GetAsync(
                "/api/users/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    [Fact]
    public async Task XssPayload_IsRejected()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    name =
                        "<script>alert('XSS')</script>",

                    email =
                        $"xss-{Guid.NewGuid()}@test.com",

                    password =
                        "Test1234!",

                    age = 25
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task SqlInjection_DoesNotReturnUsers()
    {
        string token =
            await LoginAsync(
                "admin@test.com",
                "Admin1234!");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/users/search?email=" +
                Uri.EscapeDataString(
                    "' OR 1=1 --"));

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }


    [Fact]
    public async Task RegularUser_CannotAccessAdminEndpoint()
    {
        string email =
            $"user-{Guid.NewGuid()}@test.com";

        await RegisterAsync(email);

        string token =
            await LoginAsync(
                email,
                "Test1234!");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }


    [Fact]
    public async Task Admin_CanAccessAdminEndpoint()
    {
        string token =
            await LoginAsync(
                "admin@test.com",
                "Admin1234!");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }


    [Fact]
    public async Task Password_IsStoredAsHash()
    {
        string email =
            $"hash-{Guid.NewGuid()}@test.com";

        const string password =
            "Test1234!";

        await RegisterAsync(
            email,
            password);

        using var scope =
            _factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        ApplicationUser? user =
            await userManager
                .FindByEmailAsync(email);

        Assert.NotNull(user);

        Assert.NotNull(
            user!.PasswordHash);

        Assert.NotEqual(
            password,
            user.PasswordHash);

        bool validPassword =
            await userManager
                .CheckPasswordAsync(
                    user,
                    password);

        Assert.True(validPassword);
    }


    [Fact]
    public async Task WrongPassword_Returns401()
    {
        string email =
            $"wrongpass-{Guid.NewGuid()}@test.com";

        await RegisterAsync(email);

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email,
                    password =
                        "WrongPassword123!"
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    private async Task RegisterAsync(
        string email,
        string password = "Test1234!")
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    name =
                        "Security Test User",

                    email,

                    password,

                    age = 25
                });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }


    private async Task<string> LoginAsync(
        string email,
        string password)
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email,
                    password
                });

        response.EnsureSuccessStatusCode();

        using JsonDocument json =
            JsonDocument.Parse(
                await response.Content
                    .ReadAsStringAsync());

        return json.RootElement
            .GetProperty("accessToken")
            .GetString()
            ?? throw new InvalidOperationException(
                "Access token was not returned.");
    }
}
