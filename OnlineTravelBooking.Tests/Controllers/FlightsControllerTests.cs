using System.Net;
using FluentAssertions;
using OnlineTravelBooking.Tests.Infrastructure;

namespace OnlineTravelBooking.Tests.Controllers;

/// <summary>
/// Integration tests for FlightsController endpoints:
///   GET /api/flights
///   GET /api/flights/{id}
/// Runs against the real SQL Server database — no seeding.
/// </summary>
public class FlightsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FlightsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/flights");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithOriginFilter_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/flights?origin=LHR");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistingFlight_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/flights/999999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
