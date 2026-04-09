using System.Text;
using System.Text.Json;
using OrderService.Api.Models;
using RabbitMQ.Client;

namespace OrderService.Api.Services;

public class RabbitMQPublisher : IOrderEventPublisher
{
    private readonly string _hostname;

    public RabbitMQPublisher(string hostname)
    {
        _hostname = hostname;
    }

    public Task PublishOrderCreatedAsync(Order order)
    {
        var evt = new OrderCreatedMessage
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            Total = order.Total
        };

        var json = JsonSerializer.Serialize(evt);
        Publish("order-created", json);
        return Task.CompletedTask;
    }

    public Task PublishOrderCancelledAsync(Order order)
    {
        var evt = new OrderCancelledMessage
        {
            OrderId = order.Id,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        };

        var json = JsonSerializer.Serialize(evt);
        Publish("order-cancelled", json);
        return Task.CompletedTask;
    }

    private void Publish(string queueName, string message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            UserName = "guest",
            Password = "guest"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body
        );
    }

    private sealed class OrderCreatedMessage
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    private sealed class OrderCancelledMessage
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
