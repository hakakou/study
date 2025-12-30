using System;

namespace Company.Application.Contracts;

public record UpdateCustomerAddress
{
    public Guid CommandId { get; init; }
    public DateTime Timestamp { get; init; }
    public string CustomerId { get; init; }
    public string HouseNumber { get; init; }
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
}

public record CreateCustomer(string orderId)
{
    public Guid CommandId { get; init; }
    public string Name { get; init; } = orderId;
}

