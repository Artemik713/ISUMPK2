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
        private readonly IUserService _userService;
        private readonly ITaskMaterialRepository _taskMaterialRepository; // Добавляем
        private readonly ITaskRepository _taskRepository; // Добавляем

        public MaterialService(
            IMaterialRepository materialRepository,
            INotificationService notificationService,
            IMaterialCategoryRepository materialCategoryRepository,
            IUserService userService,
            ITaskMaterialRepository taskMaterialRepository, // Добавляем
            ITaskRepository taskRepository) // Добавляем
        {
            _materialRepository = materialRepository;
            _notificationService = notificationService;
            _materialCategoryRepository = materialCategoryRepository;
            _userService = userService;
            _taskMaterialRepository = taskMaterialRepository; // Добавляем
            _taskRepository = taskRepository; // Добавляем
        }

        public async Task<MaterialDto> GetMaterialByIdAsync(Guid id)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            return material != null ? MapToDto(material) : null;
        }

        public async Task<MaterialTransactionDto> AddMaterialTransactionAsync(MaterialTransactionCreateDto transactionDto)
        {
            if (transactionDto == null)
                throw new ArgumentNullException(nameof(transactionDto));

            if (transactionDto.MaterialId == Guid.Empty)
                throw new ArgumentException("MaterialId is required", nameof(transactionDto));

            if (string.IsNullOrWhiteSpace(transactionDto.TransactionType))
                throw new ArgumentException("TransactionType is required", nameof(transactionDto));

            if (transactionDto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(transactionDto));

            // Получаем текущего пользователя
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new InvalidOperationException("Невозможно получить информацию о текущем пользователе");

            return await AddTransactionAsync(currentUser.Id, transactionDto);
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

            material.Code = materialDto.Code;
            material.Name = materialDto.Name;
            material.Description = materialDto.Description;
            material.UnitOfMeasure = materialDto.UnitOfMeasure;
            material.MinimumStock = materialDto.MinimumStock;
            material.CurrentStock = materialDto.CurrentStock;
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
            // Если UserId не передан или используется дефолтный, замените его на системного пользователя
            if (userId == Guid.Empty)
            {
                userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            }

            // Проверка и установка значений по умолчанию
            if (string.IsNullOrEmpty(transactionDto.Notes))
            {
                transactionDto.Notes = "-";
            }

            // Проверка достаточного количества материала при выдаче
            if (transactionDto.TransactionType == "Issue" &&
                !await _materialRepository.HasSufficientStockAsync(transactionDto.MaterialId, transactionDto.Quantity))
            {
                throw new InvalidOperationException("Недостаточное количество материала на складе");
            }

            // Создание записи транзакции
            var transaction = new MaterialTransaction
            {
                Id = Guid.NewGuid(),
                MaterialId = transactionDto.MaterialId,
                Quantity = transactionDto.Quantity,
                TransactionType = transactionDto.TransactionType,
                TaskId = transactionDto.TaskId,
                UserId = userId,
                Notes = transactionDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                // Обновление остатка материала
                bool isAddition = transactionDto.TransactionType == "Receipt";
                await _materialRepository.UpdateStockAsync(transactionDto.MaterialId, transactionDto.Quantity, isAddition);

                // Добавление записи транзакции
                await _materialRepository.AddTransactionAsync(transaction);

                // *** НОВАЯ ЛОГИКА: Обновление материалов задачи ***
                if (transactionDto.TaskId.HasValue && transactionDto.TransactionType == "Issue")
                {
                    await UpdateTaskMaterialQuantityAsync(transactionDto.TaskId.Value, transactionDto.MaterialId, transactionDto.Quantity);
                }

                // Сохранение всех изменений
                await _materialRepository.SaveChangesAsync();

                // Проверка, не ниже ли уровень запаса минимального
                var material = await _materialRepository.GetByIdAsync(transactionDto.MaterialId);
                if (material != null && material.CurrentStock <= material.MinimumStock)
                {
                    await _notificationService.CreateLowStockNotificationAsync(material.Id);
                }

                return MapToDto(transaction);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не удалось обработать транзакцию: {ex.Message}", ex);
            }
        }

        // *** НОВЫЙ МЕТОД: Обновление количества материала в задаче ***
        private async Task UpdateTaskMaterialQuantityAsync(Guid taskId, Guid materialId, decimal usedQuantity)
        {
            try
            {
                Console.WriteLine($"Начало обновления материалов задачи: TaskId={taskId}, MaterialId={materialId}, UsedQuantity={usedQuantity}");

                // Находим связь материала с задачей
                var taskMaterials = await _taskMaterialRepository.GetByTaskIdAsync(taskId);
                var taskMaterial = taskMaterials.FirstOrDefault(tm => tm.MaterialId == materialId);

                if (taskMaterial != null)
                {
                    Console.WriteLine($"Найден материал в задаче: текущее количество = {taskMaterial.Quantity}");

                    // Уменьшаем количество материала в задаче на использованное количество
                    if (taskMaterial.Quantity >= usedQuantity)
                    {
                        taskMaterial.Quantity -= usedQuantity;
                        taskMaterial.UpdatedAt = DateTime.UtcNow;

                        await _taskMaterialRepository.UpdateAsync(taskMaterial);
                        Console.WriteLine($"Обновлено количество материала {materialId} в задаче {taskId}: -{usedQuantity}, осталось: {taskMaterial.Quantity}");

                        // Если количество материала стало 0 или меньше, удаляем связь
                        if (taskMaterial.Quantity <= 0)
                        {
                            await _taskMaterialRepository.DeleteAsync(taskMaterial.Id);
                            Console.WriteLine($"Удалена связь материала {materialId} с задачей {taskId} - материал закончился");
                        }
                    }
                    else
                    {
                        // Если в задаче материала меньше чем списывается
                        Console.WriteLine($"Предупреждение: В задаче {taskId} материала {materialId} меньше ({taskMaterial.Quantity}) чем списывается ({usedQuantity})");

                        // Устанавливаем количество в 0 и удаляем связь
                        await _taskMaterialRepository.DeleteAsync(taskMaterial.Id);
                        Console.WriteLine($"Связь материала {materialId} с задачей {taskId} удалена из-за превышения расхода");
                    }
                }
                else
                {
                    Console.WriteLine($"Материал {materialId} не найден в задаче {taskId}. Связь не создается для расходных операций.");
                    // Для расходных операций мы не создаем новые связи с отрицательными значениями
                    // Это означает, что материал списывается не из задачи, а из общего склада
                }

                // Сохраняем изменения
                await _taskMaterialRepository.SaveChangesAsync();
                Console.WriteLine($"Изменения в материалах задачи {taskId} сохранены");
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем основную транзакцию
                Console.WriteLine($"Ошибка при обновлении материалов задачи: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public async Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByMaterialAsync(Guid materialId)
        {
            var transactions = await _materialRepository.GetTransactionsByMaterialAsync(materialId);
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Расширяем диапазон дат для более точной аналитики
            // Добавляем по одному дню с каждой стороны
            var expandedStartDate = startDate.Date.AddDays(-1);
            var expandedEndDate = endDate.Date.AddDays(1).AddDays(1).AddTicks(-1); // до конца следующего дня

            // Для месячной аналитики также расширяем на месяц
            var monthlyExpandedStartDate = startDate.Date.AddMonths(-1);
            var monthlyExpandedEndDate = endDate.Date.AddMonths(1).AddDays(1).AddTicks(-1);

            // Определяем, какой диапазон использовать на основе длительности периода
            var periodLength = (endDate - startDate).TotalDays;

            DateTime finalStartDate, finalEndDate;

            if (periodLength <= 7) // Для недельных периодов - расширяем на день
            {
                finalStartDate = expandedStartDate;
                finalEndDate = expandedEndDate;
            }
            else if (periodLength <= 31) // Для месячных периодов - расширяем на неделю
            {
                finalStartDate = startDate.Date.AddDays(-7);
                finalEndDate = endDate.Date.AddDays(7).AddDays(1).AddTicks(-1);
            }
            else // Для более длинных периодов - расширяем на месяц
            {
                finalStartDate = monthlyExpandedStartDate;
                finalEndDate = monthlyExpandedEndDate;
            }

            try
            {
                var transactions = await _materialRepository.GetTransactionsByDateRangeAsync(finalStartDate, finalEndDate);
                return transactions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем пустую коллекцию
                Console.WriteLine($"Ошибка при получении транзакций материалов: {ex.Message}");
                return Enumerable.Empty<MaterialTransactionDto>();
            }
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
                Specifications = material.Specifications,
                Manufacturer = material.Manufacturer,
                PartNumber = material.PartNumber,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }

        private MaterialTransactionDto MapToDto(MaterialTransaction transaction)
        {
            if (transaction == null)
                return null;

            return new MaterialTransactionDto
            {
                Id = transaction.Id,
                MaterialId = transaction.MaterialId,
                MaterialName = transaction.Material?.Name ?? "Материал не найден",
                Quantity = transaction.Quantity,
                TransactionType = transaction.TransactionType ?? "Не указано",
                TaskId = transaction.TaskId,
                TaskTitle = transaction.Task?.Title ?? (transaction.TaskId.HasValue ? "Задача не найдена" : null),
                UserId = transaction.UserId,
                UserName = GetUserDisplayName(transaction.User),
                Notes = string.IsNullOrEmpty(transaction.Notes) ? "" : transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }

        private string GetUserDisplayName(User user)
        {
            if (user == null)
                return "Пользователь не найден";

            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
                return $"{user.FirstName} {user.LastName}";

            if (!string.IsNullOrEmpty(user.UserName))
                return user.UserName;

            return "Неизвестный пользователь";
        }
        // Остальные методы остаются без изменений...
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

        public async Task UpdateStockAsync(Guid materialId, decimal quantity, bool isAddition)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            if (material == null)
            {
                throw new ApplicationException($"Материал с ID {materialId} не найден");
            }

            if (isAddition)
            {
                material.CurrentStock += quantity;
            }
            else
            {
                if (material.CurrentStock < quantity)
                {
                    throw new ApplicationException($"Недостаточно материала '{material.Name}' на складе. В наличии: {material.CurrentStock} {material.UnitOfMeasure}");
                }
                material.CurrentStock -= quantity;
            }

            if (material.CurrentStock <= material.MinimumStock)
            {
                if (_notificationService != null)
                {
                    await _notificationService.CreateLowStockNotificationAsync(materialId);
                }
            }

            await _materialRepository.UpdateAsync(material);
            await _materialRepository.SaveChangesAsync();
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