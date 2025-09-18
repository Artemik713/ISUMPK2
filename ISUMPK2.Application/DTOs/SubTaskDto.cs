using System;

namespace ISUMPK2.Application.DTOs
{
    public class SubTaskDto
    {
        public Guid Id { get; set; }
        public Guid ParentTaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
    }

    public class SubTaskCreateDto
    {
        public Guid ParentTaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
    }

    public class SubTaskUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
    }
}