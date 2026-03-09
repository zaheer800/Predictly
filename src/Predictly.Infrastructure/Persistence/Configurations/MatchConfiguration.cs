using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(m => m.TournamentId).HasColumnName("tournament_id");
        builder.Property(m => m.TeamHome).HasColumnName("team_home").HasMaxLength(100).IsRequired();
        builder.Property(m => m.TeamAway).HasColumnName("team_away").HasMaxLength(100).IsRequired();
        builder.Property(m => m.Venue).HasColumnName("venue").HasMaxLength(200);
        builder.Property(m => m.MatchStartTime).HasColumnName("match_start_time").HasColumnType("timestamptz");
        builder.Property(m => m.Winner).HasColumnName("winner").HasMaxLength(100);
        builder.Property(m => m.CreatedBy).HasColumnName("created_by");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                s => s.ToString().ToLower(),
                s => Enum.Parse<MatchStatus>(s, ignoreCase: true))
            .HasDefaultValue(MatchStatus.Scheduled);

        builder.HasOne(m => m.Tournament)
            .WithMany(t => t.Matches)
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Creator)
            .WithMany(u => u.CreatedMatches)
            .HasForeignKey(m => m.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.TournamentId).HasDatabaseName("idx_matches_tournament_id");
        builder.HasIndex(m => m.MatchStartTime).HasDatabaseName("idx_matches_match_start_time");
        builder.HasIndex(m => m.Status).HasDatabaseName("idx_matches_status");
    }
}
