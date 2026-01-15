using Haka.Patterns.SeedWork;

namespace Test.Domain.AggregateModel.RepoAggregate;

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
