using System;
using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class ProductType : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        // Навигационные свойства
        public ICollection<Product> Products { get; set; }
    }
}
