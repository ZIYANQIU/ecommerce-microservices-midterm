using System.Net.Http.Json;
using ApiGateway.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("gateway/customers")]
public class CustomersGatewayController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CustomersGatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return await ForwardGetAsync("/api/customers", cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        return await ForwardGetAsync($"/api/customers/{id}", cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerGatewayRequest request, CancellationToken cancellationToken)
    {
        return await ForwardPostAsync("/api/customers", request, cancellationToken);
    }

    private async Task<IActionResult> ForwardGetAsync(string path, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{GetServiceUrl()}{path}", cancellationToken);
        return await CreateProxyResponseAsync(response, cancellationToken);
    }

    private async Task<IActionResult> ForwardPostAsync(string path, object body, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{GetServiceUrl()}{path}", body, cancellationToken);
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
        return _configuration["Services:CustomerServiceBaseUrl"] ?? "http://customerservice:8080";
    }
}
