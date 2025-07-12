using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace ISUMPK2.Mobile.Models
{
    public class TaskModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public Guid CreatorId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal? Quantity { get; set; }
        public bool IsForceMarked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Ignore]
        public List<TaskCommentModel> Comments { get; set; } = new List<TaskCommentModel>();

        [Ignore]
        public List<SubTaskModel> SubTasks { get; set; } = new();

        // Вычисляемые свойства (только в get; set; формате для SQLite)
        [Ignore]
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && StatusId < 5;

        [Ignore]
        public string StatusBadgeColor => GetStatusBadgeColor(StatusId);

        [Ignore]
        public string PriorityBadgeColor => GetPriorityBadgeColor(PriorityId);

        [Ignore]
        public int SubTasksProgress
        {
            get
            {
                if (!SubTasks.Any()) return 0;
                var completedCount = SubTasks.Count(st => st.StatusId == 5);
                return (int)((double)completedCount / SubTasks.Count * 100);
            }
        }

        [Ignore]
        public int CompletedSubTasksCount => SubTasks.Count(st => st.StatusId == 5);

        [Ignore]
        public int TotalSubTasksCount => SubTasks.Count;

        private static string GetStatusBadgeColor(int statusId)
        {
            return statusId switch
            {
                1 => "Default",  // Создана
                2 => "Info",     // В работе
                3 => "Warning",  // Требует уточнения
                4 => "Primary",  // На проверке
                5 => "Success",  // Выполнена
                6 => "Error",    // Отклонена
                _ => "Default"
            };
        }

        private static string GetPriorityBadgeColor(int priorityId)
        {
            return priorityId switch
            {
                1 => "Default",     // Низкий
                2 => "Info",        // Средний
                3 => "Warning",     // Высокий
                4 => "Error",       // Критический
                _ => "Default"
            };
        }
    }

    public class TaskCommentModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SubTaskModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public Guid ParentTaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
    }

    public class TaskStatusUpdateModel
    {
        public int StatusId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class TaskCommentCreateModel
    {
        public Guid TaskId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}