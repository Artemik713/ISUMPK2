using ISUMPK2.Application.DTOs;
using ISUMPK2.Web.Models;
using System;
using System.Linq;

namespace ISUMPK2.Web.Extensions
{
    public static class DtoExtensions
    {
        // Добавляем метод расширения для UserDto
        public static UserModel ToModel(this UserDto dto)
        {
            if (dto == null) return null;

            return new UserModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DepartmentId = dto.DepartmentId,
                DepartmentName = dto.DepartmentName,
                Roles = dto.Roles ?? new List<string>(),
                CreatedAt = dto.CreatedAt,
                IsActive = dto.IsActive
            };
        }

        // Остальные методы остаются без изменений...
        public static TaskModel ToModel(this TaskDto dto)
        {
            if (dto == null) return null;

            var model = new TaskModel
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
                AssigneeName = !string.IsNullOrEmpty(dto.AssigneeName) ? dto.AssigneeName : "Не назначен",
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

            Console.WriteLine($"ToModel преобразование для задачи {dto.Id}: AssigneeId={dto.AssigneeId}, AssigneeName={dto.AssigneeName} -> {model.AssigneeName}");
            return model;
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
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                Specifications = dto.Specifications,
                Manufacturer = dto.Manufacturer,
                PartNumber = dto.PartNumber,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        public static NotificationModel ToModel(this NotificationDto dto)
        {
            if (dto == null) return null;

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

        private static string GetStatusColor(int statusId)
        {
            return statusId switch
            {
                1 => "Default",
                2 => "Info",
                3 => "Info",
                4 => "Warning",
                5 => "Success",
                6 => "Error",
                _ => "Default"
            };
        }

        private static string GetPriorityColor(int priorityId)
        {
            return priorityId switch
            {
                1 => "Default",
                2 => "Info",
                3 => "Warning",
                4 => "Error",
                _ => "Default"
            };
        }
    }
}