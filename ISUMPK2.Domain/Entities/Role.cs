using System;
using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string Description { get; set; }

        // Навигационные свойства
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
