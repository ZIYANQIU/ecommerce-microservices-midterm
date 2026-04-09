using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("gateway/order-details")]
public class OrderDetailsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OrderDetailsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(int id, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        var orderBaseUrl = GetServiceUrl("OrderServiceBaseUrl", "http://orderservice:8080");
        var productBaseUrl = GetServiceUrl("ProductServiceBaseUrl", "http://productservice:8080");
        var customerBaseUrl = GetServiceUrl("CustomerServiceBaseUrl", "http://customerservice:8080");

        try
        {
            var orderResponse = await client.GetAsync($"{orderBaseUrl}/api/orders/{id}", cancellationToken);
            if (orderResponse.StatusCode == HttpStatusCode.NotFound)
                return NotFound();

            if (!orderResponse.IsSuccessStatusCode)
                return StatusCode((int)orderResponse.StatusCode, "OrderService request failed.");

            var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);
            if (order == null)
                return StatusCode(StatusCodes.Status502BadGateway, "OrderService returned an invalid response.");

            var customer = await client.GetFromJsonAsync<CustomerDto>($"{customerBaseUrl}/api/customers/{order.CustomerId}", cancellationToken);
            var product = await client.GetFromJsonAsync<ProductDto>($"{productBaseUrl}/api/products/{order.ProductId}", cancellationToken);

            return Ok(new
            {
                Order = order,
                Customer = customer,
                Product = product
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "One of the backend services is unavailable.");
        }
    }

    private string GetServiceUrl(string key, string defaultValue)
    {
        return _configuration[$"Services:{key}"] ?? defaultValue;
    }

    private sealed class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private sealed class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int SellerId { get; set; }
    }
}
