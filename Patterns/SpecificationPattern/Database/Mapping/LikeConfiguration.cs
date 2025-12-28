using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpecificationPattern.Database.Entities;

namespace SpecificationPattern.Database.Mapping;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
