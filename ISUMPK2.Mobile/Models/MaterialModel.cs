using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Mobile.Models
{
    public class MaterialModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal Price { get; set; }
        // Дополнительные свойства
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Specifications { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // Вычисляемое свойство для определения низкого запаса
        public bool IsLowStock { get; set; }
    }
}
