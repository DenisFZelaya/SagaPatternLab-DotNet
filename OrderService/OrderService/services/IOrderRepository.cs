using OrderService.Models;

namespace OrderService.services
{
    public interface IOrderRepository
    {
        Task<Orders> GetAsync(int orderId);
        Task UpdateAsync(Orders order);
        Task<int> CreateOrderAsync(int productId, int quantity);
    }
}
