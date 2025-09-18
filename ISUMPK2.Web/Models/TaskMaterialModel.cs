using System;

namespace ISUMPK2.Web.Models
{
    public class TaskMaterialModel
    {
        public Guid TaskId { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; } = "шт.";
    }
}