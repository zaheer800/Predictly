using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class MatchBonusSelectionConfiguration : IEntityTypeConfiguration<MatchBonusSelection>
{
    public void Configure(EntityTypeBuilder<MatchBonusSelection> builder)
    {
        builder.ToTable("match_bonus_selection");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(m => m.MatchId).HasColumnName("match_id");
        builder.Property(m => m.BonusQuestionId).HasColumnName("bonus_question_id");
        builder.Property(m => m.DisplayOrder).HasColumnName("display_order");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");

        builder.HasOne(m => m.Match)
            .WithMany(match => match.BonusSelections)
            .HasForeignKey(m => m.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.BonusQuestion)
            .WithMany(b => b.MatchSelections)
            .HasForeignKey(m => m.BonusQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.MatchId).HasDatabaseName("idx_mbs_match_id");
        builder.HasIndex(new[] { "match_id", "bonus_question_id" })
            .HasDatabaseName("uq_match_bonus")
            .IsUnique();
        builder.HasIndex(new[] { "match_id", "display_order" })
            .HasDatabaseName("uq_match_display_order")
            .IsUnique();
    }
}
