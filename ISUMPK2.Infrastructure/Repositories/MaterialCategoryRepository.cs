using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using ISUMPK2.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class MaterialCategoryRepository : Repository<MaterialCategory>, IMaterialCategoryRepository
    {
        public MaterialCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MaterialCategory>> GetTopLevelCategoriesAsync()
        {
            return await _dbSet.Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaterialCategory>> GetSubcategoriesAsync(Guid parentCategoryId)
        {
            return await _dbSet.Where(c => c.ParentCategoryId == parentCategoryId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Material>> GetMaterialsByCategoryAsync(Guid categoryId, bool includeSubcategories = false)
        {
            if (!includeSubcategories)
            {
                return await _context.Set<Material>()
                    .Where(m => m.CategoryId == categoryId)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }

            // Получаем все подкатегории рекурсивно
            var subcategoryIds = new List<Guid> { categoryId };
            var categoriesToCheck = new Queue<Guid>();
            categoriesToCheck.Enqueue(categoryId);

            while (categoriesToCheck.Count > 0)
            {
                var currentId = categoriesToCheck.Dequeue();
                var subcategories = await _dbSet
                    .Where(c => c.ParentCategoryId == currentId)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var id in subcategories)
                {
                    subcategoryIds.Add(id);
                    categoriesToCheck.Enqueue(id);
                }
            }

            return await _context.Set<Material>()
                .Where(m => m.CategoryId.HasValue && subcategoryIds.Contains(m.CategoryId.Value))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
    }
}
