using System.Collections.ObjectModel;
using Haka.Patterns.DDD;
using Test.Domain.Specifications;

namespace Test.Domain.AggregateModel;

public class Repo : EntityBase<Guid>, IAggregateRoot
{
    public required string Name { get; set; }

    public Repo(Guid id, string name) : base()
    {
        Id = id;
        Name = name;
    }

}
