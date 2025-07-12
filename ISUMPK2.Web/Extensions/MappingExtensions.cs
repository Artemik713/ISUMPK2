// Extensions/MappingExtensions.cs
using System.Linq;
using System.Collections.Generic;
using System;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Web.Models;

public static class MappingExtensions
{
    public static TaskModel ToModel(this TaskDto dto)
    {
        if (dto == null) return null;

        return new TaskModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            StatusId = dto.StatusId,
            StatusName = dto.StatusName,
            PriorityId = dto.PriorityId,
            PriorityName = dto.PriorityName,
            CreatorId = dto.CreatorId,
            CreatorName = dto.CreatorName,
            AssigneeId = dto.AssigneeId,
            AssigneeName = dto.AssigneeName,
            DepartmentId = dto.DepartmentId,
            DepartmentName = dto.DepartmentName,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,
            CompletedDate = dto.CompletedDate,
            EstimatedHours = dto.EstimatedHours,
            ActualHours = dto.ActualHours,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            IsForceMarked = dto.IsForceMarked,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Comments = dto.Comments?.Select(c => c.ToModel()).ToList() ?? new List<TaskCommentModel>()
        };
    }

    public static TaskCommentModel ToModel(this TaskCommentDto dto)
    {
        if (dto == null) return null;

        return new TaskCommentModel
        {
            Id = dto.Id,
            TaskId = dto.TaskId,
            UserId = dto.UserId,
            UserName = dto.UserName,
            Comment = dto.Comment,
            CreatedAt = dto.CreatedAt
        };
    }

    public static DepartmentModel ToModel(this DepartmentDto dto)
    {
        if (dto == null) return null;

        return new DepartmentModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            HeadId = dto.HeadId,
            HeadName = dto.HeadName,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    public static UserModel ToModel(this UserDto dto)
    {
        if (dto == null) return null;

        return new UserModel
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            PhoneNumber = dto.PhoneNumber,
            DepartmentId = dto.DepartmentId,
            DepartmentName = dto.DepartmentName,
            Roles = dto.Roles ?? new List<string>(),
            CreatedAt = dto.CreatedAt
        };
    }
    public static MaterialModel ToModel(this MaterialDto dto)
    {
        if (dto == null) return null;

        return new MaterialModel
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            UnitOfMeasure = dto.UnitOfMeasure,
            CurrentStock = dto.CurrentStock,
            MinimumStock = dto.MinimumStock,
            Price = dto.Price,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
    public static ProductModel ToModel(this ProductDto dto)
    {
        if (dto == null)
            return null;

        return new ProductModel
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            ProductTypeId = dto.ProductTypeId,
            ProductTypeName = dto.ProductTypeName,
            UnitOfMeasure = dto.UnitOfMeasure,
            CurrentStock = dto.CurrentStock,
            Price = dto.Price,
            Materials = dto.Materials?.Select(m => m.ToModel()).ToList() ?? new List<ProductMaterialModel>(),
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    public static ProductTypeModel ToModel(this ProductTypeDto dto)
    {
        if (dto == null)
            return null;

        return new ProductTypeModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    public static ProductMaterialModel ToModel(this ProductMaterialDto dto)
    {
        if (dto == null)
            return null;

        return new ProductMaterialModel
        {
            ProductId = dto.ProductId,
            MaterialId = dto.MaterialId,
            MaterialName = dto.MaterialName,
            Quantity = dto.Quantity
        };
    }
    public static ChatMessageModel ToModel(this ChatMessageDto dto)
    {
        if (dto == null) return null;

        return new ChatMessageModel
        {
            Id = dto.Id,
            SenderId = dto.SenderId,
            SenderName = dto.SenderName,
            ReceiverId = dto.ReceiverId,
            ReceiverName = dto.ReceiverName,
            DepartmentId = dto.DepartmentId,
            DepartmentName = dto.DepartmentName,
            Message = dto.Message,
            IsRead = dto.IsRead,
            CreatedAt = dto.CreatedAt
        };
    }
    public static NotificationModel ToModel(this NotificationDto dto)
    {
        if (dto == null)
            return null;

        return new NotificationModel
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            IsRead = dto.IsRead,
            TaskId = dto.TaskId,
            TaskTitle = dto.TaskTitle,
            CreatedAt = dto.CreatedAt
        };
    }
}
