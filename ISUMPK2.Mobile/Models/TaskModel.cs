using System;
using System.Collections.Generic;
using SQLite;
namespace ISUMPK2.Mobile.Models
{
    public class TaskModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; }
        public Guid CreatorId { get; set; }
        public string CreatorName { get; set; }
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public bool IsForceMarked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Ignore]
        public List<TaskCommentModel> Comments { get; set; } = new List<TaskCommentModel>();

        // Вычисляемые свойства

        [Ignore]
        public bool IsOverdue { get; set; }

        [Ignore]
        public string StatusBadgeColor { get; set; } // Переименовано с StatusColor на StatusBadgeColor

        [Ignore]
        public string PriorityBadgeColor { get; set; }
    }

    public class TaskCommentModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TaskStatusUpdateModel
    {
        public int StatusId { get; set; }
        public string Comment { get; set; }
    }

    public class TaskCommentCreateModel
    {
        public Guid TaskId { get; set; }
        public string Comment { get; set; }
    }
}
