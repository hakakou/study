using Ardalis.GuardClauses;

namespace Test.Domain.AggregateModel;

public readonly record struct Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        Street = Guard.Against.NullOrWhiteSpace(street, nameof(street));
        City = Guard.Against.NullOrWhiteSpace(city, nameof(city));
        State = Guard.Against.NullOrWhiteSpace(state, nameof(state));
        PostalCode = Guard.Against.NullOrWhiteSpace(postalCode, nameof(postalCode));
        Country = Guard.Against.NullOrWhiteSpace(country, nameof(country));
    }
}
