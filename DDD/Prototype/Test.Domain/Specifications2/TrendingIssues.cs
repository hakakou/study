using Test.Domain.AggregateModel.IssueAggregate;
using Test.Domain.Specifications;
using Test;
using Test.Domain;

namespace Test.Domain.Specifications2;

public class TrendingIssues : Haka.Patterns.Specifications.BaseSpecification<Issue>
{
    public TrendingIssues()
    {
        AddFilteringQuery(i => i.AssignedUserId != null);
        AddOrderByDescendingQuery(i => i.Name);
        AddIncludeQuery(i => i.Labels);
    }
}
