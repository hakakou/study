using Ardalis.Specification;
using Test.Domain.AggregateModel;

namespace Test.Domain.Specifications;

public static class IssuePredicates
{
    public static ISpecificationBuilder<Issue> IsOld(this ISpecificationBuilder<Issue> builder)
    {
        var dateThreshold = DateTime.UtcNow.AddDays(-30);
        return builder.Where(x => x.CreatedDate < dateThreshold);
    }
}
