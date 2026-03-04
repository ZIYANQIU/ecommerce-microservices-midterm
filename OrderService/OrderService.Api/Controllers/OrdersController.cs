using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Orders.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        var customerExists = await _customerClient.CustomerExistsAsync(order.CustomerId);
        if (!customerExists)
            return BadRequest("Customer does not exist.");

        var productExists = await _productClient.ProductExistsAsync(order.ProductId);
        if (!productExists)
            return BadRequest("Product does not exist.");

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }
}
