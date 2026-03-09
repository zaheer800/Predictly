using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class GlobalLeaderboardConfiguration : IEntityTypeConfiguration<GlobalLeaderboard>
{
    public void Configure(EntityTypeBuilder<GlobalLeaderboard> builder)
    {
        builder.ToTable("global_leaderboard");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(g => g.UserId).HasColumnName("user_id");
        builder.Property(g => g.TotalPoints).HasColumnName("total_points").HasDefaultValue(0);
        builder.Property(g => g.AvgFinalizedAt).HasColumnName("avg_finalized_at").HasColumnType("timestamptz");
        builder.Property(g => g.BonusParticipationCount).HasColumnName("bonus_participation_count").HasDefaultValue(0);
        builder.Property(g => g.Rank).HasColumnName("rank");
        builder.Property(g => g.LastUpdatedAt).HasColumnName("last_updated_at").HasColumnType("timestamptz");

        builder.HasOne(g => g.User)
            .WithOne(u => u.GlobalLeaderboard)
            .HasForeignKey<GlobalLeaderboard>(g => g.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.Rank).HasDatabaseName("idx_gl_rank");
        builder.HasIndex(g => g.UserId).HasDatabaseName("uq_global_leaderboard_user").IsUnique();
    }
}
