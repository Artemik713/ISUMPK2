using System;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Mobile.Models;

namespace ISUMPK2.Mobile.Extensions
{
    public static class MappingExtensions
    {
        public static TaskModel ToMobileModel(this TaskDto dto)
        {
            if (dto == null)
                return null;

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
                Comments = dto.Comments?.Select(c => c.ToMobileModel()).ToList() ?? new List<TaskCommentModel>()
            };
        }

        public static TaskCommentModel ToMobileModel(this TaskCommentDto dto)
        {
            if (dto == null)
                return null;

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

        public static NotificationModel ToMobileModel(this NotificationDto dto)
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
}
