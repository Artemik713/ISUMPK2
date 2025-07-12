using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Entities
{
    // ISUMPK2.Domain/Entities/TaskMaterial.cs
    public class TaskMaterial : BaseEntity
    {
        [Required]
        public Guid TaskId { get; set; }
        [Required]
        public Guid MaterialId { get; set; }
        [Required]
        public decimal Quantity { get; set; }

        // Навигационные свойства
        public WorkTask WorkTask { get; set; }
        public Material Material { get; set; }
    }
}
