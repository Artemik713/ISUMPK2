using System;
using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? HeadId { get; set; }

        // Навигационные свойства
        public User Head { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<WorkTask> Tasks { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}
