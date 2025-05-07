using System;
using System.Collections.Generic;

namespace ISUMPK2.Application.DTOs
{
    public class TaskDto
    {
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
        public List<TaskCommentDto> Comments { get; set; } = new List<TaskCommentDto>();
    }

    public class TaskCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; } = 1; // По умолчанию "Создана"
        public int PriorityId { get; set; } = 2; // По умолчанию "Средний"
        public Guid? AssigneeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public Guid? ProductId { get; set; }
        public decimal? Quantity { get; set; }
    }

    public class TaskUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public int PriorityId { get; set; }
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
    }

    public class TaskStatusUpdateDto
    {
        public int StatusId { get; set; }
        public string Comment { get; set; }
    }

    public class TaskCommentDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TaskCommentCreateDto
    {
        public Guid TaskId { get; set; }
        public string Comment { get; set; }
    }
}
