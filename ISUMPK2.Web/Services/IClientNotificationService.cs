using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public interface IClientNotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllAsync();
        Task<IEnumerable<NotificationDto>> GetFilteredAsync(NotificationFilterDto filter);
        Task<NotificationDto> GetByIdAsync(Guid id);
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync();
        Task<int> GetUnreadCountAsync();
    }
}