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
        public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Now.Date && StatusId != 5 && StatusId != 6;

        [Ignore]
        public string StatusColor => StatusId switch
        {
            1 => "#2196F3", // Создана - Primary
            2 => "#03A9F4", // В работе - Info
            3 => "#FF9800", // Требует уточнения - Warning
            4 => "#9E9E9E", // На проверке - Secondary
            5 => "#4CAF50", // Выполнена - Success
            6 => "#F44336", // Отклонена - Error
            _ => "#9E9E9E"  // Default - Secondary
        };

        [Ignore]
        public string PriorityColor => PriorityId switch
        {
            1 => "#03A9F4", // Низкий - Info
            2 => "#4CAF50", // Средний - Success
            3 => "#FF9800", // Высокий - Warning
            4 => "#F44336", // Критический - Error
            _ => "#9E9E9E"  // Default - Secondary
        };
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
