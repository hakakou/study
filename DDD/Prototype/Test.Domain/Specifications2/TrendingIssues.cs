using Test.Domain.AggregateModel.IssueAggregate;

namespace Test.Domain.Specifications;

public class TrendingIssues : Haka.Patterns.Specifications.BaseSpecification<Issue>
{
    public TrendingIssues()
    {
        AddFilteringQuery(i => i.AssignedUserId != null);
        AddOrderByDescendingQuery(i => i.Name);
        AddIncludeQuery(i => i.Labels);
        AddInclude("AssignedUser");
    }
}
