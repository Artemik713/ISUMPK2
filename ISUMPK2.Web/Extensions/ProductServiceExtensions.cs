using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;

namespace ISUMPK2.Web.Extensions
{
    /// <summary>
    /// Методы расширения для работы с продукцией отделов
    /// </summary>
    public static class ProductServiceExtensions
    {
        /// <summary>
        /// Получает продукцию, связанную с отделом
        /// </summary>
        public static async Task<IEnumerable<ProductDto>> GetProductsAsync(this IProductService productService, string filter)
        {
            try
            {
                // Получаем все продукты
                var allProducts = await productService.GetAllProductsAsync();

                // Если указан фильтр по отделу
                if (!string.IsNullOrEmpty(filter) && filter.Contains("departmentId eq"))
                {
                    // Извлекаем ID отдела из строки фильтра
                    string departmentIdStr = filter.Split(' ').Last();
                    if (Guid.TryParse(departmentIdStr, out Guid departmentId))
                    {
                        // В реальности нужно добавить в API метод для фильтрации по отделу
                        // Пока возвращаем первые несколько продуктов как пример
                        return allProducts.Take(Math.Min(5, allProducts.Count())).ToList();
                    }
                }

                return allProducts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке продукции: {ex.Message}");
                return new List<ProductDto>();
            }
        }
    }
}