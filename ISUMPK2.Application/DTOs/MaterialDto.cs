using System;
using System.Collections.Generic;

namespace ISUMPK2.Application.DTOs
{
    public class MaterialDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal Price { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Specifications { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class MaterialCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
    }
    public class MaterialCreateDto
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

    public class MaterialUpdateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal Price { get; set; }
        public decimal CurrentStock { get; set; }
    }

    public class MaterialTransactionDto
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; }
        public Guid? TaskId { get; set; }
        public string TaskTitle { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MaterialTransactionCreateDto
    {
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; }
        public Guid? TaskId { get; set; }
        public string Notes { get; set; }
    }
}
