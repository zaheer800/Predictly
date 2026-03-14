using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Predictly.Api.Tests.Fixtures;
using Predictly.Api.Tests.Helpers;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Api.Tests.Controllers;

public class PredictionsControllerTests : IClassFixture<PredictlyWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly PredictlyWebAppFactory _factory;
    private readonly Guid _userId = Guid.NewGuid();

    public PredictionsControllerTests(PredictlyWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.WithAuth(_userId);
    }

    private async Task<Match> SeedMatchAsync(string status = "Scheduled", DateTimeOffset? startTime = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PredictlyDbContext>();

        var tournament = new Tournament
        {
            Id = Guid.NewGuid(),
            Name = "Test Tournament",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var match = new Match
        {
            Id = Guid.NewGuid(),
            TournamentId = tournament.Id,
            TeamA = "India",
            TeamB = "Australia",
            MatchStartTime = startTime ?? DateTimeOffset.UtcNow.AddHours(2),
            Status = status
        };
        db.Tournaments.Add(tournament);
        db.Matches.Add(match);
        await db.SaveChangesAsync();
        return match;
    }

    [Fact]
    public async Task Upsert_BeforeLock_Returns200()
    {
        var match = await SeedMatchAsync();

        var request = new UpsertPredictionRequest(match.Id, "India", []);
        var response = await _client.PutAsJsonAsync("/api/v1/predictions", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var prediction = await response.Content.ReadFromJsonAsync<PredictionDto>();
        Assert.Equal("India", prediction!.PredictedWinner);
    }

    [Fact]
    public async Task Upsert_UpdatesPrediction_FinalizedAtChanges()
    {
        var match = await SeedMatchAsync();
        var request = new UpsertPredictionRequest(match.Id, "India", []);

        await _client.PutAsJsonAsync("/api/v1/predictions", request);

        var updateRequest = new UpsertPredictionRequest(match.Id, "Australia", []);
        var updateResponse = await _client.PutAsJsonAsync("/api/v1/predictions", updateRequest);

        var updated = await updateResponse.Content.ReadFromJsonAsync<PredictionDto>();
        Assert.Equal("Australia", updated!.PredictedWinner);
    }

    [Fact]
    public async Task Upsert_AfterMatchStart_Returns409()
    {
        // Match start time in the past → locked
        var match = await SeedMatchAsync(startTime: DateTimeOffset.UtcNow.AddHours(-1));

        var request = new UpsertPredictionRequest(match.Id, "India", []);
        var response = await _client.PutAsJsonAsync("/api/v1/predictions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_NoPrediction_Returns404()
    {
        var match = await SeedMatchAsync();
        var response = await _client.GetAsync($"/api/v1/predictions/my/{match.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListByMatch_BeforeCompletion_ReturnsEmpty()
    {
        var match = await SeedMatchAsync();
        var response = await _client.GetAsync($"/api/v1/predictions/match/{match.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<PredictionDto>>();
        Assert.Empty(list!);
    }
}
