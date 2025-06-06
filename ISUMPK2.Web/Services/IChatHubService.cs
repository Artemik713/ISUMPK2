﻿using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public interface IChatHubService
    {
        event Action<ChatMessageModel> OnReceiveMessage;
        Task ConnectAsync();
        Task DisconnectAsync();
        Task JoinUserGroupAsync(string userId);
        Task JoinDepartmentGroupAsync(string departmentId);
        Task LeaveUserGroupAsync(string userId);
        Task LeaveDepartmentGroupAsync(string departmentId);
        Task SendMessageToUserAsync(string userId, string message);
        Task SendMessageToDepartmentAsync(string departmentId, string message);
    }
}
