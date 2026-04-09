using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ProductService.Api.Data;

namespace ProductService.Api.Services;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly string _rabbitMqHost;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(
        IServiceProvider serviceProvider,
        ILogger<OrderCreatedConsumer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqHost = configuration["RabbitMQ:HostName"] ?? "localhost";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureConnectedAsync(stoppingToken);
        if (_channel == null)
            return;

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<OrderCreatedMessage>(json);
                if (message == null)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == message.ProductId, stoppingToken);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found for order {OrderId}", message.ProductId, message.OrderId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (product.Stock < message.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Quantity}, Available: {Stock}", product.Id, message.Quantity, product.Stock);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                product.Stock -= message.Quantity;
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Stock updated for product {ProductId}. Remaining: {Stock}", product.Id, product.Stock);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process OrderCreated event.");
                _channel?.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: "order-created", autoAck: false, consumer: consumer);
        _logger.LogInformation("OrderCreated consumer started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqHost,
            UserName = "guest",
            Password = "guest"
        };

        for (var attempt = 1; attempt <= 20 && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: "order-created", durable: true, exclusive: false, autoDelete: false);
                return;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        _logger.LogError("Could not connect to RabbitMQ for ProductService consumer.");
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private sealed class OrderCreatedMessage
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
