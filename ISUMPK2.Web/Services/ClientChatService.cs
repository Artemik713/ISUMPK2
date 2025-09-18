using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using System.Net.Http.Json;

namespace ISUMPK2.Web.Services
{
    public class ClientChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseEndpoint = "api/chat";

        public ClientChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ChatMessageDto> GetMessageByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_baseEndpoint}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatMessageDto>();
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesForUserAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"{_baseEndpoint}/user/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ChatMessageDto>>() ?? Enumerable.Empty<ChatMessageDto>();
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesForDepartmentAsync(Guid departmentId)
        {
            var response = await _httpClient.GetAsync($"{_baseEndpoint}/department/{departmentId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ChatMessageDto>>() ?? Enumerable.Empty<ChatMessageDto>();
        }

        public async Task<IEnumerable<ChatMessageDto>> GetConversationAsync(Guid senderId, Guid receiverId)
        {
            var response = await _httpClient.GetAsync($"{_baseEndpoint}/conversation?senderId={senderId}&receiverId={receiverId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ChatMessageDto>>() ?? Enumerable.Empty<ChatMessageDto>();
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid senderId, ChatMessageCreateDto messageDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseEndpoint}/send", new
            {
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                DepartmentId = messageDto.DepartmentId,
                Message = messageDto.Message
            });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatMessageDto>();
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var response = await _httpClient.PutAsync($"{_baseEndpoint}/{messageId}/mark-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            var response = await _httpClient.PutAsync($"{_baseEndpoint}/user/{userId}/mark-all-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetUnreadMessagesCountForUserAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"{_baseEndpoint}/user/{userId}/unread-count");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
    }
}