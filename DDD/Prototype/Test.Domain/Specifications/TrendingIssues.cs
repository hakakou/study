using Test.Domain.AggregateModel;

namespace Test.Domain.Specifications;

public class TrendingIssues : Haka.Patterns.Specifications.Specification<Issue>
{
    public TrendingIssues()
    {
        AddFilteringQuery(i => i.AssignedUserId != null);
        AddOrderByDescendingQuery(i => i.Labels);
        AddIncludeQuery(i => i.Labels);
    }
}
