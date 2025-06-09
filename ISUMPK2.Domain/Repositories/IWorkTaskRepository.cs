using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Repositories
{
    public interface IWorkTaskRepository : IRepository<WorkTask>
    {
        // Здесь можно добавить специфичные для WorkTask методы, 
        // если они потребуются в будущем
    }
}