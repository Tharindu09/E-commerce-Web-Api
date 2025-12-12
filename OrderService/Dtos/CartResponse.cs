namespace OrderService.Dtos;

public class CartResponse
{
    public int UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }

    public int Quantity { get; set; }    
}
