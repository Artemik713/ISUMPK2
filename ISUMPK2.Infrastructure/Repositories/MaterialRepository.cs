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
    public class MaterialRepository : Repository<Material>, IMaterialRepository
    {
        public MaterialRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Material>> GetMaterialsWithLowStockAsync()
        {
            return await _dbSet
                .Where(m => m.CurrentStock <= m.MinimumStock)
                .ToListAsync();
        }

        public async Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity)
        {
            var material = await _dbSet.FindAsync(materialId);
            return material != null && material.CurrentStock >= requiredQuantity;
        }

        public async Task UpdateStockAsync(Guid materialId, decimal quantity, bool isAddition)
        {
            var material = await _dbSet.FindAsync(materialId);
            if (material != null)
            {
                if (isAddition)
                {
                    material.CurrentStock += quantity;
                }
                else
                {
                    if (material.CurrentStock >= quantity)
                    {
                        material.CurrentStock -= quantity;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Insufficient stock for material {material.Name}");
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MaterialTransaction>> GetTransactionsByMaterialAsync(Guid materialId)
        {
            return await _context.Set<MaterialTransaction>()
                .Where(t => t.MaterialId == materialId)
                .Include(t => t.User)
                .Include(t => t.Task)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaterialTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<MaterialTransaction>()
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .Include(t => t.Material)
                .Include(t => t.User)
                .Include(t => t.Task)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
