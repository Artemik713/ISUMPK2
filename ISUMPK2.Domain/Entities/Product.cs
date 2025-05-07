using System;
using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProductTypeId { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal Price { get; set; }

        // Навигационные свойства
        public ProductType ProductType { get; set; }
        public ICollection<ProductMaterial> ProductMaterials { get; set; }
        public ICollection<ProductTransaction> Transactions { get; set; }
        public ICollection<WorkTask> Tasks { get; set; }
    }
}
