using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.AggregateModel;

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
        
        builder.Property(i => i.GitRepositoryId)
            .IsRequired();
        
        builder.Property(i => i.CreatedDate)
            .IsRequired();
        
        builder.Property(i => i.AssignedUserId);
        
        builder.ToTable("Issues");
    }
}
