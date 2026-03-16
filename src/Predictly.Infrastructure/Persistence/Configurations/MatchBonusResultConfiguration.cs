using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class MatchBonusResultConfiguration : IEntityTypeConfiguration<MatchBonusResult>
{
    public void Configure(EntityTypeBuilder<MatchBonusResult> builder)
    {
        builder.ToTable("match_bonus_results");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(m => m.MatchId).HasColumnName("match_id");
        builder.Property(m => m.MatchBonusSelectionId).HasColumnName("match_bonus_selection_id");
        builder.Property(m => m.ActualNumeric).HasColumnName("actual_numeric").HasColumnType("numeric(12,2)");
        builder.Property(m => m.ActualChoice).HasColumnName("actual_choice").HasMaxLength(100);
        builder.Property(m => m.SubmittedBy).HasColumnName("submitted_by");
        builder.Property(m => m.SubmittedAt).HasColumnName("submitted_at").HasColumnType("timestamptz");

        builder.HasOne(m => m.Match)
            .WithMany(match => match.BonusResults)
            .HasForeignKey(m => m.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.MatchBonusSelection)
            .WithOne(mbs => mbs.BonusResult)
            .HasForeignKey<MatchBonusResult>(m => m.MatchBonusSelectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.SubmittedByUser)
            .WithMany()
            .HasForeignKey(m => m.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.MatchId).HasDatabaseName("idx_mbr_match_id");
        builder.HasIndex(new[] { "match_id", "match_bonus_selection_id" })
            .HasDatabaseName("uq_match_bonus_result")
            .IsUnique();
    }
}
