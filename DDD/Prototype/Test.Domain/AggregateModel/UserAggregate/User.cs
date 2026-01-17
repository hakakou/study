using Ardalis.SmartEnum;
using Haka.Patterns.SeedWork;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Test.Domain.AggregateModel.UserAggregate;
using Test;
using Test.Domain;
using Test.Domain.AggregateModel;

namespace Test.Domain.AggregateModel.UserAggregate;

public class User : EntityBase<Guid>, IAggregateRoot
{
    public User(Guid id, string userName) : base()
    {
        Id = id;
        UserName = userName;
        UserType = UserAggregate.UserType.Free;
    }

    [MinLength(5)]
    [Required]
    public string UserName { get; private set; }
    
    public UserAggregate.Address? Address { get; private set; }

    public UserAggregate.UserType UserType { get; private set; }

    public void SetAddress(UserAggregate.Address address)
    {
        Address = address;
    }

    public void SetUserType(UserAggregate.UserType userType)
    {
        UserType = userType;
    }
}

public abstract class UserType : SmartEnum<UserAggregate.UserType>
{
    public static readonly UserAggregate.UserType Free = new UserAggregate.UserType.FreeUserType();
    public static readonly UserAggregate.UserType Paid = new UserAggregate.UserType.PaidUserType();
    public static readonly UserAggregate.UserType Admin = new UserAggregate.UserType.AdminUserType();

    private UserType(string name, int value) : base(name, value) { }

    public virtual bool CanTransitionTo(UserAggregate.UserType next) => false;
    public abstract int AllowedProjects { get; }


    public sealed class FreeUserType : UserAggregate.UserType
    {
        public FreeUserType() : base(nameof(Free), 1) { }
        public override int AllowedProjects => 1;
        public override bool CanTransitionTo(UserAggregate.UserType next) => next == Paid;

    }

    public sealed class PaidUserType : UserAggregate.UserType
    {
        public PaidUserType() : base(nameof(Paid), 2) { }
        public override int AllowedProjects => 10;
        public override bool CanTransitionTo(UserAggregate.UserType next) => next == Free;
    }

    public sealed class AdminUserType : UserAggregate.UserType
    {
        public AdminUserType() : base(nameof(Admin), 3) { }
        public override int AllowedProjects => 10;
    }
}

public abstract class EmployeeType : SmartEnum<UserAggregate.EmployeeType>
{
    public static readonly UserAggregate.EmployeeType Hourly = new UserAggregate.HourlyType();
    public static readonly UserAggregate.EmployeeType Salaried = new UserAggregate.SalariedType();
    public static readonly UserAggregate.EmployeeType Contractor = new UserAggregate.ContractorType();

    protected EmployeeType(string name, int value) : base(name, value) { }

    public abstract bool IsOvertimeEligible { get; }
}

public sealed class HourlyType : UserAggregate.EmployeeType
{
    public HourlyType() : base(nameof(Hourly), 1) { }
    public override bool IsOvertimeEligible => true;
}

public sealed class SalariedType : UserAggregate.EmployeeType
{
    public SalariedType() : base(nameof(Salaried), 2) { }
    public override bool IsOvertimeEligible => false;
}

public sealed class ContractorType : UserAggregate.EmployeeType
{
    public ContractorType() : base(nameof(Contractor), 3) { }
    public override bool IsOvertimeEligible => false;
}