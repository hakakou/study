namespace MyProject.Components.Consumers;

using Company.Application.Contracts;
using MassTransit;
using MyProject.Contracts;

public class CreateCustomerConsumer :
    IConsumer<CreateCustomer>
{
    public async Task Consume(ConsumeContext<CreateCustomer> context)
    {
        // Handle the message
    }
 
}