using System.ComponentModel.DataAnnotations;

namespace Frontend.Models;

public class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Total { get; set; }
}
