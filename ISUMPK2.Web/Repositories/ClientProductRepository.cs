using ISUMPK2.Application.DTOs;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientProductRepository : ClientRepositoryBase<Product>, IProductRepository
    {
        protected override string ApiEndpoint => "api/products";

        public ClientProductRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByStatusAsync(string status)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Product>>($"{ApiEndpoint}/status/{Uri.EscapeDataString(status)}");
        }

        public async Task<IEnumerable<ProductTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<ProductTransaction>>($"{ApiEndpoint}/transactions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        }

        public async Task<IEnumerable<ProductTransaction>> GetTransactionsByProductAsync(Guid productId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<ProductTransaction>>($"{ApiEndpoint}/{productId}/transactions");
        }

        public async Task UpdateStatusAsync(Guid productId, string newStatus)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/{productId}/status/{Uri.EscapeDataString(newStatus)}", null);
        }

        public async Task UpdateStockAsync(Guid productId, decimal quantity, bool isAddition)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/{productId}/stock?quantity={quantity}&isAddition={isAddition}", null);
        }

        public async Task<IEnumerable<Product>> GetProductsByTypeAsync(Guid typeId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Product>>($"{ApiEndpoint}/type/{typeId}");
        }

        public async Task<IEnumerable<ProductMaterial>> GetProductMaterialsAsync(Guid productId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<ProductMaterial>>($"{ApiEndpoint}/{productId}/materials");
        }

        public async Task<bool> HasSufficientMaterialsForProductionAsync(Guid productId, decimal quantity)
        {
            return await HttpClient.GetFromJsonAsync<bool>($"{ApiEndpoint}/{productId}/check-materials/{quantity}");
        }
        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                // Исправлено: используем ApiEndpoint вместо BaseUrl
                var response = await HttpClient.GetAsync($"{ApiEndpoint}");
                response.EnsureSuccessStatusCode();

                var productsDto = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();

                // Конвертируем DTO в сущности с навигационными свойствами
                var products = new List<Product>();
                foreach (var dto in productsDto)
                {
                    var product = new Product
                    {
                        Id = dto.Id,
                        Code = dto.Code,
                        Name = dto.Name,
                        Description = dto.Description,
                        ProductTypeId = dto.ProductTypeId,
                        UnitOfMeasure = dto.UnitOfMeasure ?? "шт.",
                        CurrentStock = dto.CurrentStock,
                        Price = dto.Price,
                        ImageUrl = dto.ImageUrl,
                        DepartmentId = dto.DepartmentId,
                        Weight = dto.Weight,
                        Dimensions = dto.Dimensions,
                        Material = dto.Material,
                        TechnologyMap = dto.TechnologyMap,
                        ProductionTime = dto.ProductionTime,
                        CreatedAt = dto.CreatedAt,
                        UpdatedAt = dto.UpdatedAt
                    };

                    // Создаем навигационное свойство ProductType если доступно имя типа
                    if (!string.IsNullOrEmpty(dto.ProductTypeName))
                    {
                        product.ProductType = new ProductType
                        {
                            Id = dto.ProductTypeId,
                            Name = dto.ProductTypeName
                        };
                    }

                    // Добавляем материалы если они доступны
                    if (dto.Materials != null)
                    {
                        product.ProductMaterials = dto.Materials.Select(m => new ProductMaterial
                        {
                            ProductId = dto.Id,
                            MaterialId = m.MaterialId,
                            Quantity = m.Quantity,
                            Material = new Material
                            {
                                Id = m.MaterialId,
                                Name = m.MaterialName,
                                UnitOfMeasure = m.UnitOfMeasure
                            }
                        }).ToList();
                    }
                    else
                    {
                        product.ProductMaterials = new List<ProductMaterial>();
                    }

                    products.Add(product);
                }

                Console.WriteLine($"Загружено {products.Count} продуктов с категориями");
                foreach (var p in products)
                {
                    Console.WriteLine($"Продукт: {p.Name}, TypeId: {p.ProductTypeId}, TypeName: {p.ProductType?.Name ?? "null"}");
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка продуктов: {ex.Message}");
                throw;
            }
        }
        public async Task<ProductDto> UpdateProductAsync(Guid id, ProductUpdateDto productDto)
        {
            try
            {
                // Явно указываем полный путь с идентификатором
                var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", productDto);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ProductDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка при обновлении продукта: {ex.Message}");
                throw;
            }
        }
        public async Task<Product?> GetByIdWithDetailsAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"Запрос деталей продукта с ID: {id}");

                // Десериализуем в динамический объект, чтобы понять структуру ответа
                var response = await HttpClient.GetAsync($"{ApiEndpoint}/{id}/details");
                response.EnsureSuccessStatusCode();

                // Создаём экземпляр продукта для заполнения
                var product = new Product();

                // Десериализуем ответ в ProductDto и затем копируем в Product
                var productDto = await response.Content.ReadFromJsonAsync<ProductDto>();

                if (productDto != null)
                {
                    // Заполняем основные поля
                    product.Id = productDto.Id;
                    product.Code = productDto.Code;
                    product.Name = productDto.Name;
                    product.Description = productDto.Description;
                    product.ProductTypeId = productDto.ProductTypeId;
                    product.UnitOfMeasure = productDto.UnitOfMeasure ?? "шт.";
                    product.CurrentStock = productDto.CurrentStock;
                    product.Price = productDto.Price;
                    product.ImageUrl = productDto.ImageUrl;
                    product.DepartmentId = productDto.DepartmentId;
                    product.Weight = productDto.Weight;
                    product.Dimensions = productDto.Dimensions;
                    product.Material = productDto.Material;
                    product.TechnologyMap = productDto.TechnologyMap;
                    product.ProductionTime = productDto.ProductionTime;
                    product.CreatedAt = productDto.CreatedAt;
                    product.UpdatedAt = productDto.UpdatedAt;

                    // Создаём навигационные свойства
                    // Исправленный код
                    if (!string.IsNullOrEmpty(productDto.DepartmentName))
                    {
                        product.Department = new Department
                        {
                            Id = productDto.DepartmentId,
                            Name = productDto.DepartmentName
                        };
                    }

                    if (productDto.ProductTypeName != null)
                    {
                        product.ProductType = new ProductType
                        {
                            Id = productDto.ProductTypeId,
                            Name = productDto.ProductTypeName
                        };
                    }

                    // Заполняем коллекцию материалов
                    product.ProductMaterials = productDto.Materials?.Select(m => new ProductMaterial
                    {
                        ProductId = productDto.Id,
                        MaterialId = m.MaterialId,
                        Quantity = m.Quantity,
                        Material = new Material
                        {
                            Id = m.MaterialId,
                            Name = m.MaterialName,
                            UnitOfMeasure = m.UnitOfMeasure
                        }
                    }).ToList() ?? new List<ProductMaterial>();

                    Console.WriteLine($"Десериализован продукт: {product.Name}");
                    Console.WriteLine($"Цех: ID={product.DepartmentId}, Name={product.Department?.Name ?? "null"}");
                    Console.WriteLine($"Материалов: {product.ProductMaterials?.Count ?? 0}");

                    // Если материалов нет, попробуйте загрузить их отдельным запросом
                    if (product.ProductMaterials.Count == 0)
                    {
                        try
                        {
                            var materials = await GetProductMaterialsAsync(id);
                            product.ProductMaterials = materials.ToList();
                            Console.WriteLine($"Дополнительно загружено материалов: {product.ProductMaterials.Count}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Не удалось загрузить материалы отдельным запросом: {ex.Message}");
                        }
                    }
                }

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении деталей продукта: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
        public async Task AddProductMaterialAsync(ProductMaterial productMaterial)
        {
            try
            {
                await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{productMaterial.ProductId}/materials", productMaterial);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении материала к продукту: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveAllProductMaterialsAsync(Guid productId)
        {
            try
            {
                await HttpClient.DeleteAsync($"{ApiEndpoint}/{productId}/materials");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении всех материалов продукта: {ex.Message}");
                throw;
            }
        }
    }
}
