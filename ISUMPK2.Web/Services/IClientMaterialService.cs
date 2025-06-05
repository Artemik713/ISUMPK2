using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public interface IClientMaterialService
    {
        Task<IEnumerable<MaterialDto>> GetAllAsync();
        Task<MaterialDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MaterialDto>> GetByCategoryAsync(Guid categoryId);
        Task<MaterialDto> CreateAsync(MaterialCreateDto createDto);
        Task<MaterialDto> UpdateAsync(Guid id, MaterialUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}