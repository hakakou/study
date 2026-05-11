using CoffeeChainApp;
using Microsoft.EntityFrameworkCore;

const string connectionString =
    "Host=localhost;Database=coffee_chain;Username=admin;Password=admin";

await using var db = new CoffeeChainDbContext(connectionString);

int customerId = 4;
int productId = 4;
int quantity = 2;

Console.WriteLine($"Calling sales.order_add_item({customerId}, {productId}, {quantity})...");
Console.WriteLine();

var items = await db.Database
    .SqlQuery<OrderItemResult>(
        $"SELECT * FROM sales.order_add_item({customerId}, {productId}, {quantity})")
    .ToListAsync();

if (items.Count == 0)
{
    Console.WriteLine("No items returned.");
}
else
{
    Console.WriteLine($"{"OrderId",-38} {"ProdId",-8} {"Qty",-6} {"Price",-10}");
    Console.WriteLine(new string('-', 68));
    foreach (var item in items)
        Console.WriteLine($"{item.order_id,-38} {item.prod_id,-8} {item.quantity,-6} {item.prod_price,-10:F2}");
}
