using System;

namespace ISUMPK2.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public Guid? TaskId { get; set; }

        // Навигационные свойства
        public User User { get; set; }
        public WorkTask Task { get; set; }
    }
}
