using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Application.Services
{
    public interface IProductService
    {
        Task<ProductTypeDto> GetProductTypeByIdAsync(Guid id);
        Task<IEnumerable<ProductTypeDto>> GetAllProductTypesAsync();
        Task<ProductTypeDto> CreateProductTypeAsync(ProductTypeCreateDto productTypeDto);
        Task<ProductTypeDto> UpdateProductTypeAsync(Guid id, ProductTypeCreateDto productTypeDto);
        Task DeleteProductTypeAsync(Guid id);

        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByTypeAsync(Guid productTypeId);
        Task<ProductDto> CreateProductAsync(ProductCreateDto productDto);
        Task<ProductDto> UpdateProductAsync(Guid id, ProductUpdateDto productDto);
        Task DeleteProductAsync(Guid id);

        Task<bool> HasSufficientMaterialsForProductionAsync(Guid productId, decimal quantity);
        Task<ProductTransactionDto> AddTransactionAsync(Guid userId, ProductTransactionCreateDto transactionDto);
        Task<IEnumerable<ProductTransactionDto>> GetTransactionsByProductAsync(Guid productId);
        Task<IEnumerable<ProductTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<IEnumerable<ProductMaterialDto>> GetProductMaterialsAsync(Guid productId);
    }
}
