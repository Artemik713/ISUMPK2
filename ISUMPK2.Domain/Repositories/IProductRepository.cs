using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Domain.Repositories
{
    public interface IProductRepository: IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByTypeAsync(Guid productTypeId);
        Task<IEnumerable<ProductMaterial>> GetProductMaterialsAsync(Guid productId);
        Task<bool> HasSufficientMaterialsForProductionAsync(Guid productId, decimal quantity);
        Task UpdateStockAsync(Guid productId, decimal quantity, bool isAddition);
        Task<IEnumerable<ProductTransaction>> GetTransactionsByProductAsync(Guid productId);
        Task<IEnumerable<ProductTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
