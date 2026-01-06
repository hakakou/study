using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.AggregateModel;
using Test.Domain.Specifications;

namespace Test.Infrastructure.Data.Configurations;

public class \UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.ComplexProperty(u => u.Address, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(200)
                .IsRequired();
            
            address.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired();
            
            address.Property(a => a.State)
                .HasMaxLength(100)
                .IsRequired(false);
            
            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .IsRequired();
            
            address.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired();
        });
        
        builder.HasMany<Issue>()
            .WithOne()
            .HasForeignKey(i => i.AssignedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("AppUsers");
    }
}
