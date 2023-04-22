
using OrderService.Models;

namespace OrderService.services
{

    public interface IProductRepository
    {
        Task<Products> GetAsync(int productId);
        Task UpdateAsync(Products product);
    }
}
