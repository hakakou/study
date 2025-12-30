using Ardalis.Specification;

namespace Test.Domain.Specifications;

public class FirstOrDefaultSpecification<T> : SingleResultSpecification<T>
{
    public FirstOrDefaultSpecification()
    {
        Query.Take(1);
    }
}
