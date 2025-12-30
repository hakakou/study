using Ardalis.Specification;
using Test.Domain.AggregateModel;

namespace Test.Domain.Specifications;

public class DuplicateNameSpecification(Guid repoId, IssueName name) : Specification<Issue>
{
    public override bool IsSatisfiedBy(Issue issue)
    {
        return issue.RepoId == repoId && issue.Name == name;
    }
}
