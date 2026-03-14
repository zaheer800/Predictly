using Microsoft.EntityFrameworkCore;
using Predictly.Infrastructure.Entities;

namespace Predictly.Infrastructure.Persistence;

public class PredictlyDbContext(DbContextOptions<PredictlyDbContext> options) : DbContext(options)
{
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
        // ── users ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        });

        // ── tournaments ────────────────────────────────────────────────────
        modelBuilder.Entity<Tournament>(e =>
        {
            e.ToTable("tournaments");
            e.HasKey(t => t.Id);
            e.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Draft");
            e.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        });

        // ── matches ────────────────────────────────────────────────────────
        modelBuilder.Entity<Match>(e =>
        {
            e.ToTable("matches");
            e.HasKey(m => m.Id);
            e.Property(m => m.Status).HasMaxLength(20).HasDefaultValue("Scheduled");
            e.HasOne(m => m.Tournament)
             .WithMany(t => t.Matches)
             .HasForeignKey(m => m.TournamentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── bonus_question_catalog ─────────────────────────────────────────
        modelBuilder.Entity<BonusQuestionCatalog>(e =>
        {
            e.ToTable("bonus_question_catalog");
            e.HasKey(b => b.Id);
            e.Property(b => b.QuestionType).HasMaxLength(20);
        });

        // ── tournament_bonus_config ────────────────────────────────────────
        modelBuilder.Entity<TournamentBonusConfig>(e =>
        {
            e.ToTable("tournament_bonus_config");
            e.HasKey(c => new { c.TournamentId, c.BonusQuestionCatalogId });
            e.HasOne(c => c.Tournament)
             .WithMany(t => t.BonusConfigs)
             .HasForeignKey(c => c.TournamentId);
            e.HasOne(c => c.BonusQuestion)
             .WithMany(b => b.TournamentConfigs)
             .HasForeignKey(c => c.BonusQuestionCatalogId);
        });

        // ── match_bonus_selection ──────────────────────────────────────────
        modelBuilder.Entity<MatchBonusSelection>(e =>
        {
            e.ToTable("match_bonus_selection");
            e.HasKey(s => new { s.MatchId, s.BonusQuestionCatalogId });
            e.HasOne(s => s.Match)
             .WithMany(m => m.BonusSelections)
             .HasForeignKey(s => s.MatchId);
            e.HasOne(s => s.BonusQuestion)
             .WithMany(b => b.MatchSelections)
             .HasForeignKey(s => s.BonusQuestionCatalogId);
        });

        // ── predictions ────────────────────────────────────────────────────
        modelBuilder.Entity<Prediction>(e =>
        {
            e.ToTable("predictions");
            e.HasKey(p => p.Id);
            e.HasIndex(p => new { p.UserId, p.MatchId }).IsUnique();
            e.HasOne(p => p.User)
             .WithMany(u => u.Predictions)
             .HasForeignKey(p => p.UserId);
            e.HasOne(p => p.Match)
             .WithMany(m => m.Predictions)
             .HasForeignKey(p => p.MatchId);
        });

        // ── prediction_bonus_answers ───────────────────────────────────────
        modelBuilder.Entity<PredictionBonusAnswer>(e =>
        {
            e.ToTable("prediction_bonus_answers");
            e.HasKey(a => new { a.PredictionId, a.BonusQuestionCatalogId });
            e.HasOne(a => a.Prediction)
             .WithMany(p => p.BonusAnswers)
             .HasForeignKey(a => a.PredictionId);
            e.HasOne(a => a.BonusQuestion)
             .WithMany()
             .HasForeignKey(a => a.BonusQuestionCatalogId);
        });

        // ── match_bonus_results ────────────────────────────────────────────
        modelBuilder.Entity<MatchBonusResult>(e =>
        {
            e.ToTable("match_bonus_results");
            e.HasKey(r => new { r.MatchId, r.BonusQuestionCatalogId });
            e.HasOne(r => r.Selection)
             .WithMany(s => s.Results)
             .HasForeignKey(r => new { r.MatchId, r.BonusQuestionCatalogId });
        });

        // ── prediction_scores ──────────────────────────────────────────────
        modelBuilder.Entity<PredictionScore>(e =>
        {
            e.ToTable("prediction_scores");
            e.HasKey(s => s.PredictionId);
            e.HasOne(s => s.Prediction)
             .WithOne(p => p.Score)
             .HasForeignKey<PredictionScore>(s => s.PredictionId);
            e.HasOne(s => s.Match)
             .WithMany(m => m.PredictionScores)
             .HasForeignKey(s => s.MatchId);
        });

        // ── tournament_leaderboard ─────────────────────────────────────────
        modelBuilder.Entity<TournamentLeaderboard>(e =>
        {
            e.ToTable("tournament_leaderboard");
            e.HasKey(l => new { l.TournamentId, l.UserId });
            e.HasOne(l => l.Tournament)
             .WithMany(t => t.Leaderboard)
             .HasForeignKey(l => l.TournamentId);
            e.HasOne(l => l.User)
             .WithMany(u => u.TournamentLeaderboardEntries)
             .HasForeignKey(l => l.UserId);
        });

        // ── global_leaderboard ─────────────────────────────────────────────
        modelBuilder.Entity<GlobalLeaderboard>(e =>
        {
            e.ToTable("global_leaderboard");
            e.HasKey(l => l.UserId);
            e.HasOne(l => l.User)
             .WithMany(u => u.GlobalLeaderboardEntries)
             .HasForeignKey(l => l.UserId);
        });
    }
}
