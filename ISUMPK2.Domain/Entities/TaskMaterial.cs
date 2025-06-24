using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Entities
{
    // ISUMPK2.Domain/Entities/TaskMaterial.cs
    public class TaskMaterial : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }

        // Навигационные свойства
        public WorkTask WorkTask { get; set; }
        public Material Material { get; set; }
    }
}
