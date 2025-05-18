using ISUMPK2.Application.DTOs;
using ISUMPK2.Web.Models;
using System;

namespace ISUMPK2.Web.Extensions
{
    // Класс должен быть статическим для методов расширения
    public static class DtoExtensions
    {
        // Метод расширения для преобразования TaskDto в TaskModel
        // ISUMPK2.Web/Extensions/DtoExtensions.cs - измените метод ToModel для TaskDto
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
                // Удаляем назначение вычисляемым свойствам, так как они сами вычисляют свои значения
            };
            Console.WriteLine($"ToModel преобразование для задачи {dto.Id}: AssigneeId={dto.AssigneeId}, AssigneeName={dto.AssigneeName} -> {model.AssigneeName}");

            return model;
        }


        // Метод расширения для преобразования TaskCommentDto в TaskCommentModel
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

        // Метод расширения для преобразования MaterialDto в MaterialModel
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

        // Метод расширения для преобразования NotificationDto в NotificationModel
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

        // Вспомогательный метод для определения цвета статуса задачи
        private static string GetStatusColor(int statusId)
        {
            return statusId switch
            {
                1 => "Default", // Новая
                2 => "Info",    // В работе
                3 => "Info",    // Выполняется
                4 => "Warning", // На проверке
                5 => "Success", // Завершена
                6 => "Error",   // Отменена
                _ => "Default"
            };
        }

        // Вспомогательный метод для определения цвета приоритета задачи
        private static string GetPriorityColor(int priorityId)
        {
            return priorityId switch
            {
                1 => "Default", // Низкий
                2 => "Info",    // Средний
                3 => "Warning", // Высокий
                4 => "Error",   // Критический
                _ => "Default"
            };
        }
    }
}
