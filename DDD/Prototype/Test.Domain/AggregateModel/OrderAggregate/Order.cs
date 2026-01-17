using Haka.Patterns.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace Test.Domain.AggregateModel.OrderAggregate;

public class Order : EntityBase<long>, IAggregateRoot
{
    public Order(long id, string orderNumber, DateTime orderDate) : base()
    {
        Id = id;
        OrderNumber = orderNumber;
        OrderDate = orderDate;
        Status = OrderStatus.Pending;
    }

    [Required]
    public string OrderNumber { get; private set; }

    public DateTime OrderDate { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    public void SetStatus(OrderStatus status)
    {
        Status = status;
    }

    public void SetTotalAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(amount));
        
        TotalAmount = amount;
    }
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
