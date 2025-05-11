using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public interface IMaterialService
    {
        Task<MaterialDto> GetMaterialByIdAsync(Guid id);
        Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync();
        Task<IEnumerable<MaterialDto>> GetMaterialsWithLowStockAsync();
        Task<MaterialDto> CreateMaterialAsync(MaterialCreateDto materialDto);
        Task<MaterialDto> UpdateMaterialAsync(Guid id, MaterialUpdateDto materialDto);
        Task DeleteMaterialAsync(Guid id);
        Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity);
        Task<MaterialTransactionDto> AddTransactionAsync(Guid userId, MaterialTransactionCreateDto transactionDto);
        Task<MaterialTransactionDto> AddMaterialTransactionAsync(MaterialTransactionCreateDto transactionDto);
        Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByMaterialAsync(Guid materialId);
        Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Методы для работы с категориями материалов
        Task<MaterialCategoryDto> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<MaterialCategoryDto>> GetAllCategoriesAsync();
        Task<IEnumerable<MaterialCategoryDto>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<MaterialCategoryDto>> GetSubcategoriesAsync(Guid parentCategoryId);
        Task<IEnumerable<MaterialDto>> GetMaterialsByCategoryAsync(Guid categoryId, bool includeSubcategories = false);
        Task<MaterialCategoryDto> CreateCategoryAsync(MaterialCategoryDto categoryDto);
        Task<MaterialCategoryDto> UpdateCategoryAsync(Guid id, MaterialCategoryDto categoryDto);
        Task DeleteCategoryAsync(Guid id);
        Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm);
    }
}
