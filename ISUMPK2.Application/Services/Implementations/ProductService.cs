using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IRepository<ProductType> _productTypeRepository;
        private readonly IRepository<Department> _departmentRepository;

        public ProductService(
            IProductRepository productRepository,
            IMaterialRepository materialRepository,
            IRepository<ProductType> productTypeRepository,
            IRepository<Department> departmentRepository)
        {
            _productRepository = productRepository;
            _materialRepository = materialRepository;
            _productTypeRepository = productTypeRepository;
            _departmentRepository = departmentRepository;
        }

        #region Методы для работы с продуктами

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByTypeAsync(Guid productTypeId)
        {
            var products = await _productRepository.GetProductsByTypeAsync(productTypeId);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto productDto)
        {
            // Добавьте логирование
            Console.WriteLine($"Received product data: DepartmentId: {productDto.DepartmentId}, " +
                             $"Weight: {productDto.Weight}, Material: {productDto.Material}");

            var product = new Product
            {
                Code = productDto.Code,
                Name = productDto.Name,
                Description = productDto.Description,
                ProductTypeId = productDto.ProductTypeId,
                UnitOfMeasure = productDto.UnitOfMeasure,
                CurrentStock = productDto.CurrentStock,
                Price = productDto.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DepartmentId = productDto.DepartmentId,
                Weight = productDto.Weight,
                Dimensions = productDto.Dimensions,
                Material = productDto.Material,
                ProductionTime = productDto.ProductionTime,
                TechnologyMap = productDto.TechnologyMap,
                ImageUrl = productDto.ImageUrl,
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            // Используем существующий метод репозитория вместо локального метода
            var createdProduct = await _productRepository.GetByIdWithDetailsAsync(product.Id);

            // Проверка сохраненных значений
            Console.WriteLine($"Saved product: ID: {product.Id}, DepartmentId: {product.DepartmentId}, " +
                            $"Weight: {product.Weight}, Material: {product.Material}");

            return MapToDto(createdProduct ?? product);
        }


        public async Task<ProductDto> UpdateProductAsync(Guid id, ProductUpdateDto productDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.ProductTypeId = productDto.ProductTypeId;
            product.UnitOfMeasure = productDto.UnitOfMeasure;
            product.Price = productDto.Price;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> HasSufficientMaterialsForProductionAsync(Guid productId, decimal quantity)
        {
            return await _productRepository.HasSufficientMaterialsForProductionAsync(productId, quantity);
        }

        #endregion

        #region Методы для работы с типами продуктов

        public async Task<ProductTypeDto> GetProductTypeByIdAsync(Guid id)
        {
            var productType = await _productTypeRepository.GetByIdAsync(id);
            return productType != null ? MapToDto(productType) : null;
        }

        public async Task<IEnumerable<ProductTypeDto>> GetAllProductTypesAsync()
        {
            var productTypes = await _productTypeRepository.GetAllAsync();
            return productTypes.Select(MapToDto);
        }

        public async Task<ProductTypeDto> CreateProductTypeAsync(ProductTypeCreateDto productTypeDto)
        {
            var productType = new ProductType
            {
                Name = productTypeDto.Name,
                Description = productTypeDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productTypeRepository.AddAsync(productType);
            await _productTypeRepository.SaveChangesAsync();

            return MapToDto(productType);
        }

        public async Task<ProductTypeDto> UpdateProductTypeAsync(Guid id, ProductTypeCreateDto productTypeDto)
        {
            var productType = await _productTypeRepository.GetByIdAsync(id);
            if (productType == null)
                return null;

            productType.Name = productTypeDto.Name;
            productType.Description = productTypeDto.Description;
            productType.UpdatedAt = DateTime.UtcNow;

            await _productTypeRepository.UpdateAsync(productType);
            await _productTypeRepository.SaveChangesAsync();

            return MapToDto(productType);
        }

        public async Task DeleteProductTypeAsync(Guid id)
        {
            await _productTypeRepository.DeleteAsync(id);
            await _productTypeRepository.SaveChangesAsync();
        }

        #endregion

        #region Методы для работы с материалами продуктов

        public async Task<IEnumerable<ProductMaterialDto>> GetProductMaterialsAsync(Guid productId)
        {
            var productMaterials = await _productRepository.GetProductMaterialsAsync(productId);
            return productMaterials.Select(MapToDto);
        }

        #endregion

        #region Методы для работы с транзакциями продуктов

        public async Task<ProductTransactionDto> AddTransactionAsync(Guid userId, ProductTransactionCreateDto transactionDto)
        {
            // Проверка наличия достаточного количества материалов для производства
            if (transactionDto.TransactionType == "Production")
            {
                var hasSufficientMaterials = await _productRepository.HasSufficientMaterialsForProductionAsync(transactionDto.ProductId, transactionDto.Quantity);
                if (!hasSufficientMaterials)
                {
                    throw new InvalidOperationException("Insufficient materials for production");
                }

                // Списание материалов
                var productMaterials = await _productRepository.GetProductMaterialsAsync(transactionDto.ProductId);
                foreach (var pm in productMaterials)
                {
                    await _materialRepository.UpdateStockAsync(pm.MaterialId, pm.Quantity * transactionDto.Quantity, false);
                }
            }
            else if (transactionDto.TransactionType == "Shipment")
            {
                // Проверка наличия достаточного количества продукции для отгрузки
                var product = await _productRepository.GetByIdAsync(transactionDto.ProductId);
                if (product.CurrentStock < transactionDto.Quantity)
                {
                    throw new InvalidOperationException("Insufficient product stock for shipment");
                }
            }

            // Обновление остатка продукции
            bool isAddition = transactionDto.TransactionType == "Production";
            await _productRepository.UpdateStockAsync(transactionDto.ProductId, transactionDto.Quantity, isAddition);

            // Создание записи транзакции
            var transaction = new ProductTransaction
            {
                ProductId = transactionDto.ProductId,
                Quantity = transactionDto.Quantity,
                TransactionType = transactionDto.TransactionType,
                TaskId = transactionDto.TaskId,
                UserId = userId, // Используем переданный userId вместо свойства из DTO
                Notes = transactionDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Добавление транзакции в базу данных
            await _productRepository.SaveChangesAsync();

            return new ProductTransactionDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                ProductName = transaction.Product?.Name,
                Quantity = transaction.Quantity,
                TransactionType = transaction.TransactionType,
                TaskId = transaction.TaskId,
                UserId = transaction.UserId,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }

        public async Task<ProductDto> GetByIdWithDetailsAsync(Guid id)
        {
            try
            {
                // Получаем продукт с деталями через репозиторий
                var product = await _productRepository.GetByIdWithDetailsAsync(id);

                if (product == null)
                    return null;

                // Создаем DTO и заполняем всю информацию
                var dto = MapToDto(product);

                // Логирование для диагностики
                Console.WriteLine($"Загружен продукт: ID={product.Id}, " +
                                 $"DepartmentId={product.DepartmentId}, " +
                                 $"Department={product.Department?.Name ?? "null"}, " +
                                 $"ProductType={product.ProductType?.Name ?? "null"}, " +
                                 $"Materials Count: {product.ProductMaterials?.Count ?? 0}");

                // Если материалов нет в основных данных, загрузим их отдельно
                if ((dto.Materials == null || !dto.Materials.Any()) && product.ProductMaterials != null)
                {
                    dto.Materials = product.ProductMaterials.Select(pm => new ProductMaterialDto
                    {
                        ProductId = pm.ProductId,
                        MaterialId = pm.MaterialId,
                        MaterialName = pm.Material?.Name,
                        MaterialCode = pm.Material?.Code,
                        Quantity = pm.Quantity,
                        UnitOfMeasure = pm.Material?.UnitOfMeasure
                    }).ToList();
                }

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetByIdWithDetailsAsync: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<ProductTransactionDto>> GetTransactionsByProductAsync(Guid productId)
        {
            var transactions = await _productRepository.GetTransactionsByProductAsync(productId);
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _productRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
            return transactions.Select(MapToDto);
        }

        #endregion

        #region Методы маппинга на DTO

        private ProductDto MapToDto(Product product)
        {
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                ProductTypeId = product.ProductTypeId,
                ProductTypeName = product.ProductType?.Name ?? "Без категории",
                DepartmentId = product.DepartmentId ?? Guid.Empty,
                DepartmentName = product.Department?.Name,
                UnitOfMeasure = product.UnitOfMeasure,
                CurrentStock = product.CurrentStock,
                Price = product.Price,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                Material = product.Material,
                TechnologyMap = product.TechnologyMap,
                ProductionTime = product.ProductionTime,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Materials = product.ProductMaterials?.Select(MapToDto).ToList() ?? new List<ProductMaterialDto>()
            };
        }

        private ProductTypeDto MapToDto(ProductType productType)
        {
            return new ProductTypeDto
            {
                Id = productType.Id,
                Name = productType.Name,
                Description = productType.Description
            };
        }

        private ProductMaterialDto MapToDto(ProductMaterial productMaterial)
        {
            return new ProductMaterialDto
            {
                ProductId = productMaterial.ProductId,
                MaterialId = productMaterial.MaterialId,
                MaterialName = productMaterial.Material?.Name,
                MaterialCode = productMaterial.Material?.Code,
                Quantity = productMaterial.Quantity,
                UnitOfMeasure = productMaterial.Material?.UnitOfMeasure
            };
        }

        private ProductTransactionDto MapToDto(ProductTransaction transaction)
        {
            return new ProductTransactionDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                ProductName = transaction.Product?.Name,
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

        public async Task UpdateProductMaterialsAsync(Guid productId, List<ProductMaterialCreateDto> materials)
        {
            // Удаляем существующие связи
            await _productRepository.RemoveAllProductMaterialsAsync(productId);

            // Добавляем новые связи
            foreach (var material in materials)
            {
                await _productRepository.AddProductMaterialAsync(new ProductMaterial
                {
                    ProductId = productId,
                    MaterialId = material.MaterialId,
                    Quantity = material.Quantity
                });
            }

            await _productRepository.SaveChangesAsync();
            Console.WriteLine($"Сохранено материалов: {materials.Count}");
        }
        #endregion


    }
}
