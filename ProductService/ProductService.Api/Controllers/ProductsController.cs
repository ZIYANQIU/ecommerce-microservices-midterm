using ProductService.Api.Data;
using ProductService.Api.Dtos;
using ProductService.Api.Models;
using ProductService.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ISellerClient _sellerClient;

    public ProductsController(ProductDbContext context, ISellerClient sellerClient)
    {
        _context = context;
        _sellerClient = sellerClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
    {
        var products = await _context.Products
            .Select(product => new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                SellerId = product.SellerId
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        return Ok(ToResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var sellerExists = await _sellerClient.SellerExistsAsync(request.SellerId);
        if (!sellerExists)
            return BadRequest("Seller does not exist.");

        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            SellerId = request.SellerId
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToResponse(product));
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            SellerId = product.SellerId
        };
    }
}
