using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Application.Services
{
    public interface IMaterialService
    {
        Task<MaterialTransactionDto> AddMaterialTransactionAsync(MaterialTransactionCreateDto transactionDto);
        Task<MaterialDto> GetMaterialByIdAsync(Guid id);
        Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync();
        Task<IEnumerable<MaterialDto>> GetMaterialsWithLowStockAsync();
        Task<MaterialDto> CreateMaterialAsync(MaterialCreateDto materialDto);
        Task<MaterialDto> UpdateMaterialAsync(Guid id, MaterialUpdateDto materialDto);
        Task DeleteMaterialAsync(Guid id);
        Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity);
        Task<MaterialTransactionDto> AddTransactionAsync(Guid userId, MaterialTransactionCreateDto transactionDto);
        Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByMaterialAsync(Guid materialId);
        Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
