using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Department>> GetDepartmentsWithHeadAsync()
        {
            return await _dbSet.Include(d => d.Head).ToListAsync();
        }

        public async Task<Department> GetDepartmentByNameAsync(string name)
        {
            return await _dbSet
                .Include(d => d.Head)
                .FirstOrDefaultAsync(d => d.Name == name);
        }

        public override async Task<Department> GetByIdAsync(Guid id)
        {
            // Переопределяем метод для загрузки связанных данных
            return await _dbSet
                .Include(d => d.Head)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public override async Task<IEnumerable<Department>> GetAllAsync()
        {
            // Переопределяем метод для загрузки связанных данных для всех департаментов
            return await _dbSet
                .Include(d => d.Head)
                .ToListAsync();
        }
    }
}