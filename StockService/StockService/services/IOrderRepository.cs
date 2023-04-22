using StockService.Models;

namespace StockService.services
{
    public interface IOrderRepository
    {
        Task<Orders> GetAsync(int orderId);
        Task AddAsync(Orders order);
        Task UpdateAsync(Orders order);
    }
}
