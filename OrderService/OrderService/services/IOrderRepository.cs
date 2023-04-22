using OrderService.Models;

namespace OrderService.services
{
    public interface IOrderRepository
    {
        Task<Orders> GetAsync(int orderId);
        int AddAsync(Orders order);
        Task UpdateAsync(Orders order);
    }
}
