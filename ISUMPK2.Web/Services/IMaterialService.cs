// ISUMPK2.Web/Services/IMaterialService.cs
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public interface IMaterialService
    {
        Task<IEnumerable<MaterialModel>> GetMaterialsAsync();
        Task<MaterialModel?> GetMaterialByIdAsync(Guid id);
        Task<MaterialModel> CreateMaterialAsync(MaterialModel model);
        Task UpdateMaterialAsync(MaterialModel model);
        Task DeleteMaterialAsync(Guid id);
        Task<IEnumerable<MaterialModel>> GetMaterialsWithLowStockAsync();
    }
}
