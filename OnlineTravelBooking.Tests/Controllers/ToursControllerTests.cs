using System.Net;
using System.Net.Http.Json;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using FluentAssertions;
using OnlineTravelBooking.Tests.Infrastructure;

namespace OnlineTravelBooking.Tests.Controllers;

/// <summary>
/// Integration tests for GET /api/tours and GET /api/tours/{id}.
/// Tours read endpoints are public (no auth required).
/// Create/Delete are Admin-only.
/// Runs against the real SQL Server database — no seeding.
/// </summary>
public sealed class ToursControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _anonClient;

    public ToursControllerTests(TestWebApplicationFactory factory)
    {
        _factory    = factory;
        _anonClient = factory.CreateClient();
    }

    // ── GET /api/tours ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Anonymous_Returns200()
    {
        var response = await _anonClient.GetAsync("/api/tours");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_Returns200WithPagination()
    {
        var response = await _anonClient.GetAsync("/api/tours?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_Returns200()
    {
        var response = await _anonClient.GetAsync("/api/tours?status=Active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithSearchTerm_Returns200()
    {
        var response = await _anonClient.GetAsync("/api/tours?search=tour");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/tours/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task GetById_NonExistentTour_ReturnsNon200()
    {
        var response = await _anonClient.GetAsync("/api/tours/99999999");

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    // ── POST /api/tours (Admin only) ──────────────────────────────────────────

    [Fact]
    public async Task Create_AsAnonymous_Returns401()
    {
        var body = new
        {
            Title           = "New Tour",
            Summary         = "A new tour",
            FullDescription = "Detailed description here",
            DurationDays    = 5,
            LocationId      = 1,
            Difficulty      = "easy",
            Status          = "Active"
        };

        var response = await _anonClient.PostAsJsonAsync("/api/tours", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsPassenger_Returns403()
    {
        // Register a passenger
        var email = $"passenger_{Guid.NewGuid():N}@test.com";
        await _factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { name = "Tour Passenger", email, password = "ValidPass1!", roleId = 1 });

        var client = _factory.CreateAuthenticatedClient(email);

        var body = new
        {
            Title           = "New Tour",
            Summary         = "A new tour",
            FullDescription = "Detailed description",
            DurationDays    = 5,
            LocationId      = 1,
            Difficulty      = "easy",
            Status          = "Active"
        };

        var response = await client.PostAsJsonAsync("/api/tours", body);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── DELETE /api/tours/{id} (Admin only) ───────────────────────────────────

    [Fact]
    public async Task Delete_AsAnonymous_Returns401()
    {
        var response = await _anonClient.DeleteAsync("/api/tours/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
