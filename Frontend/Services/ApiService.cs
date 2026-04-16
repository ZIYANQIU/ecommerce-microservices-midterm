using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        return (await _http.GetFromJsonAsync<List<ProductDto>>("/products"))!;
    }
}
