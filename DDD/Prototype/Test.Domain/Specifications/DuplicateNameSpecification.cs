using Ardalis.Specification;
using Test.Domain.AggregateModel.IssueAggregate;

namespace Test.Domain.Specifications;

public class DuplicateNameSpecification : Specification<Issue>
{
    public DuplicateNameSpecification(Guid repoId, IssueName name)
    {
        Query.Where(i => i.RepoId == repoId && i.Name == name);
    }
}
