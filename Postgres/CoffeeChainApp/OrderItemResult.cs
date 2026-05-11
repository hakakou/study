namespace CoffeeChainApp;

public class OrderItemResult
{
    public Guid order_id { get; set; }
    public int prod_id { get; set; }
    public int quantity { get; set; }
    public decimal prod_price { get; set; }
}
