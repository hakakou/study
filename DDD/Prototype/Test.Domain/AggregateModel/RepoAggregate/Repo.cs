using System.Diagnostics.CodeAnalysis;
using Haka.Patterns.SeedWork;

namespace Test.Domain.AggregateModel.RepoAggregate;

public class Repo : EntityBase<Guid>, IAggregateRoot
{
    public required string Name { get; set; }

    private readonly List<RepoItem> _repoItems;
    public IReadOnlyCollection<RepoItem> RepoItems => _repoItems;

    [SetsRequiredMembers]
    public Repo(string name) : base()
    {
        _repoItems = [];
        Name = name;
    }
    
    [SetsRequiredMembers]
    public Repo(Guid id, string name) : base()
    {
        _repoItems = [];
        Id = id;
        Name = name;
    }

    public RepoItem AddRepoItem(string path)
    {
        var repoItem = new RepoItem(this, path);
        _repoItems.Add(repoItem);
        return repoItem;
    }
}
