using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpecificationPattern.Database.Entities;

namespace SpecificationPattern.Database.Mapping;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Content).IsRequired();

        builder.HasMany(x => x.Likes)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
	        .WithMany(x => x.Posts)
	        .HasForeignKey(x => x.UserId)
	        .OnDelete(DeleteBehavior.Cascade);
    }
}
