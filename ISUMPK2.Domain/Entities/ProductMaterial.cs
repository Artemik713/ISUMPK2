using System;

namespace ISUMPK2.Domain.Entities
{
    public class ProductMaterial
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid MaterialId { get; set; }
        public Material Material { get; set; }

        public decimal Quantity { get; set; }
    }
}
