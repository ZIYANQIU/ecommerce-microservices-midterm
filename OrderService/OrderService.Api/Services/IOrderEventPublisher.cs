using OrderService.Api.Models;

namespace OrderService.Api.Services;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(Order order);
    Task PublishOrderCancelledAsync(Order order);
}
