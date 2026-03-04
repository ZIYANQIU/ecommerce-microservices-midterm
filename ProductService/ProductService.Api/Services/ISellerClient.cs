namespace ProductService.Api.Services;

public interface ISellerClient
{
    Task<bool> SellerExistsAsync(int sellerId);
}
