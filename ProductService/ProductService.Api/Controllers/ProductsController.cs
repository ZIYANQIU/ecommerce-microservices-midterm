using ProductService.Api.Data;
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
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        return Ok(await _context.Products.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        var sellerExists = await _sellerClient.SellerExistsAsync(product.SellerId);
        if (!sellerExists)
            return BadRequest("Seller does not exist.");

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
