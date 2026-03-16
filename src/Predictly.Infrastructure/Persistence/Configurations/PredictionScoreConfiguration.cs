using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class PredictionScoreConfiguration : IEntityTypeConfiguration<PredictionScore>
{
    public void Configure(EntityTypeBuilder<PredictionScore> builder)
    {
        builder.ToTable("prediction_scores");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(p => p.PredictionId).HasColumnName("prediction_id");
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.MatchId).HasColumnName("match_id");
        builder.Property(p => p.TournamentId).HasColumnName("tournament_id");
        builder.Property(p => p.WinnerScore).HasColumnName("winner_score").HasDefaultValue(0);
        builder.Property(p => p.BonusScore1).HasColumnName("bonus_score_1").HasDefaultValue(0);
        builder.Property(p => p.BonusScore2).HasColumnName("bonus_score_2").HasDefaultValue(0);
        builder.Property(p => p.BonusScore3).HasColumnName("bonus_score_3").HasDefaultValue(0);
        builder.Property(p => p.BonusAnsweredCount).HasColumnName("bonus_answered_count").HasDefaultValue((short)0);
        builder.Property(p => p.ScoredAt).HasColumnName("scored_at").HasColumnType("timestamptz");

        // DB-generated computed column
        builder.Property(p => p.TotalScore)
            .HasColumnName("total_score")
            .HasComputedColumnSql("winner_score + bonus_score_1 + bonus_score_2 + bonus_score_3", stored: true);

        builder.HasOne(p => p.Prediction)
            .WithOne(pred => pred.Score)
            .HasForeignKey<PredictionScore>(p => p.PredictionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Match)
            .WithMany()
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tournament)
            .WithMany()
            .HasForeignKey(p => p.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId).HasDatabaseName("idx_ps_user_id");
        builder.HasIndex(p => p.MatchId).HasDatabaseName("idx_ps_match_id");
        builder.HasIndex(p => p.TournamentId).HasDatabaseName("idx_ps_tournament_id");
    }
}
