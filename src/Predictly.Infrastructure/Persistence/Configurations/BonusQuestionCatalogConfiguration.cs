using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class BonusQuestionCatalogConfiguration : IEntityTypeConfiguration<BonusQuestionCatalog>
{
    public void Configure(EntityTypeBuilder<BonusQuestionCatalog> builder)
    {
        builder.ToTable("bonus_question_catalog");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(b => b.QuestionKey).HasColumnName("question_key").HasMaxLength(100).IsRequired();
        builder.Property(b => b.QuestionTemplate).HasColumnName("question_template").IsRequired();
        builder.Property(b => b.MaxPoints).HasColumnName("max_points");
        builder.Property(b => b.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");

        builder.Property(b => b.Options)
            .HasColumnName("options")
            .HasColumnType("jsonb");

        builder.Property(b => b.AnswerType)
            .HasColumnName("answer_type")
            .HasMaxLength(20)
            .HasConversion(
                a => a == AnswerType.Numeric ? "numeric" : "multiple_choice",
                a => a == "numeric" ? AnswerType.Numeric : AnswerType.MultipleChoice);

        builder.Property(b => b.ScoringRule)
            .HasColumnName("scoring_rule")
            .HasMaxLength(30)
            .HasConversion(
                r => r == ScoringRule.PercentageBased ? "percentage_based" : "exact_match",
                r => r == "percentage_based" ? ScoringRule.PercentageBased : ScoringRule.ExactMatch);

        builder.HasIndex(b => b.QuestionKey).HasDatabaseName("idx_bqc_question_key").IsUnique();
    }
}
