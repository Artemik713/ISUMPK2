using System;

namespace ISUMPK2.Web.Models
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
        // Добавленные свойства
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Specifications { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Вычисляемое свойство для определения низкого запаса
        public bool IsLowStock => CurrentStock < MinimumStock;
    }

    public class MaterialCreateModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal Price { get; set; }
        public Guid? CategoryId { get; set; }
        public string Specifications { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
    }

    public class MaterialUpdateModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal Price { get; set; }
        public Guid? CategoryId { get; set; }
        public string Specifications { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
    }

    public class MaterialTransactionModel
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; } // "Receipt" или "Issue"
        public Guid? TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
