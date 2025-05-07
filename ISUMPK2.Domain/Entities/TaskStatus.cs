using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class TaskStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Навигационные свойства
        public ICollection<WorkTask> Tasks { get; set; }
    }
}
