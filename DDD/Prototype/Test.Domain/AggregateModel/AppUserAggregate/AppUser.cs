using Haka.Patterns.DDD;

namespace Test.Domain.AggregateModel;

public class AppUser : EntityBase<Guid>, IAggregateRoot
{
    public AppUser(Guid id, string userName) : base()
    {
        Id = id;
        UserName = userName;
    }

    public string UserName { get; private set; }
}
