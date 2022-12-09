using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Core.Entities;

namespace Ecommerce.Application.Stores;

public class StoreService : IStoreService
{
    private readonly IEfRepository<ProductStore> _productStoreRepo;

    public StoreService(
        IEfRepository<ProductStore> productStoreRepo)
    {
        _productStoreRepo = productStoreRepo;
    }

    public async Task<bool> AddProductAsync(int productId, int storeId)
    {
        var storeProduct = _productStoreRepo.GetFirst(ps => ps.StoreId == storeId && ps.ProductId == productId);

        if (storeProduct is not null)
        {
            return false;
        }

        storeProduct = new ProductStore()
        {
            StoreId = storeId,
            ProductId = productId,
            Quantity = 1
        };

        var storeProductCreated = await _productStoreRepo.AddAsync(storeProduct);

        if (storeProductCreated is null) return false;

        return true;
    }

    public async Task<bool> DecreaseProductAsync(int productId, int storeId)
    {
        var storeProduct = _productStoreRepo.GetFirst(ps => ps.StoreId == storeId && ps.ProductId == productId);

        if (storeProduct is null) return false;

        if (storeProduct.Quantity <= 1)
        {
            _productStoreRepo.Remove(storeProduct);
            if (await _productStoreRepo.SaveChangeAsync() < 1) return false;
            return true;
        }

        storeProduct.DecreaseQuantity();

        var operationResult = await _productStoreRepo.SaveChangeAsync();

        if (operationResult < 1) return false;

        return true;
    }

    public void DeleteProductStoreRelation(int storeId)
    {
        IEnumerable<ProductStore> productStore = _productStoreRepo.GetAll(ps => ps.StoreId == storeId);

        if (productStore is null) throw new InvalidOperationException("Store doesn't exist.");

        _productStoreRepo.RemoveRange(productStore);
    }

    public async Task<bool> IncreaseProductAsync(int productId, int storeId)
    {
        var storeProduct = _productStoreRepo.GetFirst(ps => ps.StoreId == storeId && ps.ProductId == productId);

        if (storeProduct is null)
            return false;

        storeProduct.IncreaseQuantity();

        var operationResult = await _productStoreRepo.SaveChangeAsync();

        if (operationResult < 1) return false;

        return true;
    }
}
