using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.AggregateModel.IssueAggregate;
using Test.Domain.AggregateModel.RepoAggregate;

namespace Test.Infrastructure.Data.Configurations;

public class RepoConfiguration : IEntityTypeConfiguration<Repo>
{
    public void Configure(EntityTypeBuilder<Repo> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany<Issue>()
            .WithOne()
            .HasForeignKey(i => i.RepoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Repos");
    }
}
