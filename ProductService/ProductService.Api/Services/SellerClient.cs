using System.Net;

namespace ProductService.Api.Services;

public class SellerClient : ISellerClient
{
    private readonly HttpClient _httpClient;

    public SellerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SellerExistsAsync(int sellerId)
    {
        var response = await _httpClient.GetAsync($"api/sellers/{sellerId}");
        return response.StatusCode == HttpStatusCode.OK;
    }
}
