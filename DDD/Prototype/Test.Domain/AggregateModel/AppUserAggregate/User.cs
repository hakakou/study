using Haka.Patterns.DDD;
using System.Net;

namespace Test.Domain.AggregateModel;

public class User : EntityBase<Guid>, IAggregateRoot
{
    public User(Guid id, string userName) : base()
    {
        Id = id;
        UserName = userName;
    }

    public string UserName { get; private set; }
    public Address? Address { get; private set; }

    public void SetAddress(Address address)
    {
        Address = address;
    }
}
