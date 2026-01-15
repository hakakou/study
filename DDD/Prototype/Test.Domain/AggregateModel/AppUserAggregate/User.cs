using Ardalis.SmartEnum;
using Haka.Patterns.SeedWork;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Test.Domain.AggregateModel.AppUserAggregate;

public class User : EntityBase<Guid>, IAggregateRoot
{
    public User(Guid id, string userName) : base()
    {
        Id = id;
        UserName = userName;
        UserType = UserType.Free;
    }

    [MinLength(5)]
    [Required]
    public string UserName { get; private set; }
    
    public Address? Address { get; private set; }

    public UserType UserType { get; private set; }

    public void SetAddress(Address address)
    {
        Address = address;
    }

    public void SetUserType(UserType userType)
    {
        UserType = userType;
    }
}

public abstract class UserType : SmartEnum<UserType>
{
    public static readonly UserType Free = new FreeUserType();
    public static readonly UserType Paid = new PaidUserType();
    public static readonly UserType Admin = new AdminUserType();

    private UserType(string name, int value) : base(name, value) { }

    public virtual bool CanTransitionTo(UserType next) => false;
    public abstract int AllowedProjects { get; }


    public sealed class FreeUserType : UserType
    {
        public FreeUserType() : base(nameof(Free), 1) { }
        public override int AllowedProjects => 1;
        public override bool CanTransitionTo(UserType next) => next == Paid;

    }

    public sealed class PaidUserType : UserType
    {
        public PaidUserType() : base(nameof(Paid), 2) { }
        public override int AllowedProjects => 10;
        public override bool CanTransitionTo(UserType next) => next == Free;
    }

    public sealed class AdminUserType : UserType
    {
        public AdminUserType() : base(nameof(Admin), 3) { }
        public override int AllowedProjects => 10;
    }
}

public abstract class EmployeeType : SmartEnum<EmployeeType>
{
    public static readonly EmployeeType Hourly = new HourlyType();
    public static readonly EmployeeType Salaried = new SalariedType();
    public static readonly EmployeeType Contractor = new ContractorType();

    protected EmployeeType(string name, int value) : base(name, value) { }

    public abstract bool IsOvertimeEligible { get; }
}

public sealed class HourlyType : EmployeeType
{
    public HourlyType() : base(nameof(Hourly), 1) { }
    public override bool IsOvertimeEligible => true;
}

public sealed class SalariedType : EmployeeType
{
    public SalariedType() : base(nameof(Salaried), 2) { }
    public override bool IsOvertimeEligible => false;
}

public sealed class ContractorType : EmployeeType
{
    public ContractorType() : base(nameof(Contractor), 3) { }
    public override bool IsOvertimeEligible => false;
}