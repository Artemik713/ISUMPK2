using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Repositories
{
    public interface IMaterialCategoryRepository : IRepository<MaterialCategory>
    {
        Task<IEnumerable<MaterialCategory>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<MaterialCategory>> GetSubcategoriesAsync(Guid parentCategoryId);
        Task<IEnumerable<Material>> GetMaterialsByCategoryAsync(Guid categoryId, bool includeSubcategories = false);
    }
}
