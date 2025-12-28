using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpecificationPattern.Database.Entities;

namespace SpecificationPattern.Database.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired();

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Posts)
	        .WithOne(x => x.User)
	        .HasForeignKey(x => x.UserId)
	        .OnDelete(DeleteBehavior.Cascade);
    }
}
