using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.AggregateModel.IssueAggregate;

namespace Test.Infrastructure.Data.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(i => i.Description)
            .HasMaxLength(2000);
        
        builder.Property(i => i.RepoId)
            .IsRequired();
        
        builder.Property(i => i.CreatedDate)
            .IsRequired();
        
        builder.Property(i => i.AssignedUserId);

        builder.Property(i => i.Name)
            .HasConversion(
                v => v.Value,
                v => new IssueName(v));

        builder.OwnsMany(i => i.Labels, lb =>
        {
            lb.WithOwner().HasForeignKey("IssueId");
            lb.HasKey(l => l.Id);
            lb.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100);
            lb.ToTable("IssueLabels");
        });

        builder.ToTable("Issues");
    }
}
