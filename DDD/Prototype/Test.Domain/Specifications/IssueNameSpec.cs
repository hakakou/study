using Ardalis.Specification;
using System.Linq;
using Test.Domain.AggregateModel;

namespace Test.Domain.Specifications;

public class IssueNameSpec : Specification<Issue, string>
{
    public IssueNameSpec(int id)
    {
        Query
            .Where(x => x.Id == id)
            .OrderBy(x => x.Name)
            .Select(x => x.Name.Value);
    }
}
