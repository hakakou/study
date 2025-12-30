using System.Collections.ObjectModel;
using Haka.Patterns.DDD;
using Test.Domain.Specifications;

namespace Test.Domain.AggregateModel;

public class Repo : EntityBase<Guid>, IAggregateRoot
{
    public required string Name { get; set; }

    public ICollection<Issue> Issues { get; private set; }

    public void AddIssue(Issue issue)
    {
        Issues.Add(issue);
    }

    public Repo(Guid id, string name) : base()
    {
        Id = id;
        Name = name;
        Issues = new Collection<Issue>();
    }

    public bool IsInInactive()
    {
        return new InactiveRepoSpecification().IsSatisfiedBy(this);
    }
}
