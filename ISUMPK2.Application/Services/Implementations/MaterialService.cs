using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services.Implementations
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly INotificationService _notificationService;
        private readonly IMaterialCategoryRepository _materialCategoryRepository;

        public MaterialService(
            IMaterialRepository materialRepository,
            INotificationService notificationService,
            IMaterialCategoryRepository materialCategoryRepository)
        {
            _materialRepository = materialRepository;
            _notificationService = notificationService;
            _materialCategoryRepository = materialCategoryRepository;
        }


        public async Task<MaterialDto> GetMaterialByIdAsync(Guid id)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            return material != null ? MapToDto(material) : null;
        }
        public async Task<MaterialTransactionDto> AddMaterialTransactionAsync(MaterialTransactionCreateDto transactionDto)
        {
            // Так как этот метод похож на AddTransactionAsync, мы можем использовать его,
            // передавая системный userId для транзакций, созданных системой
            var systemUserId = Guid.Parse("YOUR_SYSTEM_USER_ID"); // Замените на реальный системный ID
            return await AddTransactionAsync(systemUserId, transactionDto);
        }

        public async Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync()
        {
            var materials = await _materialRepository.GetAllAsync();
            return materials.Select(MapToDto);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsWithLowStockAsync()
        {
            var materials = await _materialRepository.GetMaterialsWithLowStockAsync();
            return materials.Select(MapToDto);
        }

        public async Task<MaterialDto> CreateMaterialAsync(MaterialCreateDto materialDto)
        {
            var material = new Material
            {
                Code = materialDto.Code,
                Name = materialDto.Name,
                Description = materialDto.Description,
                UnitOfMeasure = materialDto.UnitOfMeasure,
                CurrentStock = 0,
                MinimumStock = materialDto.MinimumStock,
                Price = materialDto.Price,
                CategoryId = materialDto.CategoryId,
                // Добавляем недостающие свойства
                Specifications = materialDto.Specifications,
                Manufacturer = materialDto.Manufacturer,
                PartNumber = materialDto.PartNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _materialRepository.AddAsync(material);
            await _materialRepository.SaveChangesAsync();

            return MapToDto(material);
        }


        public async Task<MaterialDto> UpdateMaterialAsync(Guid id, MaterialUpdateDto materialDto)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null)
                return null;

            material.Name = materialDto.Name;
            material.Description = materialDto.Description;
            material.UnitOfMeasure = materialDto.UnitOfMeasure;
            material.MinimumStock = materialDto.MinimumStock;
            material.Price = materialDto.Price;
            material.UpdatedAt = DateTime.UtcNow;

            await _materialRepository.UpdateAsync(material);
            await _materialRepository.SaveChangesAsync();

            return MapToDto(material);
        }

        public async Task DeleteMaterialAsync(Guid id)
        {
            await _materialRepository.DeleteAsync(id);
            await _materialRepository.SaveChangesAsync();
        }

        public async Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity)
        {
            return await _materialRepository.HasSufficientStockAsync(materialId, requiredQuantity);
        }

        public async Task<MaterialTransactionDto> AddTransactionAsync(Guid userId, MaterialTransactionCreateDto transactionDto)
        {
            // Проверка достаточного количества материала при выдаче
            if (transactionDto.TransactionType == "Issue" &&
                !await _materialRepository.HasSufficientStockAsync(transactionDto.MaterialId, transactionDto.Quantity))
            {
                throw new InvalidOperationException("Insufficient material stock");
            }

            // Обновление остатка материала
            bool isAddition = transactionDto.TransactionType == "Receipt";
            await _materialRepository.UpdateStockAsync(transactionDto.MaterialId, transactionDto.Quantity, isAddition);

            // Создание записи транзакции
            var transaction = new MaterialTransaction
            {
                MaterialId = transactionDto.MaterialId,
                Quantity = transactionDto.Quantity,
                TransactionType = transactionDto.TransactionType,
                TaskId = transactionDto.TaskId,
                UserId = userId,
                Notes = transactionDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Проверка, не ниже ли уровень запаса минимального
            var material = await _materialRepository.GetByIdAsync(transactionDto.MaterialId);
            if (material.CurrentStock <= material.MinimumStock)
            {
                await _notificationService.CreateLowStockNotificationAsync(material.Id);
            }

            return MapToDto(transaction);
        }

        public async Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByMaterialAsync(Guid materialId)
        {
            var transactions = await _materialRepository.GetTransactionsByMaterialAsync(materialId);
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _materialRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
            return transactions.Select(MapToDto);
        }

        private MaterialDto MapToDto(Material material)
        {
            return new MaterialDto
            {
                Id = material.Id,
                Code = material.Code,
                Name = material.Name,
                Description = material.Description,
                UnitOfMeasure = material.UnitOfMeasure,
                CurrentStock = material.CurrentStock,
                MinimumStock = material.MinimumStock,
                Price = material.Price,
                CategoryId = material.CategoryId,
                CategoryName = material.Category?.Name,
                // Добавляем отображение недостающих свойств
                Specifications = material.Specifications,
                Manufacturer = material.Manufacturer,
                PartNumber = material.PartNumber,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }


        private MaterialTransactionDto MapToDto(MaterialTransaction transaction)
        {
            return new MaterialTransactionDto
            {
                Id = transaction.Id,
                MaterialId = transaction.MaterialId,
                MaterialName = transaction.Material?.Name,
                Quantity = transaction.Quantity,
                TransactionType = transaction.TransactionType,
                TaskId = transaction.TaskId,
                TaskTitle = transaction.Task?.Title,
                UserId = transaction.UserId,
                UserName = transaction.User?.UserName,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }
        public async Task<MaterialCategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _materialCategoryRepository.GetByIdAsync(id);
            return category != null ? MapCategoryToDto(category) : null;
        }

        public async Task<IEnumerable<MaterialCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _materialCategoryRepository.GetAllAsync();
            return categories.Select(MapCategoryToDto);
        }

        public async Task<IEnumerable<MaterialCategoryDto>> GetTopLevelCategoriesAsync()
        {
            var categories = await _materialCategoryRepository.GetTopLevelCategoriesAsync();
            return categories.Select(MapCategoryToDto);
        }

        public async Task<IEnumerable<MaterialCategoryDto>> GetSubcategoriesAsync(Guid parentCategoryId)
        {
            var categories = await _materialCategoryRepository.GetSubcategoriesAsync(parentCategoryId);
            return categories.Select(MapCategoryToDto);
        }

        public async Task<MaterialCategoryDto> CreateCategoryAsync(MaterialCategoryDto categoryDto)
        {
            var category = new MaterialCategory
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ParentCategoryId = categoryDto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _materialCategoryRepository.AddAsync(category);
            await _materialCategoryRepository.SaveChangesAsync();

            return MapCategoryToDto(category);
        }

        public async Task<MaterialCategoryDto> UpdateCategoryAsync(Guid id, MaterialCategoryDto categoryDto)
        {
            var category = await _materialCategoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.ParentCategoryId = categoryDto.ParentCategoryId;
            category.UpdatedAt = DateTime.UtcNow;

            await _materialCategoryRepository.UpdateAsync(category);
            await _materialCategoryRepository.SaveChangesAsync();

            return MapCategoryToDto(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            await _materialCategoryRepository.DeleteAsync(id);
            await _materialCategoryRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByCategoryAsync(Guid categoryId, bool includeSubcategories = false)
        {
            var materials = await _materialCategoryRepository.GetMaterialsByCategoryAsync(categoryId, includeSubcategories);
            return materials.Select(MapToDto);
        }

        public async Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllMaterialsAsync();

            var materials = await _materialRepository.FindAsync(m =>
                m.Name.Contains(searchTerm) ||
                m.Code.Contains(searchTerm) ||
                m.Description.Contains(searchTerm) ||
                m.Specifications.Contains(searchTerm) ||
                m.Manufacturer.Contains(searchTerm) ||
                m.PartNumber.Contains(searchTerm));

            return materials.Select(MapToDto);
        }

        private MaterialCategoryDto MapCategoryToDto(MaterialCategory category)
        {
            return new MaterialCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name
            };
        }
    }
}
