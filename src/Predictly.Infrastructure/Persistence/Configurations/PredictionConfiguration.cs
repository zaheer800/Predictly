using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.ToTable("predictions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.MatchId).HasColumnName("match_id");
        builder.Property(p => p.PredictedWinner).HasColumnName("predicted_winner").HasMaxLength(100).IsRequired();
        builder.Property(p => p.FinalizedAt).HasColumnName("finalized_at").HasColumnType("timestamptz");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");

        builder.HasOne(p => p.User)
            .WithMany(u => u.Predictions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Match)
            .WithMany(m => m.Predictions)
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId).HasDatabaseName("idx_predictions_user_id");
        builder.HasIndex(p => p.MatchId).HasDatabaseName("idx_predictions_match_id");
        builder.HasIndex(new[] { "user_id", "match_id" })
            .HasDatabaseName("uq_user_match_prediction")
            .IsUnique();
    }
}
