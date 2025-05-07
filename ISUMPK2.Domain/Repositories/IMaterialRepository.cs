using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Domain.Repositories
{
    public interface IMaterialRepository : IRepository<Material>
    {
        Task<IEnumerable<Material>> GetMaterialsWithLowStockAsync();
        Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity);
        Task UpdateStockAsync(Guid materialId, decimal quantity, bool isAddition);
        Task<IEnumerable<MaterialTransaction>> GetTransactionsByMaterialAsync(Guid materialId);
        Task<IEnumerable<MaterialTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
