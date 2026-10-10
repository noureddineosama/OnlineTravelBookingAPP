using System.Net;
using System.Net.Http.Json;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using FluentAssertions;
using OnlineTravelBooking.Tests.Infrastructure;

namespace OnlineTravelBooking.Tests.Controllers;

/// <summary>
/// Integration tests for POST /api/auth/register and POST /api/auth/login.
/// Runs against the real SQL Server database — no seeding required.
/// </summary>
public sealed class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_Returns200AndToken()
    {
        var request = new
        {
            name     = "New User",
            email    = $"newuser_{Guid.NewGuid():N}@test.com",
            password = "ValidPass1!",
            roleId   = 1   // Passenger role must exist in real DB
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Data.Email.Should().Be(request.email);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var request = new
        {
            name     = "Bad Email User",
            email    = "not-an-email",
            password = "ValidPass1!",
            roleId   = 1
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var request = new
        {
            name     = "Short Pass",
            email    = $"short_{Guid.NewGuid():N}@test.com",
            password = "12",
            roleId   = 1
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithNonExistentRole_Returns400()
    {
        var request = new
        {
            name     = "Bad Role User",
            email    = $"badrole_{Guid.NewGuid():N}@test.com",
            password = "ValidPass1!",
            roleId   = 999
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ThenLoginWithSameCredentials_Returns200()
    {
        var email    = $"roundtrip_{Guid.NewGuid():N}@test.com";
        const string password = "RoundTrip1!";

        // Register first
        var registerReq = new { name = "Round Trip", email, password, roleId = 1 };
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        registerRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Then login
        var loginReq = new { email, password };
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", loginReq);

        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await loginRes.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        // Register a user first, then try to login with wrong password
        var email = $"wrongpass_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "WP User", email, password = "ValidPass1!", roleId = 1 });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = $"nobody_{Guid.NewGuid():N}@nowhere.com", password = "AnyPassword1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyBody_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
