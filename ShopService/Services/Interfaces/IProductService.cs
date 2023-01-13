using ShopServiceDA.Models;

namespace ShopServiceDA.Services.Interfaces;

using ShopServiceDA.Models;

public interface IProductService
{
    void SubscribeToGlobal();
    Task<List<Product>> GetProductsAsync();
    Task<Product> GetProductAsync(Guid id);
    Task<Product> UpdateProductAsync(Product updated);
    Task<Product> SaveProductAsync(Product product);
    Task DeleteProductAsync(Guid id);
}

