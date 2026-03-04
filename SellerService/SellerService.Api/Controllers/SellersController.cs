using Microsoft.AspNetCore.Mvc;
using SellerService.Api.Data;
using SellerService.Api.Models;

namespace SellerService.Api.Controllers;

[ApiController]
[Route("api/sellers")]
public class SellersController : ControllerBase
{
    private readonly SellerDbContext _context;

    public SellersController(SellerDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Seller>> GetById(int id)
    {
        var seller = await _context.Sellers.FindAsync(id);
        if (seller == null)
            return NotFound();

        return Ok(seller);
    }

    [HttpPost]
    public async Task<ActionResult<Seller>> Create(Seller seller)
    {
        await _context.Sellers.AddAsync(seller);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = seller.Id }, seller);
    }
}
