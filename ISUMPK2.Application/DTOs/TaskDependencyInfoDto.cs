namespace ISUMPK2.Application.DTOs
{
    public class TaskDependencyInfoDto
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; }
        public bool CanDelete { get; set; }
        public List<TaskDependencyDto> Dependencies { get; set; } = new();
        public string Summary { get; set; }
    }

    public class TaskDependencyDto
    {
        public string Type { get; set; } // "SubTasks", "Materials", "Comments", etc.
        public int Count { get; set; }
        public string Description { get; set; }
        public List<TaskDependencyItemDto> Items { get; set; } = new();
    }

    public class TaskDependencyItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string Status { get; set; }
    }
}