using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual Task UpdateAsync(T entity)
        {
            // Получаем текущее состояние из базы данных
            var existingEntity = _dbSet.Find(entity.Id);
            if (existingEntity != null)
            {
                // Сохраняем значения полей, которые не нужно обновлять
                var createdAt = existingEntity.CreatedAt;

                // Помечаем сущность как измененную
                _context.Entry(entity).State = EntityState.Modified;

                // Если в базе данных нет столбца CreatedAt, то исключаем его из обновления
                _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
                _context.Entry(entity).Property(e => e.UpdatedAt).IsModified = false;

                // Устанавливаем сохраненное значение CreatedAt
                entity.CreatedAt = createdAt;
            }
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
