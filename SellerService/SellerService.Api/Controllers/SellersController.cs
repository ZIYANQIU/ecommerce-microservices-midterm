using Microsoft.AspNetCore.Mvc;
using SellerService.Api.Data;
using SellerService.Api.Dtos;
using SellerService.Api.Models;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SellerResponse>>> GetAll()
    {
        var sellers = await _context.Sellers
            .Select(seller => new SellerResponse
            {
                Id = seller.Id,
                Name = seller.Name,
                Email = seller.Email
            })
            .ToListAsync();

        return Ok(sellers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SellerResponse>> GetById(int id)
    {
        var seller = await _context.Sellers.FindAsync(id);
        if (seller == null)
            return NotFound();

        return Ok(ToResponse(seller));
    }

    [HttpPost]
    public async Task<ActionResult<SellerResponse>> Create(CreateSellerRequest request)
    {
        var seller = new Seller
        {
            Name = request.Name,
            Email = request.Email
        };

        await _context.Sellers.AddAsync(seller);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = seller.Id }, ToResponse(seller));
    }

    private static SellerResponse ToResponse(Seller seller)
    {
        return new SellerResponse
        {
            Id = seller.Id,
            Name = seller.Name,
            Email = seller.Email
        };
    }
}
