using Microsoft.EntityFrameworkCore;
using SellerService.Api.Models;

namespace SellerService.Api.Data;

public class SellerDbContext : DbContext
{
    public SellerDbContext(DbContextOptions<SellerDbContext> options) : base(options)
    {
    }

    public DbSet<Seller> Sellers => Set<Seller>();
}
