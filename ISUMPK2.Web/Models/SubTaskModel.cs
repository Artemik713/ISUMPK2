using System;

namespace ISUMPK2.Web.Models
{
    public class SubTaskModel
    {
        public Guid Id { get; set; }
        public Guid ParentTaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
    }
}