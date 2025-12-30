using Ardalis.Specification;
using Test.Domain.AggregateModel;

namespace Test.Domain.Specifications;

public class InactiveRepoSpecification : Specification<Repo>
{
    public InactiveRepoSpecification()
    {
        Query.Where(issue => !issue.Issues.Any());
    }
}
