using System;
using System.Collections.Generic;

namespace ISUMPK2.Application.DTOs
{
    public class ProductTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ProductTypeCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal Price { get; set; }
        public List<ProductMaterialDto> Materials { get; set; } = new List<ProductMaterialDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ImageUrl { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Material { get; set; }
        public string? TechnologyMap { get; set; }
        public int? ProductionTime { get; set; }
    }

    public class ProductCreateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProductTypeId { get; set; } 
        public string UnitOfMeasure { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal Price { get; set; }
        public Guid? DepartmentId { get; set; }  // добавлено
        public decimal? Weight { get; set; }  // добавлено
        public string? Dimensions { get; set; }  // добавлено
        public string? Material { get; set; }  // добавлено
        public string? TechnologyMap { get; set; }  // добавлено
        public int? ProductionTime { get; set; }  // добавлено
        public string? ImageUrl { get; set; }  // добавлено
        public List<ProductMaterialCreateDto> Materials { get; set; }
    }

    public class ProductUpdateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProductTypeId { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Price { get; set; }
        // Добавляем новое свойство
        public decimal CurrentStock { get; set; }
        public Guid? DepartmentId { get; set; }
        public decimal? Weight { get; set; }
        public string Dimensions { get; set; }
        public string Material { get; set; }
        public string TechnologyMap { get; set; }
        public int? ProductionTime { get; set; }
        public string ImageUrl { get; set; }
        public List<ProductMaterialCreateDto> Materials { get; set; }
    }

    public class ProductMaterialDto
    {
        public Guid ProductId { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
    }


    public class ProductMaterialCreateDto
    {
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ProductTransactionDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; }
        public Guid? TaskId { get; set; }
        public string TaskTitle { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductTransactionCreateDto
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; }
        public Guid? TaskId { get; set; }
        public string Notes { get; set; }
    }
}
