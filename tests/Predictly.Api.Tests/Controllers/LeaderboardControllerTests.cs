using System.Net;
using System.Net.Http.Json;
using Predictly.Api.Tests.Fixtures;
using Predictly.Api.Tests.Helpers;
using Predictly.Core.Models;

namespace Predictly.Api.Tests.Controllers;

public class LeaderboardControllerTests : IClassFixture<PredictlyWebAppFactory>
{
    private readonly HttpClient _client;

    public LeaderboardControllerTests(PredictlyWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.WithAuth(Guid.NewGuid());
    }

    [Fact]
    public async Task GlobalLeaderboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/leaderboard/global");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
        Assert.NotNull(entries);
    }

    [Fact]
    public async Task TournamentLeaderboard_UnknownTournament_ReturnsOkEmptyList()
    {
        var response = await _client.GetAsync($"/api/v1/leaderboard/tournament/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GlobalLeaderboard_Unauthenticated_Returns401()
    {
        var unauthClient = new HttpClient { BaseAddress = _client.BaseAddress };
        var response = await unauthClient.GetAsync("/api/v1/leaderboard/global");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
