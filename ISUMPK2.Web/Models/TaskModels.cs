namespace ISUMPK2.Web.Models
{
    public class TaskModel
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
        public List<TaskCommentModel> Comments { get; set; } = new List<TaskCommentModel>();

        // Вычисляемые свойства для UI
        public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Now.Date && StatusId != 5 && StatusId != 6;
        public string StatusBadgeColor => StatusId switch
        {
            1 => "primary", // Создана
            2 => "info",    // В работе
            3 => "warning", // Требует уточнения
            4 => "secondary", // На проверке
            5 => "success", // Выполнена
            6 => "error",   // Отклонена
            _ => "default"
        };
        public string PriorityBadgeColor => PriorityId switch
        {
            1 => "info",    // Низкий
            2 => "success", // Средний
            3 => "warning", // Высокий
            4 => "error",   // Критический
            _ => "default"
        };
    }

    public class TaskCreateModel
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

    public class TaskUpdateModel
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

    public class TaskStatusUpdateModel
    {
        public int StatusId { get; set; }
        public string Comment { get; set; }
    }

    public class TaskCommentModel
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class StatusModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PriorityModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TaskCommentCreateModel
    {
        public Guid TaskId { get; set; }
        public string Comment { get; set; }
    }
}
