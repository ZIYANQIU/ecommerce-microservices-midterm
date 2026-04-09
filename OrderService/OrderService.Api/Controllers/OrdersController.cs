using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Dtos;
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
    private readonly IOrderEventPublisher _orderEventPublisher;

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient,
        IOrderEventPublisher orderEventPublisher)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;
        _orderEventPublisher = orderEventPublisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll()
    {
        var orders = await _context.Orders
            .Select(order => new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Total = order.Total,
                Status = order.Status
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        return Ok(ToResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var customerExists = await _customerClient.CustomerExistsAsync(request.CustomerId);
        if (!customerExists)
            return BadRequest("Customer does not exist.");

        var productExists = await _productClient.ProductExistsAsync(request.ProductId);
        if (!productExists)
            return BadRequest("Product does not exist.");

        var order = new Order
        {
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Total = request.Total,
            Status = "Created"
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        await _orderEventPublisher.PublishOrderCreatedAsync(order);

        return Ok(ToResponse(order));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        if (order.Status == "Cancelled")
            return BadRequest("Order is already cancelled.");

        order.Status = "Cancelled";
        await _context.SaveChangesAsync();

        await _orderEventPublisher.PublishOrderCancelledAsync(order);

        return Ok(ToResponse(order));
    }

    private static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            Total = order.Total,
            Status = order.Status
        };
    }
}
