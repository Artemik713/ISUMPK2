using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISUMPK2.Mobile.Models;

namespace ISUMPK2.Mobile.Services
{
    public interface IChatRoomService
    {
        Task<ChatRoomModel> GetChatRoomAsync(Guid chatId);
        Task<List<ChatMessageModel>> GetChatMessagesAsync(Guid chatId);
    }
}