using Test.Domain.AggregateModel;

namespace Test.Domain.Specifications;

public class ReposWithManyIssues : Haka.Patterns.Specifications.Specification<Repo>
{
    public ReposWithManyIssues(int min = 1)
    {
        AddFilteringQuery(repo => repo.Issues.Count >= min);
        AddOrderByDescendingQuery(repo => repo.Issues.Count);
        AddIncludeQuery(repo => repo.Issues);
    }
}
