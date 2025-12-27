using Clean.Core.ContributorAggregate;
using Test.Domain.AggregateModel;

namespace Clean.Infrastructure.Data.Config;

public class ContributorConfiguration : IEntityTypeConfiguration<Contributor>
{
  public void Configure(EntityTypeBuilder<Contributor> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, Contributor, ContributorId>>()
      //.HasVogenConversion()
      //.HasConversion(new VogenEfCoreConverters.ContributorIdEfCoreValueConverter())
      .IsRequired();

    builder.Property(entity => entity.Name)
      //.HasVogenConversion()
      .HasMaxLength(ContributorName.MaxLength)
      .IsRequired();

    builder.OwnsOne(builder => builder.PhoneNumber);

    builder.Property(x => x.Status)
      .HasConversion(
          x => x.Value,
          x => ContributorStatus.FromValue(x));
  }
}


public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
  public void Configure(EntityTypeBuilder<Issue> builder)
  {
    //builder.Property(entity => entity.Id)
    //        .HasValueGenerator<VogenIdValueGenerator<AppDbContext, Issue, IssueId>>()
      //.HasVogenConversion()
//      .IsRequired();

    builder.Property(entity => entity.Name)
      //.HasVogenConversion()
      .HasMaxLength(IssueName.MaxLength)
      .IsRequired();

    //builder.Property(entity => entity.Description)
    //  .HasVogenConversion()
    //  .HasMaxLength(IssueDescription.MaxLength)
    //  .IsRequired();

    builder.Property(entity => entity.CreatedDate)
      .IsRequired();
  }
}

