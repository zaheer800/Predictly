using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class TournamentLeaderboardConfiguration : IEntityTypeConfiguration<TournamentLeaderboard>
{
    public void Configure(EntityTypeBuilder<TournamentLeaderboard> builder)
    {
        builder.ToTable("tournament_leaderboard");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(t => t.TournamentId).HasColumnName("tournament_id");
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.TotalPoints).HasColumnName("total_points").HasDefaultValue(0);
        builder.Property(t => t.AvgFinalizedAt).HasColumnName("avg_finalized_at").HasColumnType("timestamptz");
        builder.Property(t => t.BonusParticipationCount).HasColumnName("bonus_participation_count").HasDefaultValue(0);
        builder.Property(t => t.Rank).HasColumnName("rank");
        builder.Property(t => t.LastUpdatedAt).HasColumnName("last_updated_at").HasColumnType("timestamptz");

        builder.HasOne(t => t.Tournament)
            .WithMany(tour => tour.Leaderboard)
            .HasForeignKey(t => t.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<TournamentLeaderboard>(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.TournamentId).HasDatabaseName("idx_tl_tournament_id");
        builder.HasIndex(new[] { "tournament_id", "rank" }).HasDatabaseName("idx_tl_rank");
        builder.HasIndex(new[] { "tournament_id", "user_id" })
            .HasDatabaseName("uq_tournament_leaderboard")
            .IsUnique();
    }
}
