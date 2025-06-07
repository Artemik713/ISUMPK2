using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Mobile.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal Price { get; set; }
        public List<ProductMaterialModel> Materials { get; set; } = new List<ProductMaterialModel>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductMaterialModel
    {
        public Guid ProductId { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
    }
}
