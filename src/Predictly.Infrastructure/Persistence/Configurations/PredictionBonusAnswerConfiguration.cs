using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class PredictionBonusAnswerConfiguration : IEntityTypeConfiguration<PredictionBonusAnswer>
{
    public void Configure(EntityTypeBuilder<PredictionBonusAnswer> builder)
    {
        builder.ToTable("prediction_bonus_answers");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(p => p.PredictionId).HasColumnName("prediction_id");
        builder.Property(p => p.MatchBonusSelectionId).HasColumnName("match_bonus_selection_id");
        builder.Property(p => p.AnswerNumeric).HasColumnName("answer_numeric").HasColumnType("numeric(12,2)");
        builder.Property(p => p.AnswerChoice).HasColumnName("answer_choice").HasMaxLength(100);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.HasOne(p => p.Prediction)
            .WithMany(pred => pred.BonusAnswers)
            .HasForeignKey(p => p.PredictionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MatchBonusSelection)
            .WithMany(m => m.PredictionAnswers)
            .HasForeignKey(p => p.MatchBonusSelectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PredictionId).HasDatabaseName("idx_pba_prediction_id");
        builder.HasIndex(new[] { "prediction_id", "match_bonus_selection_id" })
            .HasDatabaseName("uq_prediction_bonus_answer")
            .IsUnique();
    }
}
