using System;
namespace ISUMPK2.Domain.Entities
{
    public class TaskComment : BaseEntity
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Comment { get; set; }

        // Навигационные свойства
        public WorkTask Task { get; set; }
        public User User { get; set; }
    }
}
