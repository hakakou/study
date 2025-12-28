using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpecificationPattern.Database.Entities;

namespace SpecificationPattern.Database.Mapping;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired();

        builder.HasMany(x => x.Posts)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
