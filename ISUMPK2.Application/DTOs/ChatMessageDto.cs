using System;

namespace ISUMPK2.Application.DTOs
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public Guid? ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChatMessageCreateDto
    {
        public Guid? ReceiverId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Message { get; set; }
    }
}
