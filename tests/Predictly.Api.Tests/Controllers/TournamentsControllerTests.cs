using System.Net;
using System.Net.Http.Json;
using Predictly.Api.Tests.Fixtures;
using Predictly.Api.Tests.Helpers;
using Predictly.Core.Models;

namespace Predictly.Api.Tests.Controllers;

public class TournamentsControllerTests : IClassFixture<PredictlyWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public TournamentsControllerTests(PredictlyWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/tournaments");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_Authenticated_Returns200()
    {
        _client.WithAuth(_userId);
        var response = await _client.GetAsync("/api/v1/tournaments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsUser_Returns403()
    {
        _client.WithAuth(_userId, "User");
        var response = await _client.PostAsJsonAsync("/api/v1/tournaments",
            new CreateTournamentRequest("IPL 2025"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_Returns201WithTournament()
    {
        _client.WithAuth(_adminId, "Admin");
        var response = await _client.PostAsJsonAsync("/api/v1/tournaments",
            new CreateTournamentRequest("IPL 2025"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tournament = await response.Content.ReadFromJsonAsync<TournamentDto>();
        Assert.NotNull(tournament);
        Assert.Equal("IPL 2025", tournament.Name);
        Assert.Equal("Draft", tournament.Status);
    }

    [Fact]
    public async Task Get_NonExistent_Returns404()
    {
        _client.WithAuth(_userId);
        var response = await _client.GetAsync($"/api/v1/tournaments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ChangesDraftToActive()
    {
        _client.WithAuth(_adminId, "Admin");

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/tournaments",
            new CreateTournamentRequest("Test Cup"));
        var tournament = await createResponse.Content.ReadFromJsonAsync<TournamentDto>();

        // Activate
        var activateResponse = await _client.PostAsync(
            $"/api/v1/tournaments/{tournament!.Id}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        // Verify status
        var getResponse = await _client.GetAsync($"/api/v1/tournaments/{tournament.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TournamentDto>();
        Assert.Equal("Active", updated!.Status);
    }

    [Fact]
    public async Task Activate_AlreadyActive_Returns422()
    {
        _client.WithAuth(_adminId, "Admin");

        var createResponse = await _client.PostAsJsonAsync("/api/v1/tournaments",
            new CreateTournamentRequest("Double Activate Cup"));
        var tournament = await createResponse.Content.ReadFromJsonAsync<TournamentDto>();

        await _client.PostAsync($"/api/v1/tournaments/{tournament!.Id}/activate", null);

        // Second activation should fail
        var secondActivate = await _client.PostAsync(
            $"/api/v1/tournaments/{tournament.Id}/activate", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, secondActivate.StatusCode);
    }
}
