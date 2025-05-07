using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByTypeAsync(Guid productTypeId)
        {
            return await _dbSet
                .Where(p => p.ProductTypeId == productTypeId)
                .Include(p => p.ProductType)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductMaterial>> GetProductMaterialsAsync(Guid productId)
        {
            return await _context.Set<ProductMaterial>()
                .Where(pm => pm.ProductId == productId)
                .Include(pm => pm.Material)
                .ToListAsync();
        }

        public async Task<bool> HasSufficientMaterialsForProductionAsync(Guid productId, decimal quantity)
        {
            var productMaterials = await GetProductMaterialsAsync(productId);

            foreach (var pm in productMaterials)
            {
                var material = await _context.Set<Material>().FindAsync(pm.MaterialId);
                if (material == null || material.CurrentStock < (pm.Quantity * quantity))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task UpdateStockAsync(Guid productId, decimal quantity, bool isAddition)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product != null)
            {
                if (isAddition)
                {
                    product.CurrentStock += quantity;
                }
                else
                {
                    if (product.CurrentStock >= quantity)
                    {
                        product.CurrentStock -= quantity;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductTransaction>> GetTransactionsByProductAsync(Guid productId)
        {
            return await _context.Set<ProductTransaction>()
                .Where(t => t.ProductId == productId)
                .Include(t => t.User)
                .Include(t => t.Task)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<ProductTransaction>()
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .Include(t => t.Product)
                .Include(t => t.User)
                .Include(t => t.Task)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
