namespace eShop.Ordering.Domain.Exceptions;

/// <summary>
/// Exception type for domain exceptions
/// </summary>
public class OrderingDomainException : Exception
{
    public OrderingDomainException()
    { }

    public OrderingDomainException(string message)
        : base(message)
    { }

    public OrderingDomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}

public class BusinessException : Exception
{
    public BusinessException()
    { }
    public BusinessException(string message)
        : base(message)
    { }
    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    { }
}   
