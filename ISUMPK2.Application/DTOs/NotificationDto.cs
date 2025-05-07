using System;

namespace ISUMPK2.Application.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public Guid? TaskId { get; set; }
        public string TaskTitle { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationCreateDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Guid? TaskId { get; set; }
    }
}
