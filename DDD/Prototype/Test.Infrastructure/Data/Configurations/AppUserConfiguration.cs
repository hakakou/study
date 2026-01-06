using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.AggregateModel;
using Test.Domain.Specifications;

namespace Test.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.HasMany<Issue>()
            .WithOne()
            .HasForeignKey(i => i.AssignedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("AppUsers");
    }
}
