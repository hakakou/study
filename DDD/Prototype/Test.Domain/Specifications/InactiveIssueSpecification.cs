using Ardalis.Specification;
using Test.Domain.AggregateModel.IssueAggregate;

namespace Test.Domain.Specifications;

public class InactiveIssueSpecification : Specification<Issue>
{
    public InactiveIssueSpecification()
    {
        Query.Where(issue => issue.AssignedUserId == null);
    }
}
