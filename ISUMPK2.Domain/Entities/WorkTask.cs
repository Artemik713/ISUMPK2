using System;
using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class WorkTask : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public int PriorityId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public Guid? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public bool IsForceMarked { get; set; }

        // Навигационные свойства
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public User Creator { get; set; }
        public User Assignee { get; set; }
        public Department Department { get; set; }
        public Product Product { get; set; }
        public ICollection<TaskComment> Comments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<MaterialTransaction> MaterialTransactions { get; set; }
        public ICollection<ProductTransaction> ProductTransactions { get; set; }
    }
}
