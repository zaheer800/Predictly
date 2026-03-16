using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Infrastructure.Persistence.Configurations;

namespace Predictly.Infrastructure.Persistence;

public class PredictlyDbContext : DbContext
{
    public PredictlyDbContext(DbContextOptions<PredictlyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<BonusQuestionCatalog> BonusQuestionCatalog => Set<BonusQuestionCatalog>();
    public DbSet<TournamentBonusConfig> TournamentBonusConfigs => Set<TournamentBonusConfig>();
    public DbSet<MatchBonusSelection> MatchBonusSelections => Set<MatchBonusSelection>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<PredictionBonusAnswer> PredictionBonusAnswers => Set<PredictionBonusAnswer>();
    public DbSet<MatchBonusResult> MatchBonusResults => Set<MatchBonusResult>();
    public DbSet<PredictionScore> PredictionScores => Set<PredictionScore>();
    public DbSet<TournamentLeaderboard> TournamentLeaderboard => Set<TournamentLeaderboard>();
    public DbSet<GlobalLeaderboard> GlobalLeaderboard => Set<GlobalLeaderboard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentConfiguration());
        modelBuilder.ApplyConfiguration(new MatchConfiguration());
        modelBuilder.ApplyConfiguration(new BonusQuestionCatalogConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentBonusConfigConfiguration());
        modelBuilder.ApplyConfiguration(new MatchBonusSelectionConfiguration());
        modelBuilder.ApplyConfiguration(new PredictionConfiguration());
        modelBuilder.ApplyConfiguration(new PredictionBonusAnswerConfiguration());
        modelBuilder.ApplyConfiguration(new MatchBonusResultConfiguration());
        modelBuilder.ApplyConfiguration(new PredictionScoreConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentLeaderboardConfiguration());
        modelBuilder.ApplyConfiguration(new GlobalLeaderboardConfiguration());
    }
}
