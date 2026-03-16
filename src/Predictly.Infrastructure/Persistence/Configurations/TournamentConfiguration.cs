using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("tournaments");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description");
        builder.Property(t => t.StartDate).HasColumnName("start_date").HasColumnType("timestamptz");
        builder.Property(t => t.EndDate).HasColumnName("end_date").HasColumnType("timestamptz");
        builder.Property(t => t.CreatedBy).HasColumnName("created_by");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                s => s.ToString().ToLower(),
                s => Enum.Parse<TournamentStatus>(s, ignoreCase: true))
            .HasDefaultValue(TournamentStatus.Draft);

        builder.HasOne(t => t.Creator)
            .WithMany(u => u.CreatedTournaments)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Status).HasDatabaseName("idx_tournaments_status");
    }
}
