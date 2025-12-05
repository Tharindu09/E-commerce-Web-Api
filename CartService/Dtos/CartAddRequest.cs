namespace CartService.Dtos;

public class CartAddRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
