using FluentAssertions;
using Xunit;

namespace Predictly.Tests.Unit;

/// <summary>
/// Acting as QA (BMAD): Validates leaderboard tiebreaker ordering from CLAUDE.md §12.3.
/// These tests document the expected RANK() window function behaviour in application terms.
/// </summary>
public class LeaderboardRankingTests
{
    private record Entry(int UserId, int TotalPoints, DateTime AvgFinalizedAt, int BonusParticipationCount);

    private static List<Entry> Rank(IEnumerable<Entry> entries) =>
        entries
            .OrderByDescending(e => e.TotalPoints)
            .ThenBy(e => e.AvgFinalizedAt)
            .ThenByDescending(e => e.BonusParticipationCount)
            .ThenBy(e => e.UserId)
            .ToList();

    [Fact]
    public void Ranking_SingleUser_RankOne()
    {
        var entries = new[] { new Entry(1, 50, DateTime.UtcNow, 3) };
        Rank(entries).First().UserId.Should().Be(1);
    }

    [Fact]
    public void Ranking_EqualPoints_EarlierFinalizedAt_Wins()
    {
        var earlier = new Entry(1, 50, new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), 3);
        var later   = new Entry(2, 50, new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc), 3);

        Rank([earlier, later]).First().UserId.Should().Be(1);
    }

    [Fact]
    public void Ranking_EqualPointsEqualTime_MoreBonusParticipation_Wins()
    {
        var t = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var more = new Entry(1, 50, t, 3);
        var less = new Entry(2, 50, t, 1);

        Rank([more, less]).First().UserId.Should().Be(1);
    }

    [Fact]
    public void Ranking_FullyEqual_LowerUserId_Wins()
    {
        var t = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var userA = new Entry(1, 50, t, 3);
        var userB = new Entry(2, 50, t, 3);

        Rank([userB, userA]).First().UserId.Should().Be(1);
    }

    [Fact]
    public void Ranking_MultipleUsers_CorrectOrder()
    {
        var t = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var entries = new[]
        {
            new Entry(3, 60, t, 2),
            new Entry(1, 80, t, 1),
            new Entry(2, 60, t.AddHours(-1), 2),  // same points as user 3 but earlier time
        };

        var ranked = Rank(entries);
        ranked[0].UserId.Should().Be(1);  // highest points
        ranked[1].UserId.Should().Be(2);  // same points as 3, earlier time
        ranked[2].UserId.Should().Be(3);
    }
}
