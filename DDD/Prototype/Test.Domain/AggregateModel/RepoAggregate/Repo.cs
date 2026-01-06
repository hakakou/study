using System.Collections.ObjectModel;
using Haka.Patterns.DDD;

namespace Test.Domain.AggregateModel;

public class Repo : EntityBase<Guid>, IAggregateRoot
{
    public required string Name { get; set; }

    private readonly List<RepoItem> _repoItems;
    public IReadOnlyCollection<RepoItem> RepoItems => _repoItems;

    public Repo(string name) : base()
    {
        _repoItems = [];
        Name = name;
    }

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

public class RepoItem : EntityBase<Guid>
{
    public Repo Repo { get; private set; }
    public Guid RepoId { get; private set; }

    public string Path { get; private set; }

    private RepoItem() : base() { }

    public RepoItem(Repo repo, string path) : base()
    {
        Repo = repo;
        Path = path;
    }
}
