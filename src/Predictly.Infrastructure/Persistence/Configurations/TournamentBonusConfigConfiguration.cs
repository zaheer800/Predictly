using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class TournamentBonusConfigConfiguration : IEntityTypeConfiguration<TournamentBonusConfig>
{
    public void Configure(EntityTypeBuilder<TournamentBonusConfig> builder)
    {
        builder.ToTable("tournament_bonus_config");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(t => t.TournamentId).HasColumnName("tournament_id");
        builder.Property(t => t.BonusQuestionId).HasColumnName("bonus_question_id");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");

        builder.HasOne(t => t.Tournament)
            .WithMany(tour => tour.BonusConfig)
            .HasForeignKey(t => t.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.BonusQuestion)
            .WithMany(b => b.TournamentConfigs)
            .HasForeignKey(t => t.BonusQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.TournamentId).HasDatabaseName("idx_tbc_tournament_id");
        builder.HasIndex(new[] { "tournament_id", "bonus_question_id" })
            .HasDatabaseName("uq_tournament_bonus")
            .IsUnique();
    }
}
