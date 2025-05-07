using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

namespace ISUMPK2.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task JoinDepartmentGroup(string departmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Department_{departmentId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveDepartmentGroup(string departmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Department_{departmentId}");
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToDepartment(string departmentId, string message)
        {
            await Clients.Group($"Department_{departmentId}").SendAsync("ReceiveMessage", message);
        }
    }
}
