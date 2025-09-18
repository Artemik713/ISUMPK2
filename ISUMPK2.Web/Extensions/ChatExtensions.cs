using ISUMPK2.Application.DTOs;
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Extensions
{
    public static class ChatExtensions
    {
        public static ChatMessageModel ToModel(this ChatMessageDto dto)
        {
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
    }
}