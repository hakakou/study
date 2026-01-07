using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);

//builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();
//builder.Services.AddScoped<IValidator<Address>, AddressValidator>();

// Registered as Scoped by default.
builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

var host = builder.Build();

Customer customerNew = new Customer("ABCDE");

Customer customer = new Customer("ABCDE")
{
    Discount = 60,
    Address = new Address { Country = "U" },
    IsActive = false
};
CustomerValidator validator = new CustomerValidator();

// This will validate all except the "Required" ruleset
ValidationResult result = validator.Validate(customer);

// This will validate everything.
// result = validator.Validate(customer, o => o.IncludeAllRuleSets());

if (!result.IsValid)
{
    foreach (var failure in result.Errors)
        Console.WriteLine(failure.PropertyName + ": " + failure.ErrorMessage);
}

await host.RunAsync();

public class Customer
{
    public string Name { get; private set; }
    public decimal Discount { get; set; }
    public Address? Address { get; set; }
    public bool IsActive { get; set; }

    public Customer(string name)
    {
        Name = name;
        var validator = new CustomerValidator();
        validator.Validate(this, o =>
            // Validate only the "Required" ruleset and throw on failures
            o.ThrowOnFailures().IncludeRuleSets("Required"));
    }
}

public class Address
{
    public string Country { get; set; }
}

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleSet("Required", () =>
            RuleFor(c => c.Name)
                .NotNull().WithMessage("Surname is required")
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(50)
                .Must(n => !n.Any(char.IsDigit)).WithMessage("Surname cannot contain numbers")
        );

        RuleFor(c => c.Discount)
            .InclusiveBetween(0, 100);

        RuleFor(customer => customer.Address)
            .SetValidator(new AddressValidator());

        When(c => c.Discount > 50, () =>
        {
            RuleFor(c => c.IsActive).Must(c => c == true);
        });
    }

    // Custom validation method
    private bool BeAValidAddress(string address)
    {
        return true;
    }
}


public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(address => address.Country).NotNull()
            .Length(2);
    }
}