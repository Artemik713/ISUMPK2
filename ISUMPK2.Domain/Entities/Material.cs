using System;
using System.Collections.Generic;

namespace ISUMPK2.Domain.Entities
{
    public class Material : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal Price { get; set; }
        public Guid? CategoryId { get; set; }
        public string Specifications { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }

        // Навигационные свойства
        public MaterialCategory Category { get; set; }
        public ICollection<ProductMaterial> ProductMaterials { get; set; }
        public ICollection<MaterialTransaction> Transactions { get; set; }
    }
}
