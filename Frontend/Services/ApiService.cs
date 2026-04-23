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

    public Task<List<ProductDto>> GetProductsAsync()
    {
        return GetAsync<List<ProductDto>>("/gateway/products");
    }

    public Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        return PostAsync<CreateProductRequest, ProductDto>("/gateway/products", request);
    }

    public Task<List<CustomerDto>> GetCustomersAsync()
    {
        return GetAsync<List<CustomerDto>>("/gateway/customers");
    }

    public Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        return PostAsync<CreateCustomerRequest, CustomerDto>("/gateway/customers", request);
    }

    public Task<List<OrderDto>> GetOrdersAsync()
    {
        return GetAsync<List<OrderDto>>("/gateway/orders");
    }

    public Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        return PostAsync<CreateOrderRequest, OrderDto>("/gateway/orders", request);
    }

    public Task<List<SellerDto>> GetSellersAsync()
    {
        return GetAsync<List<SellerDto>>("/gateway/sellers");
    }

    public Task<SellerDto> CreateSellerAsync(CreateSellerRequest request)
    {
        return PostAsync<CreateSellerRequest, SellerDto>("/gateway/sellers", request);
    }

    private async Task<T> GetAsync<T>(string uri)
    {
        var response = await _http.GetAsync(uri);
        return await ReadResponseAsync<T>(response);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        var response = await _http.PostAsJsonAsync(uri, request);
        return await ReadResponseAsync<TResponse>(response);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<T>();
            if (result is not null)
            {
                return result;
            }

            throw new InvalidOperationException("The API returned an empty response.");
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException($"The API request failed with status code {(int)response.StatusCode}.");
        }

        throw new InvalidOperationException(error.Trim().Trim('"'));
    }
}
