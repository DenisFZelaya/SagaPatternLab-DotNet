using StockService.Models;

namespace StockService.services
{
    public interface IProductRepository
    {
        Task<Products> GetAsync(int productId);
        Task UpdateAsync(Products product);
    }
}
