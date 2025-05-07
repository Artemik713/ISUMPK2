using System;

namespace ISUMPK2.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid? ReceiverId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }

        // Навигационные свойства
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public Department Department { get; set; }
    }
}
