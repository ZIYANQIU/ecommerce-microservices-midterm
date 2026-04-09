using System.Net.Http.Json;
using ApiGateway.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("gateway/orders")]
public class OrdersGatewayController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OrdersGatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return await ForwardGetAsync("/api/orders", cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        return await ForwardGetAsync($"/api/orders/{id}", cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderGatewayRequest request, CancellationToken cancellationToken)
    {
        return await ForwardPostAsync("/api/orders", request, cancellationToken);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        return await ForwardPostAsync($"/api/orders/{id}/cancel", null, cancellationToken);
    }

    private async Task<IActionResult> ForwardGetAsync(string path, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{GetServiceUrl()}{path}", cancellationToken);
        return await CreateProxyResponseAsync(response, cancellationToken);
    }

    private async Task<IActionResult> ForwardPostAsync(string path, object? body, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        HttpResponseMessage response;

        if (body is null)
        {
            response = await client.PostAsync($"{GetServiceUrl()}{path}", content: null, cancellationToken);
        }
        else
        {
            response = await client.PostAsJsonAsync($"{GetServiceUrl()}{path}", body, cancellationToken);
        }

        return await CreateProxyResponseAsync(response, cancellationToken);
    }

    private async Task<IActionResult> CreateProxyResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = body,
            ContentType = contentType
        };
    }

    private string GetServiceUrl()
    {
        return _configuration["Services:OrderServiceBaseUrl"] ?? "http://orderservice:8080";
    }
}
