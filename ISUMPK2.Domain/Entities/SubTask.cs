using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace ISUMPK2.Domain.Entities
{
    public class SubTask : BaseEntity
    {
        [Required]
        public Guid ParentTaskId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public Guid? AssigneeId { get; set; }

        public int StatusId { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public decimal? EstimatedHours { get; set; }

        public decimal? ActualHours { get; set; }

        // Навигационные свойства
        public virtual WorkTask ParentTask { get; set; }
        public virtual TaskStatus Status { get; set; }
        public virtual User Assignee { get; set; }
    }
}